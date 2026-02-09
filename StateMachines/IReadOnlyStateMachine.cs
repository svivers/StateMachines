using System;
using System.Collections.Generic;

namespace Core.StateMachines
{
    public interface IReadOnlyStateMachine<TId>
    {
        TId PreviousStateId { get; }
        TId ActiveStateId { get; }
        IReadOnlyCollection<TId> AllIds { get; }

        event Action OnStateChanged;

        bool HasState(TId id);
        bool IsInState(TId id);
        bool IsActiveState(TId id);
    }
}
