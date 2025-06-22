using BehaviourTrees;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent))]
public class NPC : MonoBehaviour
{
    [SerializeField] List<Transform> wayPoints = new();
    [SerializeField] GameObject target1;
    [SerializeField] GameObject target2;

    NavMeshAgent agent;
    BehaviourTree tree;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        tree = new BehaviourTree("NPC1");

        Leaf patrol = new Leaf("Patrol", new PatrolStrategy(transform, agent, wayPoints), 0);

        Leaf isTargetPresent1 = new Leaf("isTargetPresent1", new Condition(() => target1.activeSelf));
        Leaf moveToTarget1 = new Leaf("moveToTarget1", new ActionStrategy(() => agent.SetDestination(target1.transform.position)));

        Leaf isTargetPresent2 = new Leaf("isTargetPresent2", new Condition(() => target2.activeSelf));
        Leaf moveToTarget2 = new Leaf("moveToTarget2", new ActionStrategy(() => agent.SetDestination(target2.transform.position)));

        Sequence goToTarget1 = new Sequence("goToTarget1", 10);
        goToTarget1.AddChild(isTargetPresent1);
        goToTarget1.AddChild(moveToTarget1);

        Sequence goToTarget2 = new Sequence("goToTarget2", 20);
        goToTarget2.AddChild(isTargetPresent2);
        goToTarget2.AddChild(moveToTarget2);

        PrioritySelector chooseTarget = new PrioritySelector("chooseTarget");
        chooseTarget.AddChild(goToTarget1);
        chooseTarget.AddChild(goToTarget2);
        chooseTarget.AddChild(patrol);

        tree.AddChild(chooseTarget);
    }

    private void Update()
    {
        tree.Process();
    }
}
