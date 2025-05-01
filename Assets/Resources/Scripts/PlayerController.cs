using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 720f;

    private Rigidbody rb;
    private Vector3 inputDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Obtener input de movimiento (horizontal y vertical)
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // Dirección de movimiento en base al input
        inputDirection = new Vector3(h, 0, v).normalized;
    }

    void FixedUpdate()
    {
        // Mover al personaje
        if (inputDirection.magnitude > 0.1f)
        {
            // Movimiento en dirección local
            Vector3 moveDir = Camera.main.transform.TransformDirection(inputDirection);
            moveDir.y = 0f;
            moveDir.Normalize();

            Vector3 moveVelocity = moveDir * moveSpeed;
            rb.MovePosition(rb.position + moveVelocity * Time.fixedDeltaTime);

            // Rotar suavemente hacia la dirección de movimiento
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            rb.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }
}
