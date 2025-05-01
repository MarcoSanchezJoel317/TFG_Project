using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
public class PlayerMovement_v0_1_0 : MonoBehaviour
{
    private float upForce = 250f, force = 10f;

    private Rigidbody rb;

    private PlayerInput playerInput;

    [SerializeField] private InputActionReference m_movementInput;

    private Vector2 input;



    void Start()
    {
        rb = GetComponent<Rigidbody>();  
        playerInput = GetComponent<PlayerInput>();
    }

    // Update is called once per frame
    void Update()
    {

        input = m_movementInput.action.ReadValue<Vector2>();
        Debug.Log(input);


    }

    private void FixedUpdate()
    {
        rb.AddForce(new Vector3(input.x, 0f, input.y) * force);

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
