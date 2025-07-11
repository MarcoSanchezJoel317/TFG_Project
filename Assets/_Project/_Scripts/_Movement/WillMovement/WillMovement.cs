using UnityEngine;

/// <summary>
/// Usa fuerzas físicas para posicionar un objeto a un radio fijo alrededor del centro de un receptor,
/// moviéndose hacia la dirección de input 2D que el receptor provee (plano XZ).
/// </summary>
/// 
/// <summary>
/// Interfaz para que el receptor exponga su input 2D en XZ.
/// </summary>
public interface IInputProvider
{
    Vector3 GetInputDirection();
    bool GetInput();
}


[RequireComponent(typeof(Rigidbody))]
public class WillMovement : MonoBehaviour
{
    [Header("Receptor e Input")]
    [SerializeField, Tooltip("Transform del receptor alrededor del cual posicionarse.")]
    private Transform receptor;

    [SerializeField, Tooltip("Componente del receptor que implementa IInputProvider.")]
    private MonoBehaviour inputProviderComponent;

    //[Header("Configuración de Órbita")]
    //[SerializeField, Tooltip("Radio deseado en metros desde el centro del receptor.")]
    //private float radius = 3f;

    //[SerializeField, Tooltip("Fuerza de spring para corregir la posición radial (solo eje XZ).")]
    //private float followStrength = 50f;

    private IInputProvider _inputProvider;
    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        if (receptor == null)
        {
            //Debug.LogError("OrbitalFollower: receptor no asignado.");
            enabled = false;
            return;
        }

        _inputProvider = inputProviderComponent as IInputProvider;
        if (_inputProvider == null)
        {
            //Debug.LogError("OrbitalFollower: El componente no implementa IInputProvider.");
            enabled = false;
            return;
        }
    }

    private void FixedUpdate()
    {
        if (_inputProvider.GetInput())
        {
            Vector3 move = new Vector3(_inputProvider.GetInputDirection().x, this.transform.position.y, _inputProvider.GetInputDirection().z);
            _rb.MovePosition(move);
        }
        
    }
}











