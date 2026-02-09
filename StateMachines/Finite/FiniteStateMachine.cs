using System;
using System.Collections.Generic;

namespace Core.StateMachines.Finite
{
    public class FiniteStateMachine<TId, TState> : IStateMachine<TId, TState>
        where TState : BaseState
    {
        private readonly Dictionary<TId, TState> m_allStates;
        private TState m_activeState;

        private FiniteStateMachine(Dictionary<TId, TState> states)
        {
            m_allStates = states;
            m_activeState = null;
            PreviousStateId = default;
            ActiveStateId = default;
        }

        public TId PreviousStateId { get; private set; }
        public TId ActiveStateId {  get; private set; }
        public IReadOnlyCollection<TId> AllIds => m_allStates.Keys;
        protected IReadOnlyDictionary<TId, TState> AllStates => m_allStates;
        protected TState ActiveState => m_activeState;

        public event Action OnStateChanged;

        public static Builder Create()
        {
            return new Builder();
        }

        public bool ChangeState(TId to)
        {
            if (!m_allStates.TryGetValue(to, out TState next))
                return false;

            if (IsInState(to))
                return false;

            m_activeState?.Exit();
            m_activeState = next;
            m_activeState.Enter();
            PreviousStateId = ActiveStateId;
            ActiveStateId = to;
            OnStateChanged?.Invoke();
            return true;
        }

        public TState GetState(TId id)
        {
            if (m_allStates.TryGetValue(id, out TState state))
                return state;

            return null;
        }

        public bool HasState(TId id)
        {
            return m_allStates.ContainsKey(id);
        }

        public bool IsInState(TId id)
        {
            if (m_activeState == null)
                return false;

            return EqualityComparer<TId>.Default.Equals(id, ActiveStateId);
        }

        public bool IsActiveState(TId id)
        {
            return IsInState(id);
        }

        public class Builder
        {
            private readonly Dictionary<TId, TState> m_allStates;

            public Builder()
            {
                m_allStates = new Dictionary<TId, TState>();
            }

            public Builder AddState(TId id, TState state)
            {
                if (m_allStates.ContainsKey(id))
                    throw new InvalidOperationException($"Already has a state with id: {id}");

                m_allStates.Add(id, state);
                return this;
            }

            public FiniteStateMachine<TId, TState> Build()
            {
                return new FiniteStateMachine<TId, TState>(m_allStates);
            }
        }
    }
}
