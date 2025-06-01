using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;


//[RequireComponent(typeof(InputActionReference))]
public class PlayerMovement_v0_1_1 : MonoBehaviour
{
    [Header("Movement Settings")]
    [Range(5f, 20f)]
    [SerializeField] private float maxSpeed = 10f;    // Velocidad m�xima
    [SerializeField] private float acceleration = 20f; // Aceleraci�n
    [SerializeField] private float deceleration = 15f; // Desaceleraci�n

    //Salto

    [Tooltip("Fuerza de salto del jugador")]
    [SerializeField] private float upForce = 250f;

    
    private Rigidbody rb;
    private PlayerInput playerInput;
    [SerializeField] private InputActionReference m_movementInput;


    private Vector3 targetVelocity;

    private Vector2 input;

    
    void Start()
    {
        rb = GetComponent<Rigidbody>();  
        playerInput = GetComponent<PlayerInput>();
        rb.linearDamping = 3f;
    }

    // Update is called once per frame
    void Update()
    {
        
        input = m_movementInput.action.ReadValue<Vector2>();
        Debug.Log(m_movementInput.action.phase);
        
    }

    private void FixedUpdate()
    {
        // Calcular la direcci�n deseada (X y Z globales)
        Vector3 desiredDirection = new Vector3(input.x, 0f, input.y).normalized;

        // Escalar por la magnitud del input (0 a 1)
        float inputMagnitude = input.magnitude;
        desiredDirection *= inputMagnitude;

        // Calcular velocidad objetivo
        targetVelocity = desiredDirection * maxSpeed;

        // Aplicar aceleraci�n/desaceleraci�n
        Vector3 velocityChange = (targetVelocity - rb.linearVelocity);
        velocityChange.y = 0f; // Ignorar cambios en Y

        // Elegir fuerza seg�n si hay input
        float force = inputMagnitude > 0.1f ? acceleration : deceleration;

        // Aplicar fuerza
        rb.AddForce(velocityChange * force, ForceMode.Acceleration);
    }

    
    public void Jump(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed)
        {
            rb.AddForce(Vector3.up * upForce);
        }
        Debug.Log(callbackContext.phase, this.gameObject);

    }


}
