using NUnit.Framework;
using System;
using System.Collections;
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

    // LOGIC STRATEGIES
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

        public Condition(Func<bool> predicate)
        {
            this.predicate = predicate;
        }

        public Node.Status Process() => predicate() ? Node.Status.Succsess : Node.Status.Failure;
    }

    // WOLF STRATEGIES
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

    public class PatrolAreaStrategy : IStrategy
    {
        readonly Transform entity;
        readonly Vector3 initialPos;
        readonly NavMeshAgent agent;
        readonly float areaRadius;

        Vector3 destination = Vector3.zero;

        public PatrolAreaStrategy(Transform entity, Vector3 initialPos, NavMeshAgent agent, float areaRadius)
        {
            this.entity = entity;
            this.initialPos = initialPos;
            this.agent = agent;
            this.areaRadius = areaRadius;
        }

        public Node.Status Process()
        {
            float x;
            float z;

            if (destination == Vector3.zero || agent.remainingDistance < 5f)
            {
                x = UnityEngine.Random.Range(-areaRadius, areaRadius);
                z = UnityEngine.Random.Range(-areaRadius, areaRadius);
                destination = initialPos + new Vector3(x, 0, z);
                agent.SetDestination(destination);
                entity.LookAt(destination);
            }
            else return Node.Status.Succsess;

            return Node.Status.Running;
        }
    }

    public class TurnAroundStrategy : IStrategy
    {
        readonly Transform transform;
        readonly NavMeshAgent agent;
        float totalAngle = 0;
        int stage = 0;
        float speed;

        public TurnAroundStrategy(Transform transform, NavMeshAgent agent, float speed)
        {
            this.transform = transform;
            this.agent = agent;
            this.speed = speed;
        }

        public Node.Status Process()
        {
            float step = speed * Time.deltaTime;
            agent.ResetPath();
            switch (stage)
            {
                case 0:
                    transform.Rotate(Vector3.up, step);
                    totalAngle += step;
                    if (totalAngle >= 45f)
                    {
                        totalAngle = 0;
                        stage++;
                    }
                    return Node.Status.Running;

                case 1:
                    transform.Rotate(Vector3.up, -step);
                    totalAngle += step;
                    if (totalAngle >= 180f)
                    {
                        totalAngle = 0;
                        stage++;
                    }
                    return Node.Status.Running;

                case 2:
                    transform.Rotate(Vector3.up, step);
                    totalAngle += step;
                    if (totalAngle >= 360f)
                    {
                        return Node.Status.Succsess;
                    }
                    return Node.Status.Running;

                default:
                    return Node.Status.Succsess;
            }
        }

        public void Reset()
        {
            totalAngle = 0;
            stage = 0;
        }
    }

}
