using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

//[RequireComponent(typeof(InputActionReference))]
public class PlayerMovement_v0_1_4 : MonoBehaviour
{
    private Rigidbody _rb;
    private Animator _animator; // Referencia al Animator

    [Header("Input Actions")]
    [SerializeField] private InputActionReference _movementInputAction;
    [SerializeField] private InputActionReference _sprintInputAction; // Nueva acción para Sprint
    [SerializeField] private InputActionReference _jumpInputAction; // Asumimos que tienes una acción de salto separada

    [Header("Locomotion")]
    [Space(5)]
    [Header("    Direction 📐")]  // Direcciones de movimiento
    [SerializeField] 
    private Vector3 _unitGoal;

    [SerializeField]
    private Vector2 _moveDirectionInput;

    [SerializeField]
    private Vector2 _animationInput; // Input 2D para animaciones (relativo al personaje)


    [Space(5)]
    [Header("    Speed 🚀")]  // Espacios para indentar
    [Space(5)]

    [Range(5f, 20f)]
    [SerializeField]
    private float _maxSpeed = 8f;    // Se muestra como "Max Speed"

    private Vector3 _velGoal;       // Velocidad objetivo
    public float speedFactor = 1.0f;
    public Vector3 groundVel = Vector3.zero;

    [Space(5)]
    [Header("    Acceleration ⏩")]  // Espacios para indentar
    [Space(5)]

    [Range(100f, 300f)]
    [SerializeField]
    private float _acceleration = 200f; // Se muestra como "Acceleration"

    [SerializeField]
    private AnimationCurve _accelerationFactorFromDot = AnimationCurve.EaseInOut(-1, 0.1f, 1, 1);

    [Range(5f, 15f)]
    [SerializeField]
    private float _gravityScaleDrop = 10f; // "Gravity Scale Drop"

    [Space(5)]
    [Header("    Force 💪🏻")]  // Espacios para indentar
    [Space(5)]

    [Range(100f, 300f)]
    [SerializeField]
    private float _maxAccelerationForce = 150f; // "Max Acceleration Force"

    [SerializeField]
    private AnimationCurve _maxAccelerationForceFactorFromDot = AnimationCurve.EaseInOut(-1, 0.1f, 1, 1);

    [SerializeField]
    private Vector3 _forceScale = new Vector3(1, 0, 1); // "Force Scale"

    [SerializeField]
    private float _maxAccelForceFactor = 1.0f;

    [Space(5)]
    [Header("    Stun 💥")]  // Espacios para indentar
    [Space(5)]

    [SerializeField] 
    private float _movementControlDisabledTimer = 0f; //Stun o aturdimiento

    [Space(15)]
    [Header("Jump")]
    [Space(5)]
    //Salto
    [Tooltip("Fuerza de salto del jugador")]

    [SerializeField] 
    private float upForce = 250f;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        if (_movementInputAction != null && _movementInputAction.action != null) // Buena práctica comprobar nulls
        {
            _movementInputAction.action.Enable();
        }
        else
        {
            Debug.LogError("Movement Input Action Reference no está asignada o no es válida.", this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Guarda el input 2D leído
        if (_movementInputAction != null && _movementInputAction.action != null)
        {
            _moveDirectionInput = _movementInputAction.action.ReadValue<Vector2>();
        }
    }

    private void FixedUpdate()
    {
        ProcessMovementInput(Time.fixedDeltaTime);
    }

    void ProcessMovementInput(float time)
    {
        Vector2 currentInput = _moveDirectionInput;

        // 2. Comprueba si el control está deshabilitado por el temporizador
        if (_movementControlDisabledTimer > 0f)
        {
            currentInput = Vector2.zero; // ¡Usa Vector2.zero!
            _movementControlDisabledTimer -= time; // ¡Usa Time.fixedDeltaTime!
        }
        else
        {
            // 3. Normaliza si la magnitud es mayor que 1
            if (currentInput.magnitude > 1.0f)
            {
                currentInput.Normalize();
            }

            // 4. Convierte el Vector2 procesado a un Vector3 para m_UnitGoal
            //    Asumimos que el movimiento es en el plano XZ
            //    Input X -> World X, Input Y -> World Z
            _unitGoal = new Vector3(currentInput.x, 0f, currentInput.y);

            // --- Fin del procesamiento de la entrada ---

            // Ahora llama a Moverse(), que usará el m_UnitGoal ya calculado
            Moverse();
        }
    }

    void Moverse()
    {
        //1) Calcula la dirección normalizada de la velocidad objetivo actual del script. Obtener solo la dirección (un vector de magnitud 1) es necesario para calcular el "dot product" (producto escalar) en el siguiente paso.
        Vector3 unitVel = _velGoal.normalized;

        // 2) producto escalar entre la dirección del input deseado y dirección de la velocidad objetivo actual. Este valor (velDot) se usa para saber
        // si el jugador está intentando acelerar en la misma dirección, cambiar de dirección, o ir en la dirección opuesta a la que actualmente tiene la velocidad objetivo.
        float velDot = Vector3.Dot(_unitGoal, unitVel);

        //3) Calcula la aceleración efectiva para este paso físico.  multiplica por un factor obtenido evaluando la curva _accelerationFactorFromDot usando velDot como el "tiempo" o punto de entrada en la curva.
        //Si velDot es -1 (input opuesto a _velGoal), el factor es 2. Aceleración = _acceleration * 2.
        //Si velDot es 1(input igual a _velGoal), el factor es 1.Aceleración = _acceleration * 1.
        //Si velDot es 0(input perpendicular a _velGoal), el factor es aproximadamente 1.5(dependiendo de la curva exacta).Aceleración = _acceleration * 1.5.
        float accel = _acceleration * _accelerationFactorFromDot.Evaluate(velDot);

        //4) Calcula la velocidad objetivo final que el personaje debería intentar alcanzar en este momento, basada puramente en el input del jugador.
        //Esto te da el vector de velocidad ideal si el jugador estuviera instantáneamente a la velocidad máxima en la dirección deseada, modificada por un factor externo
        Vector3 velGoal = _unitGoal * _maxSpeed * speedFactor;

        //5) Actualiza el vector de velocidad objetivo interno
        _velGoal = Vector3.MoveTowards(_velGoal, (velGoal) + (groundVel), accel * Time.fixedDeltaTime);

        AplicarFuerza(Time.fixedDeltaTime, velDot);

        CustomLogger.Log(this, $"m_UnitGoal: {_unitGoal}, Timer: {_movementControlDisabledTimer}");
    }

    void AplicarFuerza(float tiempo, float velDot)
    {
        Vector3 neededAccel = (_velGoal - _rb.linearVelocity) / tiempo;

        float maxAccel = _maxAccelerationForce * _maxAccelerationForceFactorFromDot.Evaluate(velDot) * _maxAccelForceFactor;

        neededAccel = Vector3.ClampMagnitude(neededAccel, maxAccel);

        _rb.AddForce(Vector3.Scale(neededAccel * _rb.mass, _forceScale));

        //Realmente no pone rb.rb.linearVelocity, pone _setup.rider.rigidbody.velocity, pero he supuesto que se refiere a lo mismo, ya que en esta version no existe tampoco velocity, solo linear y angular
    }

    #region Logica de salto
    //Logica del salto
    public void Jump(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed)
        {
            _rb.AddForce(Vector3.up * upForce);
        }
        Debug.Log(callbackContext.phase, this.gameObject);
    }
    #endregion

    #region Logica de destruccion o desactivacion
    private void OnDestroy()
    {
        _movementInputAction.action.Disable();
    }

    private void OnDisable()
    {
        _movementInputAction.action.Disable();
    }
    #endregion
}







