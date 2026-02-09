using System;
using System.Collections.Generic;

namespace Core.StateMachines.Transitions.Executors
{
    public class TriggerTransitionExecutor<TTrigger, TId>
    {
        private readonly IStateSwitcher<TId> m_stateSwitcher;
        private readonly Dictionary<TTrigger, List<Transition<TId>>> m_transitions;

        public TriggerTransitionExecutor(IStateSwitcher<TId> stateSwitcher)
        {
            if (stateSwitcher == null)
                throw new ArgumentNullException(nameof(stateSwitcher));

            m_stateSwitcher = stateSwitcher;
            m_transitions = new Dictionary<TTrigger, List<Transition<TId>>>();
        }

        public IReadOnlyCollection<TTrigger> Triggers => m_transitions.Keys;

        public void Add(Transition<TId> transition, TTrigger trigger)
        {
            if (m_transitions.TryGetValue(trigger, out List<Transition<TId>> transitions))
            {
                for (int i = 0; i < transitions.Count; i++)
                    if (EqualityComparer<TId>.Default.Equals(transition.From, transitions[i].From))
                        throw new InvalidOperationException($"The trigger '{trigger}' already has a transition from this state '{transition.From}'");

                transitions.Add(transition);
            }
            else
                m_transitions.Add(trigger, new List<Transition<TId>>() { transition });
        }

        public void Remove(Transition<TId> transition, TTrigger trigger)
        {
            if (!m_transitions.TryGetValue(trigger, out List<Transition<TId>> transitions))
                return;

            transitions.Remove(transition);

            if (transitions.Count == 0)
                m_transitions.Remove(trigger);
        }

        public void Remove(TTrigger trigger)
        {
            m_transitions.Remove(trigger);
        }

        public IReadOnlyList<Transition<TId>> GetTransitions(TTrigger trigger)
        {
            if (m_transitions.TryGetValue(trigger, out List<Transition<TId>> transitions))
                return transitions;

            return Array.Empty<Transition<TId>>();
        }

        public void Execute(TTrigger trigger)
        {
            if (!m_transitions.TryGetValue(trigger, out List<Transition<TId>> transitions))
                return;

            for (int i = 0; i < transitions.Count; i++)
            {
                if (!CanExecute(transitions[i]))
                    continue;

                m_stateSwitcher.ChangeState(transitions[i].To);
                return;
            }
        }

        public bool HasTransition(Transition<TId> transition)
        {
            foreach(var t in m_transitions.Values)
                for (int i = 0; i < t.Count; i++)
                    if (transition == t[i])
                        return true;

            return false;
        }

        private bool CanExecute(Transition<TId> transition)
        {
            return m_stateSwitcher.IsActiveState(transition.From);
        }
    }
}
