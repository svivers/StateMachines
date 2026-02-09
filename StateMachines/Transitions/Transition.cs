using System;
using System.Collections.Generic;

namespace Core.StateMachines.Transitions
{
    public readonly struct Transition<TId> : IEquatable<Transition<TId>>
    {
        private readonly TId m_from;
        private readonly TId m_to;

        public Transition(TId from, TId to)
        {
            m_from = from;
            m_to = to;
        }

        public TId From => m_from;
        public TId To => m_to;

        public bool Equals(Transition<TId> other)
        {
            return EqualityComparer<TId>.Default.Equals(m_from, other.m_from)
                && EqualityComparer<TId>.Default.Equals(m_to, other.m_to);
        }

        public override bool Equals(object obj) => obj is Transition<TId> other && Equals(other);

        public override int GetHashCode()
        {
            return HashCode.Combine(EqualityComparer<TId>.Default.GetHashCode(m_from),
                EqualityComparer<TId>.Default.GetHashCode(m_to));
        }

        public static bool operator ==(Transition<TId> left, Transition<TId> right) => left.Equals(right);

        public static bool operator !=(Transition<TId> left, Transition<TId> right) => !left.Equals(right);
    }
}
