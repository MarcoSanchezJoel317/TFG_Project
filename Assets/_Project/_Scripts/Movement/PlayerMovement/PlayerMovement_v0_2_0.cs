using UnityEngine;
using UnityEngine.InputSystem; // Necesario para tipos de input, aunque no se lea aquí en v0.2.0
// using static UnityEngine.InputSystem.InputAction; // No se usa estáticamente en esta versión

// Este script necesita un Rigidbody en el mismo GameObject para aplicar fuerzas.
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement_v0_2_0 : MonoBehaviour
{
    // --- Componentes Necesarios ---
    private Rigidbody _rb; // Referencia al Rigidbody de la oveja (se asigna en Awake)

    // --- Referencia a la Entidad Controlada por el Jugador ---
    [Header("Sistema de Seguimiento")]
    [Tooltip("Asigna el Transform de la entidad que representa la Voluntad del jugador (el objetivo que la oveja persigue).")]
    [SerializeField] private Transform _willSphereTarget; // El punto al que la oveja intenta llegar

    // --- Parámetros de Persecución (Cómo persigue la oveja al objetivo) ---
    [Tooltip("Velocidad máxima lineal que la oveja intenta alcanzar al perseguir a la Voluntad.")]
    [SerializeField] private float _maxChaseSpeed = 8f;

    [Tooltip("Distancia mínima a la Voluntad a la que la oveja deja de moverse (velocidad objetivo 0).")]
    [SerializeField] private float _minChaseDistance = 0.5f;

    [Tooltip("Distancia a la Voluntad a la que la oveja alcanza su velocidad máxima (_maxChaseSpeed).")]
    [SerializeField] private float _maxChaseDistance = 5.0f;

    [Tooltip("Determina cuán agresivamente la oveja persigue a la Voluntad. Valores más altos persiguen más rápido pero pueden causar inestabilidad.")]
    [SerializeField] private float _chaseStrength = 15f; // Factor de fuerza/respuesta de la persecución

    // --- Parámetros de Rotación (Cómo gira el morro de la oveja) ---
    [Header("Parámetros de Rotación ↩️")]
    [Tooltip("Velocidad de rotación del morro de la oveja en grados por segundo para mirar hacia la Voluntad.")]
    [SerializeField] private float _rotationSpeed = 260f;

    // Añade esta variable serializada en la sección de parámetros de Persecución/Velocidad:
    [Tooltip("Magnitud de velocidad lineal por debajo de la cual la oveja se detiene completamente si está dentro de la distancia mínima.")]
    [SerializeField] private float _stopVelocityThreshold = 0.1f; // Velocidad mínima para considerarse detenido


    void Awake()
    {
        // Obtener la referencia al Rigidbody al inicio.
        _rb = GetComponent<Rigidbody>();

        // Congelar la rotación del Rigidbody para que sea controlada completamente por este script.
        _rb.freezeRotation = true;
    }

    // FixedUpdate se ejecuta a intervalos de tiempo fijos y es donde se debe manejar la física del Rigidbody.
    // ... (variables y Awake) ...

    void FixedUpdate()
    {
        if (_rb == null || _willSphereTarget == null)
        {
            return;
        }

        // --- Lógica de Persecución del Objetivo de la Voluntad ---

        // Calcular el vector COMPLETO 3D desde la oveja hasta el objetivo.
        Vector3 vectorToTarget = _willSphereTarget.position - transform.position;

        // Calcular la distancia 3D completa (necesaria para algunas lógicas, aunque no para la velocidad).
        float distanceToTarget = vectorToTarget.magnitude;

        // Calcular el vector y la distancia SOLO en el plano horizontal (XZ).
        Vector3 vectorToTargetHorizontal = new Vector3(vectorToTarget.x, 0f, vectorToTarget.z);
        float distanceToTargetHorizontal = vectorToTargetHorizontal.magnitude;


        // Calcular la dirección de persecución horizontal.
        Vector3 chaseDirectionHorizontal = Vector3.zero;
        // Solo normalizar si hay una distancia horizontal apreciable para evitar problemas.
        if (distanceToTargetHorizontal > 0.01f) // Usar una pequeña tolerancia
        {
            chaseDirectionHorizontal = vectorToTargetHorizontal.normalized;
        }
        // Si no hay distancia horizontal, chaseDirectionHorizontal se mantiene en Vector3.zero.


        // --- Lógica de Movimiento Horizontal ---

        // Calcular la velocidad objetivo lineal (magnitud) basada en la DISTANCIA HORIZONTAL.
        float targetSpeedThisStep = 0f; // La velocidad objetivo por defecto es cero.

        // Solo calcular velocidad si la distancia horizontal al target es mayor que la distancia mínima para detenerse.
        // Nota: Usamos distanceToTargetHorizontal > 0.01f en lugar de > _minChaseDistance para evitar problemas cuando
        // _minChaseDistance es muy pequeño y InverseLerp se vuelve inestable cerca de cero. La lógica de detenerse
        // se manejará explícitamente más abajo con el _minChaseDistance y _stopVelocityThreshold.
        if (distanceToTargetHorizontal > 0.01f)
        {
            // Usar InverseLerp con la DISTANCIA HORIZONTAL para mapear el rango a [0, 1].
            // Lerp para obtener la velocidad objetivo entre 0 y _maxChaseSpeed.
            targetSpeedThisStep = Mathf.Lerp(0f, _maxChaseSpeed,
                Mathf.InverseLerp(_minChaseDistance, _maxChaseDistance, distanceToTargetHorizontal));

            // Clamp la velocidad objetivo para asegurar que no exceda _maxChaseSpeed.
            targetSpeedThisStep = Mathf.Clamp(targetSpeedThisStep, 0f, _maxChaseSpeed);
        }
        // Si distanceToTargetHorizontal <= 0.01f, targetSpeedThisStep se mantiene en 0.


        // Calcular el vector de velocidad deseada (dirección horizontal * velocidad objetivo).
        Vector3 desiredVelocity = chaseDirectionHorizontal * targetSpeedThisStep;


        // --- NUEVO: Lógica de Detención y Estabilidad ---




        Vector3 currentVelocity = _rb.linearVelocity; // La velocidad lineal actual del Rigidbody.
        Vector3 forceToApply = Vector3.zero; // Inicializamos la fuerza a aplicar a cero.


        // Condición para forzar la detención:
        // Si la distancia horizontal al target es MENOR O IGUAL que la distancia mínima
        // Y la velocidad actual del Rigidbody es muy baja...
        if (distanceToTargetHorizontal <= _minChaseDistance && currentVelocity.magnitude < _stopVelocityThreshold)
        {
            // ... forzamos la velocidad a cero para evitar el temblor.
            _rb.linearVelocity = Vector3.zero;
            forceToApply = Vector3.zero; // No aplicar fuerza en este paso.
                                         // Opcional: Puedes añadir un Debug.Log("Forzando detención") para ver cuándo ocurre.
        }
        else
        {
            // Si no estamos en la condición de detención forzada, calculamos la fuerza normal de persecución.
            Vector3 velocityError = desiredVelocity - currentVelocity; // La diferencia entre lo que queremos y lo que tenemos.

            // Calcular la fuerza proporcional al error de velocidad y a la "agresividad".
            forceToApply = velocityError * _chaseStrength * _rb.mass;
        }


        // Aplicar la fuerza horizontal calculada al Rigidbody de la oveja (será cero si forzamos la detención).
        _rb.AddForce(forceToApply, ForceMode.Force);

        // Nota: La gravedad ya está haciendo su trabajo en el eje Y.
        // Lógica de manejo específico del eje Y (salto, pegarse al suelo) vendrá después y añadirá/modificará fuerzas Y.


        // --- Lógica de Rotación (Girar el Morro para Mirar al Objetivo) ---

        // Calcular la rotación deseada para que la oveja mire horizontalmente hacia el objetivo de la Voluntad.
        Quaternion targetRotation = _rb.rotation; // Mantener rotación actual por defecto.
                                                  // Solo calcular una nueva rotación si hay una dirección horizontal válida hacia el target.
        if (chaseDirectionHorizontal.magnitude > 0.01f) // Usar la misma tolerancia que antes.
        {
            // Crea una rotación que apunta en la dirección horizontal hacia el target.
            targetRotation = Quaternion.LookRotation(chaseDirectionHorizontal);
        }

        // Rotar suavemente el Rigidbody hacia la rotación deseada.
        _rb.rotation = Quaternion.RotateTowards(_rb.rotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime);



    }
    
}
