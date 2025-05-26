using BehaviourTrees;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CrowAI : MonoBehaviour
{
    [Header("General Setting")]
    [SerializeField] float speed = 2;
    [SerializeField] float runningSpeed = 10;

    [Header("Vision Settings")]
    [SerializeField] float maxDistance = 100f;
    [SerializeField] float horizontalFOV = 120f;
    [SerializeField] float verticalFOV = 120f;


    [Header("AI Settings")]
    [SerializeField] List<Transform> wayPoints = new();
    GameObject player;
    internal bool detected = false;
    bool hear = false;
    Vector3 initialPos;
    internal NavMeshAgent agent;
    BehaviourTree tree;
    Node.Status callBack = Node.Status.Failure;    

    [Header("Patrol")]
    [SerializeField] float areaRadius = 100f;
    [SerializeField] float lookAroundSpeed = 90f; // grados por segundo

    [Header("Surrounding Setting")]
    [SerializeField] float radius;
    [SerializeField] float rotationSpeed = 100f;

    private void Awake()
    {
        player = GameObject.FindWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        initialPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);

        agent.updateUpAxis = false;
        agent.updateRotation = true;

        tree = new BehaviourTree("CrowAI");

        PrioritySelector actions = new PrioritySelector("Actions");

        Secuence detectAndPersecute = new Secuence("DetectAndPersecute", 10);
        detectAndPersecute.AddChild(new Leaf("PlayerDetected", new Condition(() => detected)));
        detectAndPersecute.AddChild(new Leaf("SurroundPlayer", new SurroundTargetStrategy(transform, agent, player.transform, radius, rotationSpeed)));
        //detectAndPersecute.AddChild(new Leaf("Persecute", new ActionStrategy(() => agent.SetDestination(player.transform.position))));

        Secuence lookAround = new Secuence("LookAround", 5);
        lookAround.AddChild(new Leaf("HearSomething", new Condition(() => hear)));
        lookAround.AddChild(new Leaf("TurnAround", new TurnAroundStrategy(transform, agent, lookAroundSpeed)));

        Leaf wander = new Leaf("Wander", new PatrolAreaStrategy(transform, initialPos, agent, areaRadius), 0);

        actions.AddChild(detectAndPersecute);
        actions.AddChild(lookAround);
        actions.AddChild(wander);

        tree.AddChild(actions);
    }

    private void Update()
    {
        // Datos variables del comportamiento        
        detected = IsDetected(player.transform);
        hear = IsHearing();

        if (detected)
            agent.speed = runningSpeed;
        else agent.speed = speed;

        // Inicio del árbol de decisiones
        callBack = tree.Process();            
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

    private bool IsHearing()
    {
        if ((player.transform.position - transform.position).magnitude < maxDistance) return true;
        return false;
    }
}