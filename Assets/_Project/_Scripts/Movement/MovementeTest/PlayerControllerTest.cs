using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerControllerTest : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 720f;

    private Rigidbody rb;
    private Vector3 inputDirection;

    Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Obtener input de movimiento (horizontal y vertical)
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool run = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);        

        if (h != 0 || v != 0)
        {
            animator.SetBool("Wait", false);
            animator.SetBool("Walk", true);

            if (run)
            {
                animator.SetBool("Run", true);
                moveSpeed = 10f;
            }
            else
            {
                animator.SetBool("Run", false);
                moveSpeed = 5f;
            }

            animator.SetFloat("XSpeed", h);
            animator.SetFloat("YSpeed", v);
        }
        else
        {
            animator.SetBool("Wait", true);
            animator.SetBool("Walk", false);
        }

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Walk Tree") || stateInfo.IsName("Run Tree"))
        {
            // Dirección de movimiento en base al input
            inputDirection = new Vector3(h, 0, v).normalized;
        }
        else inputDirection = new Vector3(0, 0, 0);
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
            Quaternion targetRotation;
            if (inputDirection.z < 0f)
                targetRotation = Quaternion.LookRotation(moveDir * -1);
            else targetRotation = Quaternion.LookRotation(moveDir);
            rb.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }
}
