using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerControllerTest : MonoBehaviour
{
    [Header("Control Movimiento")]
    public float moveSpeed = 15f;
    float saveSpeed;
    public float rotationSpeed = 720f;

    private Rigidbody rb;
    private Vector3 inputDirection;

    public Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        //animator = GetComponent<Animator>();
        saveSpeed = moveSpeed;
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
                moveSpeed = saveSpeed*3;
            }
            else
            {
                animator.SetBool("Run", false);
                moveSpeed = saveSpeed;
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
            // Direcci�n de movimiento en base al input
            inputDirection = new Vector3(h, 0, v).normalized;
        }
        else inputDirection = new Vector3(0, 0, 0);
    }

    void FixedUpdate()
    {
        // Mover al personaje
        if (inputDirection.magnitude > 0.1f)
        {
            // Movimiento en direcci�n local
            Vector3 moveDir = Camera.main.transform.TransformDirection(inputDirection);
            moveDir.y = 0f;
            moveDir.Normalize();

            // --- Nuevo bloque: ajustar seg�n pendiente ---
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 2f))
            {
                // Proyectar la direcci�n sobre el plano del suelo
                moveDir = Vector3.ProjectOnPlane(moveDir, hit.normal).normalized;
            }
            // ------------------------------------------------

            Vector3 moveVelocity = moveDir * moveSpeed;
            rb.MovePosition(rb.position + moveVelocity * Time.fixedDeltaTime);

            // Rotar suavemente hacia la direcci�n de movimiento
            Quaternion targetRotation;
            if (inputDirection.z < 0f)
                targetRotation = Quaternion.LookRotation(moveDir * -1);
            else targetRotation = Quaternion.LookRotation(moveDir);
            rb.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }
}
