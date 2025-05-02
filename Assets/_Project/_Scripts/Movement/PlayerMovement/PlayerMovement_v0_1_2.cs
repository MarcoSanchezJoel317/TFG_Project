using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;


//[RequireComponent(typeof(InputActionReference))]
public class PlayerMovement_v0_1_2 : MonoBehaviour
{
    [Header("Locomotion")]
    
    [Range(5f, 20f)] [SerializeField] 
    private float maxSpeed = 8f;    // Velocidad maxima

    [Range(100f, 300f)] [SerializeField] 
    private float acceleration = 200f; // Aceleraci�n

    [SerializeField]
    private AnimationCurve accelerationFactorFromDot = AnimationCurve.EaseInOut(-1, 2, 1, 1); // Curva predeterminada

    [Range(100f, 300f)] [SerializeField] 
    private float maxAccelerationForce = 150f;    // Maxima fuerza de aceleracion

    [SerializeField]
    private AnimationCurve accelerationForceFactorFromDot = AnimationCurve.EaseInOut(-1, 2, 1, 1); // Curva predeterminada

    [SerializeField] 
    Vector3 forceScale = new Vector3(1, 0, 1);

    [Range(5f, 15f)] [SerializeField] 
    private float gravityScaleDrop = 10f;    // Escala de gravedad

    private Vector3 m_UnitGoal;

    private Vector2 moveDirectionInput;


    //Stun o aturdimiento
    private float m_MovementControlDisabledTimer = 0f;


    //Salto

    [Tooltip("Fuerza de salto del jugador")]
    [SerializeField] private float upForce = 250f;

    
    private Rigidbody rb;
    [SerializeField] private InputActionReference m_movementInput;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (m_movementInput != null && m_movementInput.action != null) // Buena práctica comprobar nulls
        {
            m_movementInput.action.Enable();
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
        if (m_movementInput != null && m_movementInput.action != null)
        {
            moveDirectionInput = m_movementInput.action.ReadValue<Vector2>();
        }
    }


    private void FixedUpdate()
    {
        ProcessMovementInput(Time.fixedDeltaTime);
    }

    void ProcessMovementInput(float time)
    {
        Vector2 currentInput = moveDirectionInput;

        // 2. Comprueba si el control está deshabilitado por el temporizador
        if (m_MovementControlDisabledTimer > 0f)
        {
            currentInput = Vector2.zero; // ¡Usa Vector2.zero!
            m_MovementControlDisabledTimer -= time; // ¡Usa Time.fixedDeltaTime!
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
            m_UnitGoal = new Vector3(currentInput.x, 0f, currentInput.y);

            // --- Fin del procesamiento de la entrada ---


            // Ahora llama a Moverse(), que usará el m_UnitGoal ya calculado
            Moverse();

        }



    }


    void Moverse()
    {
        // Ahora esta función ya no calcula m_UnitGoal, sino que lo USA.
        // Aquí es donde añadirás la lógica de fuerzas, velocidad, etc.
        // utilizando m_UnitGoal como la dirección deseada.

        // Por ejemplo (esto es solo para ilustrar, no es la implementación final):
        // Vector3 desiredVelocity = m_UnitGoal * maxSpeed;
        // Vector3 force = (desiredVelocity - rb.velocity) * acceleration;
        // rb.AddForce(force);

        // De momento, podemos imprimir m_UnitGoal para verificar que se calcula bien:
        CustomLogger.Log(this, $"m_UnitGoal: {m_UnitGoal}, Timer: {m_MovementControlDisabledTimer}");
        Debug.Log("Hello");
    }


    public void Jump(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed)
        {
            rb.AddForce(Vector3.up * upForce);
        }
        Debug.Log(callbackContext.phase, this.gameObject);

    }


    #region Logica de destruccion o desactivacion
    private void OnDestroy()
    {
        m_movementInput.action.Disable();
    }

    private void OnDisable()
    {
        m_movementInput.action.Disable();
    }

    #endregion
}
