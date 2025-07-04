using BehaviourTrees;
using IAManager;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SheepAI : MonoBehaviour
{
    [Header("General Setting")]
    [SerializeField] float speed = 2;

    [Header("AI Settings")]
    [SerializeField] List<Transform> wayPoints = new();
    GameObject player;
    internal bool detected = false;
    Vector3 initialPos;
    internal NavMeshAgent agent;
    BehaviourTree tree;
    Node.Status callBack = Node.Status.Failure;
    public bool follow = false;
    public bool wander = false;

    [Header("Vision Settings")]
    [SerializeField] float maxDistance = 100f;
    [SerializeField] float horizontalFOV = 120f;
    [SerializeField] float verticalFOV = 120f;

    [Header("Patrol")]
    [SerializeField] float areaRadius = 100f;

    private void Awake()
    {
        player = GameObject.FindWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        initialPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);

        agent.updateUpAxis = false;
        agent.updateRotation = true;

        // [[[[ ARBOL DE DECISIONES OBEJA ]]]] 

        tree = new BehaviourTree("SheepAI");

        // [[[Selector de comportamiento general]]]
        PrioritySelector actions = new PrioritySelector("Actions");

        // [[Comportamiento seguir jugador]]
        BehaviourTrees.Sequence followPlayer = new BehaviourTrees.Sequence("FollowPlayer", 10);
        followPlayer.AddChild(new Leaf("IsFollowing", new Condition(() => follow)));
        followPlayer.AddChild(new Leaf("Follow", new ConidtionatedActionStrategy(() => agent.SetDestination(player.transform.position), () => detected)));

        // [[Comportamiento pasear]]
        BehaviourTrees.Sequence wanderAround = new BehaviourTrees.Sequence("WanderAround", 5);
        wanderAround.AddChild(new Leaf("IsWandering", new Condition(() => wander)));
        if (wayPoints.Count > 0)
            wanderAround.AddChild(new Leaf("Wander", new PatrolStrategy(transform, agent, wayPoints, speed)));
        else
            wanderAround.AddChild(new Leaf("Wander", new PatrolAreaStrategy(transform, initialPos, agent, areaRadius)));        

        Leaf stay = new Leaf("StayInPlace", new ActionStrategy(() => agent.ResetPath()));

        // [[[Contrucción del arbol final]]]
        actions.AddChild(followPlayer);
        actions.AddChild(wanderAround);
        actions.AddChild(stay);

        tree.AddChild(actions);
    }

    private void Update()
    {
        // Datos variables del comportamiento        
        detected = IsDetected(player.transform);
        // Inicio del árbol de decisiones
        callBack = tree.Process();
        agent.speed = speed;
    }

    private bool IsDetected(Transform target)
    {
        Vector3 start = transform.position + Vector3.up;
        Vector3 end = player.transform.position + Vector3.up;
        Vector3 directionToTarget = end - start;

        // Rechazamos por distancia
        if (directionToTarget.magnitude > maxDistance)
            return false;

        if (!detected)
        {
            float distance = directionToTarget.magnitude;
            Debug.DrawLine(start, end, Color.red, 2f);

            if (Physics.Raycast(start, directionToTarget.normalized, out RaycastHit hit, distance))
                if (!hit.collider.CompareTag("Player")) return false;

            Vector3 localDirection = transform.InverseTransformDirection(directionToTarget.normalized);

            // Ángulo horizontal: eje XZ (proyección sobre plano horizontal)
            float horizontalAngle = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;

            // Ángulo vertical: eje Y (elevación)
            float verticalAngle = Mathf.Asin(localDirection.y) * Mathf.Rad2Deg;

            // Comprobamos si está dentro de los límites del cono
            return Mathf.Abs(horizontalAngle) <= horizontalFOV / 2f &&
                   Mathf.Abs(verticalAngle) <= verticalFOV / 2f;
        }
        return true;
    }
    private void OnDrawGizmosSelected()
    {
        // Dibujar el área de patrullaje
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, areaRadius);

        // Dibujar el cono de visión (simplificado a líneas guía)
        Gizmos.color = Color.yellow;

        Vector3 origin = transform.position + Vector3.up * 6;

        // Centro
        Vector3 forward = transform.forward * maxDistance;
        Gizmos.DrawLine(origin, origin + forward);

        // Extremos horizontales
        Quaternion leftRotation = Quaternion.Euler(0, -horizontalFOV / 2f, 0);
        Quaternion rightRotation = Quaternion.Euler(0, horizontalFOV / 2f, 0);
        Gizmos.DrawLine(origin, origin + leftRotation * forward);
        Gizmos.DrawLine(origin, origin + rightRotation * forward);

        // Extremos verticales (solo representativo en 3D)
        Quaternion upRotation = Quaternion.Euler(-verticalFOV / 2f, 0, 0);
        Quaternion downRotation = Quaternion.Euler(verticalFOV / 2f, 0, 0);
        Gizmos.DrawLine(origin, origin + upRotation * forward);
        Gizmos.DrawLine(origin, origin + downRotation * forward);
    }    
}