using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SpiritMovement : MonoBehaviour
{
    [Header("References")]
    public Transform origin;
    public Transform target;

    [Header("Navigation Settings")]
    public float maxDistanceFromOrigin = 10f;
    public float minHeightAboveOrigin = 4f;

    [Header("Breathing Settings")]
    [Tooltip("Frecuencia de respiración (ciclos por segundo)")]
    public float breathingFrequency = 0.5f;
    [Tooltip("Amplitud del pulso de respiración (unidades Y)")]
    public float breathingAmplitude = 0.3f;

    NavMeshAgent agent;
    float breathingOffset;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = true;
        agent.updateUpAxis = true;
    }

    void Update()
    {
        if (origin == null || target == null) return;

        // --- Misma lógica de clamp en XZ ---
        Vector3 originXZ = new Vector3(origin.position.x, 0, origin.position.z);
        Vector3 targetXZ = new Vector3(target.position.x, 0, target.position.z);
        Vector3 offset = targetXZ - originXZ;
        if (offset.magnitude > maxDistanceFromOrigin)
            offset = offset.normalized * maxDistanceFromOrigin;
        Vector3 clampedTarget = originXZ + offset;
        clampedTarget.y = origin.position.y;
        agent.SetDestination(clampedTarget);
    }

    void LateUpdate()
    {
        // 1) Calcula offset de respiración
        breathingOffset = Mathf.Sin(Time.time * Mathf.PI * 2f * breathingFrequency)
                          * breathingAmplitude;

        // 2) Aplica altura mínima + respiración
        Vector3 pos = transform.position;
        float baseMinY = origin.position.y + minHeightAboveOrigin;
        float targetY = baseMinY + breathingOffset;
        if (pos.y < targetY)
            pos.y = targetY;
        else
            pos.y = targetY;  // si quieres que siempre oscile arriba y abajo, quita el if
        transform.position = pos;
    }

    void OnDrawGizmosSelected()
    {
        if (origin == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(origin.position, maxDistanceFromOrigin);
        // Altura mínima
        Gizmos.color = Color.yellow;
        Vector3 floor = origin.position;
        Vector3 ceiling = origin.position + Vector3.up * minHeightAboveOrigin;
        Gizmos.DrawLine(floor, ceiling);
    }
}





