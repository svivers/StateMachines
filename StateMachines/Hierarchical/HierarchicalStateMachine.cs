using System;
using System.Collections.Generic;

namespace Core.StateMachines.Hierarchical
{
    public class HierarchicalStateMachine<TId, TState> : IStateMachine<TId, TState>
        where TState : BaseState
    {
        private readonly Dictionary<TId, StateNode<TState>> m_allNodes;
        private readonly StateNode<TState> m_root;
        private StateNode<TState> m_lowestActiveNode;

        private HierarchicalStateMachine(StateNode<TState> root, Dictionary<TId, StateNode<TState>> states)
        {
            m_allNodes = states;
            m_root = root;
            m_lowestActiveNode = null;
            PreviousStateId = default;
            ActiveStateId = default;
        }

        public TId PreviousStateId { get; private set; }
        public TId ActiveStateId { get; private set; }
        public IReadOnlyCollection<TId> AllIds => m_allNodes.Keys;
        protected IReadOnlyDictionary<TId, StateNode<TState>> AllNodes => m_allNodes;
        protected StateNode<TState> Root => m_root;
        protected StateNode<TState> LowestActiveNode => m_lowestActiveNode;

        public event Action OnStateChanged;

        public static Builder Create(TId rootId, TState root)
        {
            return new Builder(rootId, root);
        }

        public bool ChangeState(TId to)
        {
            if (!m_allNodes.TryGetValue(to, out StateNode<TState> next))
                return false;

            if (m_lowestActiveNode == next)
                return false;

            StateNode<TState> ancestor = GetLowestCommonAncestor(m_lowestActiveNode, next);
            ExitUp(m_lowestActiveNode, ancestor);
            EnterDown(ancestor, next);
            m_lowestActiveNode = next;
            PreviousStateId = ActiveStateId;
            ActiveStateId = to;
            OnStateChanged?.Invoke();
            return true;
        }

        public TState GetState(TId id)
        {
            if (m_allNodes.TryGetValue(id, out StateNode<TState> node))
                return node.State;

            return null;
        }

        public bool HasState(TId id)
        {
            return m_allNodes.ContainsKey(id);
        }

        public bool IsInState(TId id)
        {
            if (m_lowestActiveNode == null)
                return false;

            if (!m_allNodes.TryGetValue(id, out StateNode<TState> node))
                return false;

            StateNode<TState> current = m_lowestActiveNode;

            while (current != null)
            {
                if (node == current)
                    return true;

                current = current.Parent;
            }

            return false;
        }

        public bool IsActiveState(TId id)
        {
            if (m_lowestActiveNode == null)
                return false;

            return EqualityComparer<TId>.Default.Equals(id, ActiveStateId);
        }

        protected StateNode<TState> GetLowestCommonAncestor(StateNode<TState> a, StateNode<TState> b)
        {
            if (a == null || b == null)
                return null;

            while (a.Depth > b.Depth)
                a = a.Parent;

            while (b.Depth > a.Depth)
                b = b.Parent;

            while (a != b)
            {
                a = a.Parent;
                b = b.Parent;
            }

            return a;
        }

        private void EnterDown(StateNode<TState> from, StateNode<TState> to)
        {
            if (to == null || to == from)
                return;

            EnterDown(from, to.Parent);
            to.State.Enter();
        }

        private void ExitUp(StateNode<TState> from, StateNode<TState> to)
        {
            while (from != null && from != to)
            {
                from.State.Exit();
                from = from.Parent;
            }
        }

        public class Builder
        {
            private readonly Dictionary<TId, StateNode<TState>> m_allStates;
            private readonly StateNode<TState> m_root;

            public Builder(TId rootId, TState root)
            {
                if (root == null)
                    throw new ArgumentNullException(nameof(root));

                m_allStates = new Dictionary<TId, StateNode<TState>>();
                m_root = new StateNode<TState>(root);
                m_allStates.Add(rootId, m_root);
            }

            public Builder AddState(TId parentId, TId id, TState state)
            {
                if (m_allStates.ContainsKey(id))
                    throw new InvalidOperationException($"Already has a state with this id: {id}");

                if (!m_allStates.TryGetValue(parentId, out StateNode<TState> parent))
                    throw new InvalidOperationException($"State with id: {parentId} must be registered before assigning a child to it");

                StateNode<TState> node = new StateNode<TState>(state);
                parent.AddChild(node);
                m_allStates.Add(id, node);
                return this;
            }

            public HierarchicalStateMachine<TId, TState> Build()
            {
                return new HierarchicalStateMachine<TId, TState>(m_root, m_allStates);
            }
        }
    }
}

