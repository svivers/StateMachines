namespace Core.StateMachines
{
    public interface IStateSwitcher<TId> : IReadOnlyStateMachine<TId>
    {
        bool ChangeState(TId toState);
    }
}
