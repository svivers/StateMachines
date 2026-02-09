using System;
using System.Collections.Generic;

namespace Core.StateMachines.Transitions.Executors
{
    public class ConditionalTransitionExecutor<TId>
    {
        private readonly IStateSwitcher<TId> m_stateSwitcher;
        private readonly Dictionary<TId, List<ConditionalTransition>> m_transitions;
        private List<ConditionalTransition> m_activeTransitions;
        private bool m_isActiveTransitionsDirty;
        private TId m_activeStateId;

        public ConditionalTransitionExecutor(IStateSwitcher<TId> stateSwitcher)
        {
            if (m_stateSwitcher == null)
                throw new ArgumentNullException(nameof(stateSwitcher));

            m_stateSwitcher = stateSwitcher;
            m_transitions = new Dictionary<TId, List<ConditionalTransition>>();
            m_activeTransitions = null;
            m_isActiveTransitionsDirty = true;
            m_activeStateId = default;
        }

        public ConditionalTransitionExecutor<TId> Add(Transition<TId> transition, Func<bool> condition)
        {
            if (m_transitions.TryGetValue(transition.From, out List<ConditionalTransition> transitions))
            {
                if (HasTransition(transition, transitions))
                    throw new InvalidOperationException($"Already contains this transition: from '{transition.From}' to '{transition.To}'");

                transitions.Add(new ConditionalTransition(transition, condition));
            }
            else
                m_transitions.Add(transition.From, new List<ConditionalTransition>() { new ConditionalTransition(transition, condition) });

            m_isActiveTransitionsDirty = true;
            return this;
        }

        public ConditionalTransitionExecutor<TId> Remove(Transition<TId> transition)
        {
            if (!m_transitions.TryGetValue(transition.From, out List<ConditionalTransition> transitions))
                return;

            for (int i = 0; i < transitions.Count; i++)
            {
                if (transition != transitions[i].Transition)
                    continue;

                transitions.RemoveAt(i);
                break;
            }

            if (transitions.Count == 0)
                m_transitions.Remove(transition.From);

            m_isActiveTransitionsDirty = true;
            return this;
        }

        public IReadOnlyList<Transition<TId>> GetTransitions()
        {
            List<Transition<TId>> transitions = new List<Transition<TId>>();

            foreach (var condition in m_transitions.Values)
                for (int i = 0; i < condition.Count; i++)
                    transitions.Add(condition[i].Transition);

            return transitions;
        }

        public void Tick()
        {
            if (!m_stateSwitcher.IsActiveState(m_activeStateId) || m_isActiveTransitionsDirty)
            {
                m_activeStateId = m_stateSwitcher.ActiveStateId;
                m_isActiveTransitionsDirty = false;
                m_transitions.TryGetValue(m_activeStateId, out m_activeTransitions);
            }

            if (m_activeTransitions == null)
                return;

            for (int i = 0; i < m_activeTransitions.Count; i++)
            {
                if (!m_activeTransitions[i].Condition())
                    continue;

                m_stateSwitcher.ChangeState(m_activeTransitions[i].Transition.To);
                return;
            }
        }

        public bool HasTransition(Transition<TId> transition)
        {
            if (m_transitions.TryGetValue(transition.From, out List<ConditionalTransition> transitions))
                return HasTransition(transition, transitions);

            return false;
        }

        private bool HasTransition(Transition<TId> transition, List<ConditionalTransition> list)
        {
            for (int i = 0; i < list.Count; i++)
                if (transition == list[i].Transition)
                    return true;
            
            return false;
        }

        private readonly struct ConditionalTransition
        {
            public readonly Transition<TId> Transition;
            public readonly Func<bool> Condition;

            public ConditionalTransition(Transition<TId> transition, Func<bool> condition)
            {
                Transition = transition;
                Condition = condition;
            }
        }
    }
}

