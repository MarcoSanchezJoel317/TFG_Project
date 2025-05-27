using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class WolfAnimation : MonoBehaviour
{
    [Header("Surface alaign")]
    public float raycastDistance = 2f;
    public LayerMask groundMask = Physics.DefaultRaycastLayers;
    public float alignSpeed = 10f;

    [Header("Animator controler")]
    Animator animator;
    Transform parentTransform;
    Vector3 LastPos;
    Quaternion lastRotation;
    float valueY = 1;

    [Header("AIconection")]
    [SerializeField]WolfAI wolfAI;
    
    private void Start()
    {
        animator = GetComponent<Animator>();
        parentTransform = transform.parent;
        lastRotation = parentTransform.rotation;
        LastPos = transform.position;
        wolfAI = transform.parent.gameObject.GetComponent<WolfAI>();
    }
    private void Update()
    {
        Vector3 nowPos = transform.position;
        Quaternion nowRotation = transform.rotation;

        float XMotion = animator.GetFloat("MotionX");
        float YMotion = animator.GetFloat("MotionY");

        if (wolfAI.detected)
        {
            animator.SetBool("Run", true);
            if (wolfAI.agent.remainingDistance < 3f)
                valueY = -1;
        }
        else{
            animator.SetBool("Run", false);
            valueY = 1;
        }            

        // Movement
        if (LastPos != nowPos)
            animator.SetFloat("MotionY", valueY);
        else animator.SetFloat("MotionY", 0);

        // Rotation
        if (nowRotation.eulerAngles.y > lastRotation.eulerAngles.y && XMotion < 1)
            animator.SetFloat("MotionX", XMotion + 0.01F);
        else if (nowRotation.eulerAngles.y < lastRotation.eulerAngles.y && XMotion > -1)
            animator.SetFloat("MotionX", XMotion - 0.01F);
        else if (XMotion != 0)
        {
            if (XMotion < 0)
                animator.SetFloat("MotionX", XMotion + 0.01F);
            else
                animator.SetFloat("MotionX", XMotion - 0.01F);
        }
            

        lastRotation = nowRotation;
    }
    void FixedUpdate()
    {
        AlignXToSlope();
    }

    void AlignXToSlope()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, raycastDistance, groundMask))
        {
            // Tomamos la rotación actual en Y y Z
            float currentY = transform.eulerAngles.y;
            float currentZ = transform.eulerAngles.z;

            // Creamos una rotación que se alinee con la pendiente
            Quaternion slopeRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

            // Aplicamos la inclinación X
            Vector3 alignedEuler = (slopeRotation * Quaternion.Euler(0, currentY, currentZ)).eulerAngles;

            // Aplicamos solo X
            Quaternion finalRotation = Quaternion.Euler(alignedEuler.x, currentY, currentZ);

            transform.rotation = Quaternion.Slerp(transform.rotation, finalRotation, alignSpeed * Time.deltaTime);
        }
    }
}
