namespace Core.StateMachines
{
    public interface IStateMachine<TId, TState> : IStateSwitcher<TId>
        where TState : BaseState
    {
        TState GetState(TId id);
    }
}
