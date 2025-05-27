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
        Secuence followPlayer = new Secuence("FollowPlayer", 10);
        followPlayer.AddChild(new Leaf("PlayerDetected", new Condition(() => follow)));
        followPlayer.AddChild(new Leaf("LookPack", new ConidtionatedActionStrategy(() => agent.SetDestination(player.transform.position), () => detected)));

        // [[Comportamiento pasear]]
        Secuence wanderAround = new Secuence("WanderAround", 5);
        wanderAround.AddChild(new Leaf("PlayerDetected", new Condition(() => wander)));
        wanderAround.AddChild(new Leaf("WalkAround", new PatrolAreaStrategy(transform, initialPos, agent, areaRadius)));

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
}