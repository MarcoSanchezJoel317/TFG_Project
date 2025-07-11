using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Mueve un cuerpo rígido según input y ajusta la intensidad de emisión
/// pasándosela directamente a la propiedad float de tu shader (2–10).
/// </summary>
public interface IInputProvider
{
    Vector3 GetInputDirection();
    bool GetInput();
}

public interface IEnergyRemain
{
    float RemainEnergy();  // [0,1]
}

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(MeshRenderer))]
public class WillMovement : MonoBehaviour
{
    [Header("Receptor e Input")]
    [SerializeField] private Transform receptor;
    [SerializeField] private MonoBehaviour inputProviderComponent;
    [SerializeField] private MonoBehaviour energyProviderComponent;

    [Header("Emisión (Shader Graph)")]
    [SerializeField] private float MinIntensity = 2f;
    [SerializeField] private float MaxIntensity = 10f;

    private IInputProvider _inputProvider;
    private IEnergyRemain _energyRemain;
    private Rigidbody _rb;
    private Material _matInstance;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();

        if (receptor == null ||
            !(inputProviderComponent is IInputProvider) ||
            !(energyProviderComponent is IEnergyRemain))
        {
            Debug.LogError($"[WillMovement] Configuración incorrecta en {name}");
            enabled = false;
            return;
        }

        _inputProvider = (IInputProvider)inputProviderComponent;
        _energyRemain = (IEnergyRemain)energyProviderComponent;

        var rend = GetComponent<MeshRenderer>();
        _matInstance = rend.material;
        _matInstance.EnableKeyword("_EMISSION");

        // Debug inicial
        Debug.Log($"[WillMovement] Material instanciado: {_matInstance.name}");
        float initialValue = _matInstance.GetFloat("_EmissionIntensity");
        Debug.Log($"[WillMovement] Valor inicial _EmissionIntensity = {initialValue:F3}");
    }

    private void FixedUpdate()
    {
        // Movimiento
        if (_inputProvider.GetInput())
        {
            var dir = _inputProvider.GetInputDirection();
            var target = new Vector3(dir.x, transform.position.y, dir.z);
            _rb.MovePosition(target);
        }

        // Actualización de emisión
        UpdateEmission();
    }

    /// <summary>
    /// Calcula y aplica la intensidad de emisión según la energía restante,
    /// y emite logs para debug.
    /// </summary>
    private void UpdateEmission()
    {
        float energyNorm = Mathf.Clamp01(_energyRemain.RemainEnergy());
        float intensity = Mathf.Lerp(MinIntensity, MaxIntensity, energyNorm);

        _matInstance.SetFloat("_EmissionIntensity", intensity);

        float shaderValue = _matInstance.GetFloat("_EmissionIntensity");
        Debug.Log($"[WillMovement] EnergiaNorm={energyNorm:F3} | IntensityCalc={intensity:F3} | Shader['_EmissionIntensity']={shaderValue:F3}");
    }
}















