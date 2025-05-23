using System;
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

    // Referencia a la Oveja
    [Header("Sheep Control")] // Nueva sección para la oveja
    [SerializeField] private SheepMovement _targetSheepMovement; // Referencia al script SheepMovement de la oveja
    [SerializeField] private float _sheepMoveSpeedFactor = 0.5f; // Factor para la velocidad de la oveja respecto a la Voluntad

    [Header("Locomotion")]
    [Space(5)]
    [Header("     Direction 📐")]    // Direcciones de movimiento
    [SerializeField] private Vector3 _unitGoal; // Dirección de movimiento unitaria deseada en espacio de mundo
    [SerializeField] private Vector2 _moveDirectionInput; // Input 2D crudo del jugador

    [Space(5)]
    [Header("     Speed 🚀")]    
    [Space(5)]
    [Range(5f, 20f)]
    [SerializeField] private float _maxSpeed = 8f;
    private Vector3 _velGoal;        // Velocidad objetivo
    public float speedFactor = 1.0f; // Factor global de velocidad
    public Vector3 groundVel = Vector3.zero; // Velocidad heredada del suelo en movimiento

    [Space(5)]
    [Header("     Acceleration ⏩")]    
    [Space(5)]
    [Range(0f, 50f)]
    [SerializeField] private float _acceleration = 8f;
    [SerializeField] private AnimationCurve _accelerationFactorFromDot = AnimationCurve.EaseInOut(-1, 1f, 1, 1f);

    [Space(5)]
    [Header("     Force 💪🏻")]    
    [Space(5)]
    [Range(0f, 300)]
    [SerializeField] private float _maxAccelerationForce = 80f;
    [SerializeField] private AnimationCurve _maxAccelerationForceFactorFromDot = AnimationCurve.EaseInOut(-1, 1f, 1, 1f);
    [SerializeField] private Vector3 _forceScale = new Vector3(1, 0, 1); // Escala de aplicación de fuerza por eje
    [SerializeField] private float _maxAccelForceFactor = 1.0f;

    [Space(5)]
    [Header("     Stun 💥")]    
    [Space(5)]
    [SerializeField] private float _movementControlDisabledTimer = 0f; // Temporizador para deshabilitar el control de movimiento

    [Space(15)]
    [Header("Jump")]
    [Space(5)]
    [Tooltip("Fuerza de salto de la Voluntad")]
    [SerializeField] private float _upForce = 250f;


    #region Metodos de inicio
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (!TryGetComponent<WillGroundRider>(out _groundRider))
        {
            Debug.LogError("WillGroundRider no encontrado en " + name, this);
        }

        if ((_mainCameraTransform = Camera.main?.transform) == null)
        {
            Debug.LogWarning("No se encontró cámara con tag 'MainCamera'", this);
        }
    }
    #endregion


    #region Metodos de activacion/desactivacion
    // Helper local dentro de la misma clase
    private void ToggleAction(InputActionReference inputRef, bool enable, Action<InputAction.CallbackContext> callback = null)
    {
        var action = inputRef?.action;
        if (action == null) return;

        if (enable)
        {
            action.Enable();
            if (callback != null) action.performed += callback;
        }
        else
        {
            if (callback != null) action.performed -= callback;
            action.Disable();
        }
    }

    private void OnEnable()
    {
        ToggleAction(_movementInputAction, true);
        ToggleAction(_sprintInputAction, true);
        ToggleAction(_jumpInputAction, true, Jump);
    }

    private void OnDisable()
    {
        ToggleAction(_movementInputAction, false);
        ToggleAction(_sprintInputAction, false);
        ToggleAction(_jumpInputAction, false, Jump);
    }
    #endregion


    #region Metodos update y fixedupdate
    void Update()
    {
        _moveDirectionInput = _movementInputAction?.action?.ReadValue<Vector2>() ?? Vector2.zero;
    }

    private void FixedUpdate()
    {
        ProcessMovementInput(Time.fixedDeltaTime);
    }

    #endregion


    
    /// Procesa el input crudo, aplica efectos de aturdimiento y calcula la dirección de movimiento deseada.
    
    void ProcessMovementInput(float deltaTime)
    {
        Vector2 currentInput = _moveDirectionInput;     //Goje el input. EJM:(0.5, 1) PULSAR JOYSTICK ARRIBA DERECHA

        if (_movementControlDisabledTimer > 0f)         //Si esta stuneado, el curreninput es 0
        {
            currentInput = Vector2.zero;
            _movementControlDisabledTimer -= deltaTime;
        }
        else
        {
            if (currentInput.magnitude > 1.0f)          //Si se mueve en diagonal, lo normaliza
            {
                currentInput.Normalize();
            }

            _unitGoal = ConvertInputToWorldDirection(currentInput);     //Lo convierte a coordenadas 3D del mundo

            CalculateMovementForces();                                  //Ajusta la velocidad hacia la zona que ha de moverse

            // Lógica para mover a la oveja cuando la Voluntad se mueve
            if (_targetSheepMovement != null && currentInput.magnitude > 0.05f) // Pequeño umbral para evitar movimiento por ruido
            {
                // La oveja se mueve en la misma dirección que la Voluntad, pero con su propio factor de velocidad.
                _targetSheepMovement.MoveSheep(_unitGoal, _sheepMoveSpeedFactor);
            }
            else if (_targetSheepMovement != null)
            {
                // Si no hay input de movimiento de la Voluntad, la oveja debería detenerse.
                _targetSheepMovement.MoveSheep(Vector3.zero);
            }
        }
    }


    // Convierte el input 2D (WASD/Stick) en una dirección 3D en el espacio de mundo,
    // alineada con la orientación de la cámara (sin considerar la inclinación vertical de la cámara).

    private Vector3 ConvertInputToWorldDirection(Vector2 input)
    {
        // Si no hay cámara, mapear input X→X, Y→Z
        if (_mainCameraTransform == null)
        {
            return new Vector3(input.x, 0f, input.y);
        }

        // Obtener ejes de cámara
        Vector3 forward = _mainCameraTransform.forward;
        Vector3 right = _mainCameraTransform.right;

        // Eliminar inclinación vertical
        forward.y = 0;
        right.y = 0;

        // Calcular dirección mundo y normalizar
        Vector3 dir = forward * input.y + right * input.x;
        return dir.normalized;
    }



    // Calcula la velocidad objetivo y aplica las fuerzas de aceleración al Rigidbody.
    void CalculateMovementForces()
    {
        Vector3 unitVel = _velGoal.normalized;              //Direccion de la velocidad actual

        float velDot = Vector3.Dot(_unitGoal, unitVel);     //Producto escalar entre las direcciones de nueva velocidad y la velocidad actual

        float accelGoal = _acceleration * _accelerationFactorFromDot.Evaluate(velDot);  //Aceleracion a la que puede moverse, dependiendo de la direccion de la velocidad

        Vector3 velGoal = _unitGoal * _maxSpeed * speedFactor;                      //Vector que calcula la velocidad a la direccion deseada, además de multiplicar por el speedfactor(para buffs y tal)

        _velGoal = Vector3.MoveTowards(_velGoal, velGoal + groundVel, accelGoal * Time.fixedDeltaTime);     //Vamos desde nuestra velocidad actual hasta la velocidad que buscamos ahora poco a poco, a lo que la aceleracion permite. El groundvel es por si hay lodo, hielo, etc

        ApplyForce(_velGoal, velDot);
    }

   
    /// Aplica la fuerza calculada al Rigidbody basándose en la velocidad deseada y la velocidad actual.

    /// <param name="desiredVelocity">La velocidad objetivo para el frame actual.</param>
    /// <param name="velDot">Producto escalar entre la dirección de movimiento deseada y la dirección de velocidad actual.</param>
    void ApplyForce(Vector3 desiredVelocity, float velDot)
    {
        
        Vector3 neededAccel = (desiredVelocity - _rb.linearVelocity) / Time.fixedDeltaTime;     // Calcula la aceleración necesaria en el frame fisico actual

        float maxAccel = _maxAccelerationForce * _maxAccelerationForceFactorFromDot.Evaluate(velDot) * _maxAccelForceFactor;
        
        neededAccel = Vector3.ClampMagnitude(neededAccel, maxAccel);

        _rb.AddForce(Vector3.Scale(neededAccel * _rb.mass, _forceScale));
    }

    #region Lógica de Salto
    
    // Aplica una fuerza de salto al Rigidbody si la Voluntad está en el suelo.  
    public void Jump(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed && _groundRider != null && _groundRider.IsGrounded())
        {
            _rb.AddForce(Vector3.up * _upForce, ForceMode.Impulse);
        }
    }
    #endregion



    #region Lógica de Destrucción
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

}