using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;


//[RequireComponent(typeof(InputActionReference))]
public class PlayerMovement_v0_1_3 : MonoBehaviour
{

    private Rigidbody _rb;
    [SerializeField] private InputActionReference _movementInputAction;


    [Header("Locomotion")]
    [Space(5)]
    [Header("    Direction 📐")]  // Direcciones de movimiento


    private Vector3 _unitGoal;


    private Vector2 _moveDirectionInput;

    [Space(5)]
    [Header("    Speed 🚀")]  // Espacios para indentar
    [Space(5)]
    [SerializeField]
    private AnimationCurve _directionalSpeedFactorFromDot = AnimationCurve.Linear(-1, 0.7f, 1, 1f); // Curva para controlar la velocidad por dirección

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

    //Stun o aturdimiento
    private float _movementControlDisabledTimer = 0f;


    [Header("Jump 🐇")]
    [Space(5)]
    //Salto

    [Tooltip("Fuerza de salto del jugador")]
    [SerializeField] private float upForce = 250f;


    


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
            currentInput = Vector2.zero;
            _movementControlDisabledTimer -= time;
        }
        else
        {
            // 3. Normaliza si la magnitud es mayor que 1 (Joystick)
            if (currentInput.magnitude > 1.0f)
            {
                currentInput.Normalize();
            }

            // 4. Convierte el Vector2 procesado a un Vector3 para _unitGoal
            //    AHORA relativo a la orientación del personaje (ignorando el eje Y vertical)

            // Obtenemos los vectores forward y right del personaje
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            // Eliminamos la componente vertical (Y) para que el movimiento sea solo horizontal
            forward.y = 0f;
            right.y = 0f;

            // Normalizamos los vectores después de poner Y a 0 (para evitar problemas si el personaje está inclinado)
            // No es estrictamente necesario si el Rigidbody restringe rotación, pero buena práctica
            forward.Normalize();
            right.Normalize();

            // Calculamos la dirección deseada en el mundo (unitGoal)
            // Usamos la componente Y del input para la dirección forward del personaje
            // Usamos la componente X del input para la dirección right del personaje
            _unitGoal = (forward * currentInput.y + right * currentInput.x);

            // Normalizamos el resultado final si su magnitud es mayor que 1
            // Esto puede ocurrir si los vectores forward y right no son perfectamente ortogonales después de la proyección,
            // o si quieres un comportamiento específico con inputs diagonales (ej: no ir más rápido en diagonal)
            if (_unitGoal.magnitude > 1.0f) // Comprobación de seguridad
            {
                _unitGoal.Normalize();
            }
            // Importante: si currentInput.magnitude <= 1, _unitGoal.magnitude será <= 1. Normalizar solo si > 1 mantiene control analógico parcial.
            // Si siempre quieres un vector de dirección normalizado cuando hay input:
            // if (currentInput.magnitude > 0.01f) _unitGoal.Normalize(); else _unitGoal = Vector3.zero;


            // --- Fin del procesamiento de la entrada ---

            // Ahora llama a Moverse(), que usará el _unitGoal ya calculado
            Moverse();
        }
    }


    // Añade esta variable (o una curva) en la sección Speed 🚀
    [Range(0f, 1f)]
    [SerializeField] private float _strafeSpeedMultiplier = 0.7f; // Factor de velocidad al strafear/atrás (0.7 = 70% de velocidad)

    // Opcional: Añade una curva para más control sobre la velocidad direccional
    // [SerializeField] AnimationCurve _directionalSpeedFactorFromDot = AnimationCurve.Linear(-1, 0.7f, 1, 1f); // Factor de velocidad basado en Dot(inputDir, characterForward)


    void Moverse()
    {
        // ... (código existente) ...
        Vector3 unitVel = _velGoal.normalized;
        float velDot = Vector3.Dot(_unitGoal, unitVel); // Dot entre input deseado y velocidad objetivo script (_velGoal)

        float accel = _acceleration * _accelerationFactorFromDot.Evaluate(velDot);

        // --- Calcular la velocidad objetivo real modulada por la dirección de input ---

        float targetSpeed = _maxSpeed * speedFactor; // Velocidad máxima base con speedFactor aplicado

        float directionalSpeedFactor = 1.0f; // Renombramos a factor para claridad

        if (_unitGoal.magnitude > 0.01f) // Solo si hay input de movimiento
        {
            // Obtener la dirección forward horizontal del personaje
            Vector3 characterForward = transform.forward;
            characterForward.y = 0;
            if (characterForward.magnitude > 0.01f)
            {
                characterForward.Normalize();
            }
            else
            {
                characterForward = Vector3.forward; // Default si transform.forward es (0,1,0) o similar
            }

            // Calcular el producto escalar entre la dirección de input deseada (_unitGoal) y la dirección forward horizontal del personaje
            float inputDotCharacterForward = Vector3.Dot(_unitGoal, characterForward);

            // --- USAR LA NUEVA CURVA PARA EL FACTOR DE VELOCIDAD ---
            // El Tiempo de la curva es inputDotCharacterForward (-1 a 1)
            // El Valor de la curva es el factor de velocidad resultante (0 a 1)
            directionalSpeedFactor = _directionalSpeedFactorFromDot.Evaluate(inputDotCharacterForward);

            // Asegurarse de que el factor esté en el rango esperado
            directionalSpeedFactor = Mathf.Clamp01(directionalSpeedFactor);
        }
        else
        {
            // Si no hay input (magnitud <= 0.01f), la velocidad objetivo debe ser 0
            directionalSpeedFactor = 0f;
        }

        // La velocidad objetivo FINAL basada en input y penalización direccional
        float finalTargetMagnitude = targetSpeed * directionalSpeedFactor; // Usar el factor de la curva
        Vector3 finalVelGoal = _unitGoal * finalTargetMagnitude;

        // 5) Actualiza el vector de velocidad objetivo interno hacia finalVelGoal + groundVel
        _velGoal = Vector3.MoveTowards(_velGoal, finalVelGoal + groundVel, accel * Time.fixedDeltaTime);

        AplicarFuerza(Time.fixedDeltaTime, velDot); // velDot sigue siendo input vs _velGoal

        // Debugging (recuerda condicionar o eliminar para release)
        // CustomLogger.Log(this, $"_unitGoal: {_unitGoal}, InputDotCF: {inputDotCharacterForward:F2}, DirSpeedFactor: {directionalSpeedFactor:F2}, FinalTargetMag: {finalTargetMagnitude:F2}, _velGoal: {_velGoal}, RB.vel: {_rb.linearVelocity}, Timer: {_movementControlDisabledTimer}");

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


