// SheepMovement.cs
using UnityEngine;

// Requiere que el GameObject tenga un Rigidbody adjunto.
[RequireComponent(typeof(Rigidbody))]
public class SheepMovement : MonoBehaviour
{
    private Rigidbody _rb;

    // Velocidad de movimiento básica para la oveja.
    [SerializeField] private float _moveSpeed = 3f;

    [Header("Rotation Settings")]
    [SerializeField] private Transform _targetWillTransform; // ¡Nueva referencia al Transform de la Voluntad!
    [SerializeField] private float _rotationSpeed = 10f; // Velocidad a la que la oveja gira para mirar a la Voluntad

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Aplicar la rotación suavemente en FixedUpdate para consistencia física.
        RotateTowardsWill();
    }

    /// <summary>
    /// Mueve la oveja en una dirección y a una velocidad dadas.
    /// Esta función es llamada desde otros scripts (ej. WillMovement).
    /// </summary>
    /// <param name="direction">La dirección normalizada en la que se moverá la oveja.</param>
    /// <param name="speedMultiplier">Un multiplicador opcional para ajustar la velocidad base.</param>
    public void MoveSheep(Vector3 direction, float speedMultiplier = 1f)
    {
        // Asegurarse de que la dirección sea unitaria y que el movimiento sea solo en el plano XZ.
        Vector3 flatDirection = new Vector3(direction.x, 0f, direction.z).normalized;

        // Calcula la velocidad objetivo.
        Vector3 targetVelocity = flatDirection * _moveSpeed * speedMultiplier;

        // Aplica la velocidad al Rigidbody.
        _rb.linearVelocity = new Vector3(targetVelocity.x, _rb.linearVelocity.y, targetVelocity.z);
    }

    /// <summary>
    /// Rota suavemente la oveja para que mire directamente a la Voluntad.
    /// </summary>
    private void RotateTowardsWill()
    {
        // Solo rotar si tenemos una referencia a la Voluntad.
        if (_targetWillTransform == null) return;

        // Calculamos la dirección desde la posición actual de la oveja hacia la posición de la Voluntad.
        // Aplanamos el vector en Y para que la oveja no "incline" su cabeza hacia arriba o abajo,
        // solo gire en el plano horizontal.
        Vector3 directionToWill = (_targetWillTransform.position - transform.position);
        directionToWill.y = 0; // Importante: aplanar la dirección para rotación horizontal

        // Si la dirección es muy pequeña (están casi en el mismo lugar), no rotar.
        if (directionToWill.magnitude < 0.01f) return;

        // Normalizamos la dirección para obtener un vector unitario.
        directionToWill.Normalize();

        // Calcula la rotación necesaria para mirar hacia esa dirección.
        // Vector3.up asegura que la oveja se mantenga "erguida".
        Quaternion targetRotation = Quaternion.LookRotation(directionToWill, Vector3.up);

        // Interpola suavemente la rotación actual del Rigidbody hacia la rotación objetivo.
        _rb.rotation = Quaternion.Slerp(_rb.rotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime);
    }

    /// <summary>
    /// Dibuja gizmos para la depuración en el Editor.
    /// </summary>
    void OnDrawGizmos()
    {
        // Dibuja una línea de la oveja a la Voluntad para visualizar la dirección de mirada.
        if (_targetWillTransform != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, _targetWillTransform.position);
            Gizmos.DrawSphere(_targetWillTransform.position, 0.1f); // Marca la posición de la voluntad
        }
    }
}
