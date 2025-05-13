using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace BehaviourTrees
{    
    public class UntilFail : Node {
        public UntilFail(string name) : base(name) { }

        public override Status Process()
        {
            if (children[0].Process() == Status.Failure)
            {
                Reset();
                return Status.Failure;
            }

            return Status.Running;
        }
    }

    public class Inverter : Node
    {
        public Inverter(string name) : base(name) { }

        public override Status Process()
        {
            switch (children[0].Process())
            {
                case Status.Running:
                    return Status.Running;
                case Status.Failure:
                    return Status.Succsess;
                default:
                    return Status.Failure;
            }
        }
    }

    public class RandomSelector : PrioritySelector
    {
        protected override List<Node> SortChildren() => Shuffle().ToList();

        public RandomSelector (string name) : base(name) { }

        public List<Node> Shuffle()
        {
            List<Node> shuffeledList = new List<Node>(children);
            int rand;
            for (int i = 0; i < children.Count; i++)
            {
                rand = Random.Range(0, children.Count);

                shuffeledList[i] = shuffeledList[rand];
                shuffeledList[rand] = children[i];
            }
            return shuffeledList;
        }
    }

    public class PrioritySelector : Selector
    {
        List<Node> sortedChildren;
        List<Node> SortedChildren => sortedChildren ??= SortChildren();

        protected virtual List<Node> SortChildren() => children.OrderByDescending(child => child.priority).ToList();

        public PrioritySelector (string name) : base(name) { }

        public override void Reset()
        {
            base.Reset();
            sortedChildren = null;
        }

        public override Status Process()
        {
            foreach (var child in SortedChildren)
            {
                switch (child.Process())
                {
                    case Status.Running:
                        return Status.Running;
                    case Status.Succsess:
                        Reset();
                        return Status.Succsess;
                    default:
                        continue;
                }
            }

            return Status.Failure;
        }
    }

    public class Selector : Node
    {
        public Selector(string name, int priority = 0) : base(name, priority) { }

        public override Status Process()
        {
            if (currentChild < children.Count)
            {
                switch (children[currentChild].Process())
                {
                    case Status.Running:
                        return Status.Running;
                    case Status.Succsess:
                        Reset();
                        return Status.Succsess;
                    default:
                        currentChild++;
                        return Status.Running;
                }
            }

            Reset();
            return Status.Succsess;
        }
    }

    public class Secuence : Node
    {
        public Secuence(string name, int priority = 0) : base(name, priority) { }

        public override Status Process()
        {
            if (currentChild < children.Count)
            {
                switch (children[currentChild].Process())
                {
                    case Status.Running:
                        return Status.Running;
                    case Status.Failure:
                        Reset();
                        return Status.Failure;
                    default:
                        currentChild++;
                        return currentChild == children.Count ? Status.Succsess : Status.Running;
                }
            }

            Reset();
            return Status.Succsess;
        }
    }

    public class BehaviourTree : Node
    {
        public BehaviourTree(string name) : base(name) { }

        public override Status Process()
        {
            while (currentChild < children.Count)
            {
                var status = children[currentChild].Process();
                if (status != Status.Succsess)
                    return status;
                currentChild++;
            }

            Reset();
            return Status.Succsess;            
        }
    }

    public class Leaf : Node
    {
        readonly IStrategy strategy;

        public Leaf(string name, IStrategy strategy, int priority = 0) : base(name, priority)
        {
            //Preconditions.CheckNotNull(strategy);
            this.strategy = strategy;
        }

        public override Status Process() => strategy.Process();

        public override void Reset() => strategy.Reset();
    }
    
    public class Node : MonoBehaviour
    {
        public enum Status { Succsess, Failure, Running}

        public readonly string name;
        public readonly int priority;

        public readonly List<Node> children = new();
        protected int currentChild;

        public Node(string name = "Node", int priority = 0)
        {
            this.name = name;
            this.priority = priority;
        }

        public void AddChild(Node child) => children.Add(child);

        public virtual Status Process() => children[currentChild].Process();

        public virtual void Reset()
        {
            currentChild = 0;
            foreach (var child in children) 
            {
                child.Reset();
            }
        }
    }
}