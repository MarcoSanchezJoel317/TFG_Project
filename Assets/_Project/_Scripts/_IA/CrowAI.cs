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
    CrowAlarmManager alarmManager;

    [Header("Patrol")]
    [SerializeField] float areaRadius = 100f;
    [SerializeField] float lookAroundSpeed = 90f; // grados por segundo

    [Header("Surrounding Setting")]
    [SerializeField] float radius;
    [SerializeField] float rotationSpeed = 100f;

    private void Awake()
    {
        player = GameObject.FindWithTag("Player");
        alarmManager = GameObject.FindWithTag("Manager").GetComponent<CrowAlarmManager>();
        agent = GetComponent<NavMeshAgent>();
        initialPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);

        agent.updateUpAxis = false;
        agent.updateRotation = true;

        // [[[[ ARBOL DE DECISIONES CCUERVO ]]]] 

        tree = new BehaviourTree("CrowAI");

        // [[[Selector de comportamiento general]]]
        PrioritySelector actions = new PrioritySelector("Actions");

        // [[Comportamiento detección jugador]]
        BehaviourTrees.Sequence detectAndPersecute = new BehaviourTrees.Sequence("DetectAndPersecute", 10);
        detectAndPersecute.AddChild(new Leaf("PlayerDetected", new Condition(() => detected)));
        detectAndPersecute.AddChild(new Leaf("SurroundPlayer", new SurroundTargetStrategy(transform, agent, player.transform, radius, rotationSpeed, maxDistance)));

        // [[Comportamiento búsqueda jugador]]
        BehaviourTrees.Sequence lookAround = new BehaviourTrees.Sequence("LookAround", 5);
        lookAround.AddChild(new Leaf("HearSomething", new Condition(() => hear)));
        lookAround.AddChild(new Leaf("TurnAround", new TurnAroundStrategy(transform, agent, lookAroundSpeed)));

        Leaf wander = new Leaf("Wander", new PatrolAreaStrategy(transform, initialPos, agent, areaRadius), 0);

        // [[[Contrucción del arbol final]]]
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
        else
        {
            agent.speed = speed;
        }

        // Inicio del árbol de decisiones
        callBack = tree.Process();            
    }

    private bool IsDetected(Transform target)
    {
        Vector3 start = transform.position + Vector3.up * 6;
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