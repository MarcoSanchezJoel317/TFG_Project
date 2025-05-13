using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace BehaviourTrees
{
    public interface IStrategy
    {
        Node.Status Process();
        void Reset()
        {
            //No op
        }
    }

    public class ActionStrategy : IStrategy
    {
        readonly Action doSomething;

        public ActionStrategy(Action doSomething)
        {
            this.doSomething = doSomething;
        }
    
        public Node.Status Process()
        {
            doSomething();
            return Node.Status.Succsess;
        }
    }

    public class Condition : IStrategy
    {
        readonly Func<bool> predicate;

        public Condition (Func<bool> predicate)
        {
            this.predicate = predicate;
        }

        public Node.Status Process() => predicate() ? Node.Status.Succsess : Node.Status.Failure;
    }

    public class PatrolStrategy : IStrategy
    {
        readonly Transform entity;
        readonly NavMeshAgent agent;
        readonly List<Transform> patrolPoints;
        readonly float patrolSpeed;
        int courrentIndex;
        bool isPathCalpulated;

        public PatrolStrategy(Transform entity, NavMeshAgent agent, List<Transform> patrolPoints, float patrolSpeed = 200f)
        {
            this.entity = entity;
            this.agent = agent;
            this.patrolPoints = patrolPoints;
            this.patrolSpeed = patrolSpeed;
        }

        public Node.Status Process()
        {
            if (courrentIndex == patrolPoints.Count) return Node.Status.Succsess;

            var target = patrolPoints[courrentIndex];
            agent.SetDestination(target.position);
            entity.LookAt(target);

            if (isPathCalpulated && agent.remainingDistance < 0.1f)
            {
                courrentIndex++;
                isPathCalpulated = false;
            }
            if (agent.pathPending)
            {
                isPathCalpulated = true;
            }

            return Node.Status.Running;
        }

        public void Reset() => courrentIndex = 0;
    }
}   