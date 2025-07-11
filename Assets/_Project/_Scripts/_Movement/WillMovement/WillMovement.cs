using UnityEngine;

/// <summary>
/// Usa fuerzas físicas para posicionar un objeto a un radio fijo alrededor del centro de un receptor,
/// moviéndose hacia la dirección de input 2D que el receptor provee (plano XZ).
/// Además ajusta la intensidad de emisión del material según la energía restante.
/// </summary>
public interface IInputProvider
{
    Vector3 GetInputDirection();
    bool GetInput();
}

public interface IEnergyRemain
{
    /// <summary>
    /// Energía normalizada en [0,1].
    /// </summary>
    float RemainEnergy();
}

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(MeshRenderer))]
public class WillMovement : MonoBehaviour
{
    [Header("Receptor e Input")]
    [SerializeField, Tooltip("Transform del receptor alrededor del cual posicionarse.")]
    private Transform receptor;

    [SerializeField, Tooltip("Componente del receptor que implementa IInputProvider.")]
    private MonoBehaviour inputProviderComponent;

    [SerializeField, Tooltip("Componente del receptor que implementa IEnergyRemain.")]
    private MonoBehaviour energyProviderComponent;

    [Header("Emisión")]
    [SerializeField, Tooltip("Color base de la emisión.")]
    private Color emissionColor = new Color32(191, 11, 0, 255);

    // Intensidad mínima y máxima
    private const float MinIntensity = 3f;
    private const float MaxIntensity = 6f;

    private IInputProvider _inputProvider;
    private IEnergyRemain _energyRemain;
    private Rigidbody _rb;
    private Material _matInstance;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        // Validaciones receptor / interfaces
        if (receptor == null ||
            !(inputProviderComponent is IInputProvider) ||
            !(energyProviderComponent is IEnergyRemain))
        {
            Debug.LogError($"[WillMovement] Configuración incorrecta en {name}.");
            enabled = false;
            return;
        }

        _inputProvider = inputProviderComponent as IInputProvider;
        _energyRemain = energyProviderComponent as IEnergyRemain;

        // Instanciamos el material para no modificar el shared de otros objetos
        var renderer = GetComponent<MeshRenderer>();
        _matInstance = renderer.material;
        // Aseguramos que la keyword de emisión está activa
        _matInstance.EnableKeyword("_EMISSION");
    }

    private void FixedUpdate()
    {
        // 1) Movimiento
        if (_inputProvider.GetInput())
        {
            Vector3 dir = _inputProvider.GetInputDirection();
            Vector3 targetPos = new Vector3(dir.x, transform.position.y, dir.z);
            _rb.MovePosition(targetPos);
        }

        // 2) Ajuste de emisión según energía
        float energyNorm = Mathf.Clamp01(_energyRemain.RemainEnergy());
        print($"Energia restante: {energyNorm}");
        float intensity = Mathf.Lerp(MinIntensity, MaxIntensity, energyNorm);
        print($"Intensidad: {intensity}");
        // Multiplicamos el color base por la intensidad
        _matInstance.SetColor("_EmissionColor", emissionColor * intensity);
    }
}












