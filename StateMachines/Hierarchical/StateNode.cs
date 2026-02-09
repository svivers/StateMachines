using System;
using System.Collections.Generic;

namespace Core.StateMachines
{
    public class StateNode<TState> where TState : BaseState
    {
        private readonly TState m_state;
        private readonly List<StateNode<TState>> m_children;

        public StateNode(TState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            m_state = state;
            m_children = new List<StateNode<TState>>();
            Parent = null;
            Depth = 0;
        }

        public StateNode<TState> Parent { get; private set; }
        public IReadOnlyList<StateNode<TState>> Children => m_children;
        public TState State => m_state;
        public int Depth { get; private set; }

        public void AddChild(StateNode<TState> child)
        {
            if (child == null)
                throw new ArgumentNullException(nameof(child));

            if (child.Parent != null)
                throw new InvalidOperationException("The child node already has a parent");

            if (IsAncestorOf(child))
                throw new InvalidOperationException("Circular dependency is not allowed");

            m_children.Add(child);
            child.Parent = this;
            child.Depth = Depth + 1;
        }

        public void RemoveChild(StateNode<TState> child)
        {
            if (m_children.Remove(child))
            {
                child.Parent = null;
                child.Depth = 0;
            }
        }

        public bool IsAncestorOf(StateNode<TState> node)
        {
            StateNode<TState> current = this;

            while (current != null)
            {
                if (current == node)
                    return true;

                current = current.Parent;
            }

            return false;
        }
    }
}
