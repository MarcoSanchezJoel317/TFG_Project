using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

// Controla el movimiento físico de la Voluntad basado en el input del jugador y la orientación de la cámara.
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(WillGroundRider))] // Asegura que el GroundRider esté presente
public class WillMovement : MonoBehaviour
{
    private Rigidbody _rb;
    private WillGroundRider _groundRider;
    private Transform _mainCameraTransform; // Transform de la cámara principal cacheado

    [Header("Input Actions")]
    [SerializeField] private InputActionReference _movementInputAction;
    [SerializeField] private InputActionReference _sprintInputAction;
    [SerializeField] private InputActionReference _jumpInputAction;


    [Header("Locomotion")]
    [Space(5)]
    [Header("     Direction 📐")]    // Direcciones de movimiento
    [SerializeField] private Vector3 _unitGoal; // Dirección de movimiento unitaria deseada en espacio de mundo
    [SerializeField] private Vector2 _moveDirectionInput; // Input 2D crudo del jugador

    [Space(5)]
    [Header("     Speed 🚀")]    // Espacios para indentar
    [Space(5)]
    [Range(5f, 20f)]
    [SerializeField] private float _maxSpeed = 8f;
    private Vector3 _velGoal;        // Velocidad objetivo
    public float speedFactor = 1.0f; // Factor global de velocidad
    public Vector3 groundVel = Vector3.zero; // Velocidad heredada del suelo en movimiento

    [Space(5)]
    [Header("     Acceleration ⏩")]    // Espacios para indentar
    [Space(5)]
    [Range(100f, 300f)]
    [SerializeField] private float _acceleration = 200f;
    [SerializeField] private AnimationCurve _accelerationFactorFromDot = AnimationCurve.EaseInOut(-1, 0.1f, 1, 1);

    [Space(5)]
    [Header("     Force 💪🏻")]    // Espacios para indentar
    [Space(5)]
    [Range(100f, 300f)]
    [SerializeField] private float _maxAccelerationForce = 150f;
    [SerializeField] private AnimationCurve _maxAccelerationForceFactorFromDot = AnimationCurve.EaseInOut(-1, 0.1f, 1, 1);
    [SerializeField] private Vector3 _forceScale = new Vector3(1, 0, 1); // Escala de aplicación de fuerza por eje
    [SerializeField] private float _maxAccelForceFactor = 1.0f;

    [Space(5)]
    [Header("     Stun 💥")]    // Espacios para indentar
    [Space(5)]
    [SerializeField] private float _movementControlDisabledTimer = 0f; // Temporizador para deshabilitar el control de movimiento

    [Space(15)]
    [Header("Jump")]
    [Space(5)]
    [Tooltip("Fuerza de salto de la Voluntad")]
    [SerializeField] private float _upForce = 250f;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _groundRider = GetComponent<WillGroundRider>();

        if (_groundRider == null)
        {
            Debug.LogError("WillGroundRider script no encontrado en el mismo GameObject que WillMovement.", this);
        }

        if (Camera.main != null)
        {
            _mainCameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogWarning("Cámara principal no encontrada. Asegúrate de que tu cámara principal tiene la etiqueta 'MainCamera'.", this);
        }
    }

    private void OnEnable()
    {
        _movementInputAction?.action?.Enable();
        _sprintInputAction?.action?.Enable();
        _jumpInputAction?.action?.Enable();

        if (_jumpInputAction != null && _jumpInputAction.action != null)
        {
            _jumpInputAction.action.performed += Jump;
        }
    }

    private void OnDisable()
    {
        _movementInputAction?.action?.Disable();
        _sprintInputAction?.action?.Disable();
        _jumpInputAction?.action?.Disable();

        if (_jumpInputAction != null && _jumpInputAction.action != null)
        {
            _jumpInputAction.action.performed -= Jump;
        }
    }

    void Update()
    {
        _moveDirectionInput = _movementInputAction?.action?.ReadValue<Vector2>() ?? Vector2.zero;
    }

    private void FixedUpdate()
    {
        ProcessMovementInput(Time.fixedDeltaTime);
        // ApplyTargetFollowForce(); // Lógica de seguimiento de la oveja eliminada por ahora.
    }

    /// <summary>
    /// Procesa el input crudo, aplica efectos de aturdimiento y calcula la dirección de movimiento deseada.
    /// </summary>
    /// <param name="deltaTime">El fixedDeltaTime para las actualizaciones de física.</param>
    void ProcessMovementInput(float deltaTime)
    {
        Vector2 currentInput = _moveDirectionInput;

        if (_movementControlDisabledTimer > 0f)
        {
            currentInput = Vector2.zero;
            _movementControlDisabledTimer -= deltaTime;
        }
        else
        {
            if (currentInput.magnitude > 1.0f)
            {
                currentInput.Normalize();
            }

            _unitGoal = ConvertInputToWorldDirection(currentInput);

            CalculateMovementForces();
        }
    }

    /// <summary>
    /// Convierte el input 2D (WASD/Stick) en una dirección 3D en el espacio de mundo,
    /// alineada con la orientación de la cámara (sin considerar la inclinación vertical de la cámara).
    /// </summary>
    /// <param name="input">El vector de input 2D crudo.</param>
    /// <returns>La dirección 3D calculada en el espacio de mundo.</returns>
    private Vector3 ConvertInputToWorldDirection(Vector2 input)
    {
        if (_mainCameraTransform == null)
        {
            return new Vector3(input.x, 0f, input.y);
        }

        Vector3 cameraForward = _mainCameraTransform.forward;
        Vector3 cameraRight = _mainCameraTransform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;

        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 worldDirection = cameraForward * input.y + cameraRight * input.x;
        return worldDirection.normalized;
    }

    /// <summary>
    /// Calcula la velocidad objetivo y aplica las fuerzas de aceleración al Rigidbody.
    /// </summary>
    void CalculateMovementForces()
    {
        Vector3 unitVel = _velGoal.normalized;
        float velDot = Vector3.Dot(_unitGoal, unitVel);

        float accel = _acceleration * _accelerationFactorFromDot.Evaluate(velDot);

        Vector3 velGoal = _unitGoal * _maxSpeed * speedFactor;

        _velGoal = Vector3.MoveTowards(_velGoal, velGoal + groundVel, accel * Time.fixedDeltaTime);

        ApplyForce(_velGoal, velDot);
    }

    /// <summary>
    /// Aplica la fuerza calculada al Rigidbody basándose en la velocidad deseada y la velocidad actual.
    /// </summary>
    /// <param name="desiredVelocity">La velocidad objetivo para el frame actual.</param>
    /// <param name="velDot">Producto escalar entre la dirección de movimiento deseada y la dirección de velocidad actual.</param>
    void ApplyForce(Vector3 desiredVelocity, float velDot)
    {
        Vector3 neededAccel = (desiredVelocity - _rb.linearVelocity) / Time.fixedDeltaTime;

        float maxAccel = _maxAccelerationForce * _maxAccelerationForceFactorFromDot.Evaluate(velDot) * _maxAccelForceFactor;
        neededAccel = Vector3.ClampMagnitude(neededAccel, maxAccel);

        _rb.AddForce(Vector3.Scale(neededAccel * _rb.mass, _forceScale));
    }

    #region Lógica de Salto
    /// <summary>
    /// Aplica una fuerza de salto al Rigidbody si la Voluntad está en el suelo.
    /// </summary>
    /// <param name="callbackContext">Contexto del callback del Input System para la acción de salto.</param>
    public void Jump(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed && _groundRider != null && _groundRider.IsGrounded())
        {
            _rb.AddForce(Vector3.up * _upForce, ForceMode.Impulse);
        }
    }
    #endregion



    #region Lógica de Destrucción o Desactivación
    private void OnDestroy()
    {
        _movementInputAction?.action?.Disable();
        _sprintInputAction?.action?.Disable();
        _jumpInputAction?.action?.Disable();

        if (_jumpInputAction != null && _jumpInputAction.action != null)
        {
            _jumpInputAction.action.performed -= Jump;
        }
    }


    #endregion

    /// <summary>
    /// Dibuja Gizmos en el Editor para visualización constante.
    /// </summary>
    void OnDrawGizmos() // Cambiado de OnDrawGizmosSelected a OnDrawGizmos
    {
        // Si en el futuro se añade la lógica de seguimiento de la oveja,
        // los gizmos relacionados con ella irán aquí si se desea que sean siempre visibles.
        // Por ahora, no hay gizmos de seguimiento de oveja ya que la lógica ha sido removida.

        // Ejemplo de un gizmo básico que siempre se ve:
        // Gizmos.color = Color.magenta;
        // Gizmos.DrawSphere(transform.position, 0.2f);
    }
}