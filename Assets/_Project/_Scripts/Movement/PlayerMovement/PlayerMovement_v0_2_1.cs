using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction; // Para usar CallbackContext directamente

// Este script necesita un Rigidbody en el mismo GameObject.
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement_v0_2_1 : MonoBehaviour
{
    // --- Componentes Necesarios ---
    private Rigidbody _rb; // Referencia al Rigidbody de la oveja (se asigna en Awake).

    // --- Input System ---
    [Header("Input Actions")]
    [Tooltip("Asigna la Input Action Reference para el movimiento.")]
    [SerializeField] private InputActionReference _movementInputAction; // La acción de input para el movimiento.

    // Input leído del jugador (Vector2 XY).
    private Vector2 _moveDirectionInput;


    // --- Referencias de las Entidades del Sistema de Movimiento ---
    [Header("Entidades del sistema de movimiento")]
    [Tooltip("Asigna el Transform de la entidad que representa la Voluntad del jugador (el objetivo que la oveja persigue).")]
    [SerializeField] private Transform _willSphereTarget; // El punto al que la oveja intenta llegar.


    // --- Referencia a la Cámara para Input Relativo ---
    [Header("Cámara")]
    [Tooltip("Asigna el Transform de la cámara o el objeto que define la dirección 'adelante' para el input.")]
    [SerializeField] private Transform _cameraTransform; // Usaremos su forward/right para el input relativo.


    // --- Parámetros de la Entidad de la Voluntad (Cómo se comporta el objetivo) ---
    [Header("Parámetros de la Voluntad")]
    [Tooltip("Distancia a la que la Voluntad intentará mantenerse de la oveja en la dirección del input.")]
    [SerializeField] private float _willSphereDistance = 3.0f;

    [Tooltip("Altura a la que la Voluntad se mantendrá respecto a la base de la oveja.")]
    [SerializeField] private float _willSphereHeightOffset = 1.0f; // Offset en Y.

    [Tooltip("Factor de suavizado para el movimiento de la Voluntad hacia su posición objetivo (0 = sin suavizado, 1 = instantáneo).")]
    [Range(0f, 1f)]
    [SerializeField] private float _willSphereMoveSmoothness = 0.1f; // Suaviza el movimiento de la Voluntad.


    // --- Parámetros de Persecución (Cómo persigue la oveja al objetivo) ---
    [Header("Parámetros de la Oveja (Persecución)")]
    [Tooltip("Velocidad máxima lineal que la oveja intenta alcanzar al perseguir a la Voluntad.")]
    [SerializeField] private float _maxChaseSpeed = 8f;

    [Tooltip("Distancia mínima horizontal a la Voluntad a la que la oveja deja de moverse (velocidad objetivo 0).")]
    [SerializeField] private float _minChaseDistance = 0.5f;

    [Tooltip("Distancia horizontal a la Voluntad a la que la oveja alcanza su velocidad máxima de persecución.")]
    [SerializeField] private float _maxChaseDistance = 5.0f;

    [Tooltip("Determina cuán agresivamente la oveja persigue a la Voluntad. Valores más altos persiguen más rápido pero pueden causar inestabilidad.")]
    [SerializeField] private float _chaseStrength = 150f; // Factor de fuerza/respuesta de la persecución.

    [Tooltip("Magnitud de velocidad lineal por debajo de la cual la oveja se detiene completamente si está dentro de la distancia mínima.")]
    [SerializeField] private float _stopVelocityThreshold = 0.1f; // Velocidad mínima para considerarse detenido.


    // --- Parámetros de Rotación (Cómo gira el morro de la oveja) ---
    [Header("Parámetros de Rotación")]
    [Tooltip("Velocidad de rotación del morro de la oveja en grados por segundo para mirar hacia la Voluntad.")]
    [SerializeField] private float _rotationSpeed = 180f;


    // --- Estado de Mecánicas Adicionales (se añadirán en versiones posteriores) ---
    private float _movementControlDisabledTimer = 0f; // Stun/Aturdimiento (v0.2.3).
    // private bool _isGrounded = false; // Detección de suelo (v0.2.x).
    // private float _upForce = 250f; // Fuerza de salto (v0.2.2).


    void Awake()
    {
        // Obtener la referencia al Rigidbody al inicio.
        _rb = GetComponent<Rigidbody>();

        // Congelar la rotación del Rigidbody para que sea controlada completamente por este script.
        _rb.freezeRotation = true;
    }

    // OnEnable se llama cuando el objeto se activa (al inicio y al reactivarse).
    private void OnEnable()
    {
        // Habilitar la Input Action si está asignada para que empiece a leer input.
        if (_movementInputAction != null && _movementInputAction.action != null)
        {
            _movementInputAction.action.Enable();
        }
        else
        {
            Debug.LogError("Movement Input Action Reference no está asignada o no es válida en OnEnable.", this);
        }
    }

    // OnDisable se llama cuando el objeto se desactiva.
    private void OnDisable()
    {
        // Deshabilitar la Input Action para dejar de leer input cuando el objeto no está activo.
        if (_movementInputAction != null && _movementInputAction.action != null)
        {
            _movementInputAction.action.Disable();
        }
    }

    // OnDestroy se llama cuando el GameObject es destruido.
    private void OnDestroy()
    {
        // Deshabilitar la Input Action al destruir el objeto para limpiar referencias.
        if (_movementInputAction != null && _movementInputAction.action != null)
        {
            _movementInputAction.action.Disable();
        }
    }


    // Update se llama una vez por frame. Es el lugar estándar para leer Input.
    void Update()
    {
        // Guarda el input 2D leído del sistema de input.
        if (_movementInputAction != null && _movementInputAction.action != null)
        {
            _moveDirectionInput = _movementInputAction.action.ReadValue<Vector2>();
        }

        // Lógica para el Stun/Aturdimiento (se añadirá en v0.2.3).
        if (_movementControlDisabledTimer > 0f)
        {
            _movementControlDisabledTimer -= Time.deltaTime; // Decrementar con Time.deltaTime en Update.
            // Nota: el input efectivo se anula en FixedUpdate para asegurar coherencia con física.
        }

        // Lógica de Salto (Input Action para el salto se manejará aquí en Update en v0.2.2).
        // Por ahora solo existe la variable _upForce.
    }


    // FixedUpdate se ejecuta a intervalos de tiempo fijos y es donde se debe manejar la física del Rigidbody.
    void FixedUpdate()
    {
        // --- Procesar Input y Posicionar la Entidad de la Voluntad ---

        Vector2 processedInput = _moveDirectionInput; // Empezamos con el input leído.

        // Lógica para anular el input si la oveja está aturdida (v0.2.3).
        if (_movementControlDisabledTimer > 0f)
        {
            processedInput = Vector2.zero; // Anular el input efectivo para el movimiento.
        }

        // Asegurarse de que tenemos las referencias necesarias para el movimiento.
        if (_rb == null || _willSphereTarget == null || _cameraTransform == null)
        {
            // Si falta algo esencial, no hacemos nada este paso físico.
            // Debug.LogWarning("Rigidbody, Will Sphere Target, o Camera Transform no asignados.", this);
            return;
        }

        // Calcular la dirección de input en el mundo, relativa a la cámara.
        Vector3 cameraRelativeInputDirection = CalculateCameraRelativeDirection(processedInput);

        // Calcular la posición objetivo a la que debería ir la entidad de la Voluntad.
        Vector3 targetWillPosition;

        // Si hay input de movimiento (magnitud > 0.01f para evitar jitter con joysticks ligeramente descentrados).
        if (processedInput.magnitude > 0.01f)
        {
            // La posición objetivo de la Voluntad es la posición de la oveja +
            // la dirección del input relativa a la cámara (normalizada) * la distancia deseada.
            targetWillPosition = transform.position + cameraRelativeInputDirection.normalized * _willSphereDistance;

            // Forzar la altura Y del target de la Voluntad a la altura de la oveja + offset.
            targetWillPosition.y = transform.position.y + _willSphereHeightOffset;
        }
        else
        {
            // Si no hay input, la Voluntad se queda justo encima de la oveja a la altura deseada.
            targetWillPosition = new Vector3(transform.position.x, transform.position.y + _willSphereHeightOffset, transform.position.z);

            // Opcional: Podrías hacer que la Voluntad "flote" suavemente de vuelta si no hay input
            // targetWillPosition = Vector3.Lerp(_willSphereTarget.position, new Vector3(transform.position.x, transform.position.y + _willSphereHeightOffset, transform.position.z), _willSphereMoveSmoothness * Time.fixedDeltaTime * 10f); // Ejemplo de regreso suave
        }


        // Mover (interpolar) la posición actual de la entidad de la Voluntad hacia su posición objetivo.
        // Usamos Lerp con un factor para un movimiento suave y gomoso de la Voluntad.
        _willSphereTarget.position = Vector3.Lerp(_willSphereTarget.position, targetWillPosition, _willSphereMoveSmoothness);


        // --- Lógica de Persecución del Objetivo de la Voluntad (por la oveja Rigidbody) ---

        // Calcular el vector desde la posición actual de la oveja hasta la posición ACTUAL de la Voluntad.
        Vector3 vectorToWillTarget = _willSphereTarget.position - transform.position;

        // Calcular la distancia 3D completa al target de la Voluntad.
        // float distanceToWillTarget = vectorToWillTarget.magnitude; // No se usa directamente para velocidad en esta versión.

        // Calcular el vector y la distancia SOLO en el plano horizontal (XZ) hasta la Voluntad.
        Vector3 vectorToWillTargetHorizontal = new Vector3(vectorToWillTarget.x, 0f, vectorToWillTarget.z);
        float distanceToWillTargetHorizontal = vectorToWillTargetHorizontal.magnitude;

        // Calcular la dirección de persecución horizontal hacia la Voluntad.
        Vector3 chaseDirectionHorizontal = Vector3.zero;
        // Solo normalizar si hay una distancia horizontal apreciable hacia la Voluntad.
        if (distanceToWillTargetHorizontal > 0.01f)
        {
            chaseDirectionHorizontal = vectorToWillTargetHorizontal.normalized;
        }

        // --- Lógica de Movimiento Horizontal de la Oveja ---

        // Calcular la velocidad objetivo lineal (magnitud) para la oveja basada en la DISTANCIA HORIZONTAL a la Voluntad.
        float targetSpeedThisStep = 0f;

        // Solo calcular velocidad si la distancia horizontal a la Voluntad es mayor que la distancia mínima para detenerse.
        if (distanceToWillTargetHorizontal > _minChaseDistance)
        {
            targetSpeedThisStep = Mathf.Lerp(0f, _maxChaseSpeed,
                Mathf.InverseLerp(_minChaseDistance, _maxChaseDistance, distanceToWillTargetHorizontal));

            targetSpeedThisStep = Mathf.Clamp(targetSpeedThisStep, 0f, _maxChaseSpeed);
        }


        // Calcular el vector de velocidad deseada para la oveja (dirección horizontal * velocidad objetivo).
        Vector3 desiredSheepVelocity = chaseDirectionHorizontal * targetSpeedThisStep;


        // --- Lógica de Detención y Estabilidad de la Oveja ---

        Vector3 currentSheepVelocity = _rb.linearVelocity; // La velocidad lineal actual del Rigidbody.
        Vector3 forceToApply = Vector3.zero; // Inicializamos la fuerza a aplicar a cero.


        // Condición para forzar la detención de la oveja:
        // Si la distancia horizontal a la Voluntad es MENOR O IGUAL que la distancia mínima
        // Y la velocidad actual de la oveja es muy baja...
        if (distanceToWillTargetHorizontal <= _minChaseDistance && currentSheepVelocity.magnitude < _stopVelocityThreshold)
        {
            // ... forzamos la velocidad del Rigidbody a cero para evitar el temblor.
            _rb.linearVelocity = Vector3.zero;
            // La fuerza se mantiene en Vector3.zero inicializado.
        }
        else
        {
            // Si no estamos en la condición de detención forzada, calculamos la fuerza normal de persecución.
            Vector3 velocityError = desiredSheepVelocity - currentSheepVelocity; // La diferencia entre lo que queremos y lo que tenemos.

            // Calcular la fuerza proporcional al error de velocidad y a la "agresividad".
            forceToApply = velocityError * _chaseStrength * _rb.mass;
        }


        // Aplicar la fuerza horizontal calculada al Rigidbody de la oveja.
        _rb.AddForce(forceToApply, ForceMode.Force);

        // Nota: La gravedad ya está haciendo su trabajo en el eje Y si Use Gravity está activado en el Rigidbody.
        // Lógica de manejo específico del eje Y (salto, pegarse al suelo) vendrá después.


        // --- Lógica de Rotación (Girar el Morro de la Oveja) ---

        // Calcular la rotación deseada para que la oveja mire horizontalmente hacia el objetivo de la Voluntad.
        Quaternion targetRotation = _rb.rotation; // Mantener rotación actual por defecto.
        // Solo calcular una nueva rotación si hay una dirección horizontal válida hacia el target de la Voluntad.
        if (chaseDirectionHorizontal.magnitude > 0.01f)
        {
            targetRotation = Quaternion.LookRotation(chaseDirectionHorizontal);
        }

        // Rotar suavemente el Rigidbody de la oveja hacia la rotación deseada.
        _rb.rotation = Quaternion.RotateTowards(_rb.rotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime);


        // --- Comentarios sobre Configuración de Componentes ---
        // Asegúrate de que el GameObject de la oveja tenga un Rigidbody (RequireComponent, Awake).
        // Añade un Collider principal (ej: Capsule Collider horizontal) al Rigidbody.
        // Los colliders o detección para las patas (para IK, pisar suelo) van en script separado.
        // La rotación del Rigidbody está congelada en Awake.
        // Crea un GameObject 'WillTarget' (esfera visible para debug) y asigna su Transform a _willSphereTarget.
        // Crea un GameObject 'CameraContainer' o usa el Transform de tu Cinemachine FreeLook,
        // o la cámara activa (Camera.main.transform) y asigna su Transform a _cameraTransform.



    }

    /// <summary>
    /// Convierte el input 2D (ej: joystick) a una dirección 3D en el mundo relativa a la cámara.
    /// </summary>
    /// <param name="inputVector">El vector de input 2D (X, Y).</param>
    /// <returns>Un vector 3D en el mundo indicando la dirección deseada, con Y=0.</returns>
    private Vector3 CalculateCameraRelativeDirection(Vector2 inputVector)
    {
        if (_cameraTransform == null || inputVector.magnitude < 0.01f) // Si no hay input o cámara asignada
        {
            return Vector3.zero; // No hay dirección de movimiento deseada.
        }

        // Obtener la dirección forward y right de la cámara.
        Vector3 cameraForward = _cameraTransform.forward;
        Vector3 cameraRight = _cameraTransform.right;

        // Eliminar la componente vertical (Y) para mantener el movimiento horizontal.
        cameraForward.y = 0f;
        cameraRight.y = 0f;

        // Normalizar los vectores después de eliminar la Y (importante si la cámara está muy inclinada).
        // Usar una pequeña tolerancia para evitar normalizar vectores casi cero.
        if (cameraForward.magnitude > 0.01f) cameraForward.Normalize(); else cameraForward = Vector3.forward;
        if (cameraRight.magnitude > 0.01f) cameraRight.Normalize(); else cameraRight = Vector3.right;


        // Combinar los vectores de la cámara con el input del jugador.
        // inputVector.y controla cuánto nos movemos "adelante" respecto a la cámara.
        // inputVector.x controla cuánto nos movemos "a la derecha" respecto a la cámara.
        Vector3 desiredDirection = (cameraForward * inputVector.y + cameraRight * inputVector.x);

        // Normalizar la dirección resultante si tiene magnitud (y si el input ya estaba normalizado, el resultado debería ser <= 1).
        // Esto asegura que la "fuerza" total del input diagonal no sea mayor que la de un solo eje.
        if (desiredDirection.magnitude > 1.0f)
        {
            desiredDirection.Normalize();
        }

        return desiredDirection;
    }

    
}
