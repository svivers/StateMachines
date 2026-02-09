# StateMachines
Fast, extendible and flexible state machines for you to add in any c# project be it .NET or Unity.

## A quick overview
The state machines designed to be immutable so after creating them you can't add or remove states. And the machines use an id to address the states, so the same state can be added to the machine more than once. It's a double edged sword, it can be a feature used for optimization and lower the number of classes you write, but also can cause bugs if used without caution!

Any state inheriting from ```BaseState``` class can be used in a FSM or HSM without making any changes to the state itself.
The HSM uses a wrapper object ```StateNode<TState>``` around ```BaseState``` to allow tree structure of states.

Transitions are used as a controller object for the state machines so the machines don't know about their existance, you can use them or write your own or not use them at all.

# How to use
Let's make a door with states open and closed.
```
    FSM
   /   \
Open   Closed
```

```csharp
// creating fsm
IStateMachine<string, BaseState> m_doorFsm = FiniteStateMachine<string, BaseState>
  .Create()
  .AddState("open", new OpenState())
  .AddState("closed", new ClosedState())
  .Build();

// to start the machine switch it to initial state
m_doorFsm.ChangeState("closed");
```

Going hierarchical we can make it more complex.
```
        HSM
         |
       Root
      /   \
Locked     Unlocked
  |        /       \
Closed   Closed    Open
```

```csharp
// creating hsm
IStateMachine<string, BaseState> m_doorHsm = HierarchicalStateMachine<string, BaseState>
  .Create("root", new RootState())
  .AddState("root", "locked", new LockedState())
  .AddState("root", "unlocked", new UnlockedState())
  .AddState("locked", "locked_closed", new ClosedState())
  .AddState("unlocked", "unlocked_closed", new ClosedState())
  .AddState("unlocked", "unlocked_open", new OpenState())
  .Build();

// switching to initial state
m_doorHsm.ChangeState("locked_closed");
```

# Using transitions
Since transitions are controlling objects for state machines they require ```IStateSwitcher<TId>``` upon creation to controll the machines

Note that ```TriggerTransitionExecutor<TTrigger, TId>``` can have multiple transitions assigned to one trigger.
```csharp
TriggerTransitionExecutor<string, string> m_hsmTransitions = new TriggerTransitionExecutor<string, string>(m_doorHsm)
  .Add(new Transition<string>("locked", "unlocked"), "unlock_the_door");
  .Add(new Transition<string>("locked_closed", "unlocked_closed"), "unlock_the_door");
  .Add(new Transition<string>("unlocked", "locked"), "lock_the_door");
  .Add(new Transition<string>("unlocked_closed", "locked_closed"), "lock_the_door");
  .Add(new Transition<string>("unlocked_closed", "unlocked_open"), "LET_ME_IIIIN!");
  .Add(new Transition<string>("unlocked_open", "unlocked_closed"), "close");

// opening the door
m_hsmTransitions.Execute("LET_ME_IIIIN!");
// closing and locking it
m_hsmTransitions.Execute("close");
m_hsmTransitions.Execute("lock_the_door");
```

```ConditionalTransitionExecutor<TId>``` is a collection of transitions and boolean functions (though i use lambdas here i recomend using properties)
```csharp
ConditionalTransitionExecutor<string> m_fsmTransitions = new ConditionalTransitionExecutor<string>(m_doorFsm)
    .Add(new Transition<string>("open", "closed"), () => Input.GetKeyUp(KeyCode.C))
    .Add(new Transition<string>("closed", "open"), () => Input.GetKeyUp(KeyCode.O));
```
Now update logic for your transitions
```csharp
void Update()
{
    m_fsmTransitions.Tick();
}
```

# Extending state machines
Now let's imagine we want to update our state each frame. We will need a ```Tick(float deltaTime)``` method in each state, so let's extend ```BaseState``` class
```csharp
public abstract class MyBaseState : BaseState
{
  public abstract void Tick(float deltaTime);
}
```

And we will need to extend a state machine for it to support updating a state each frame.
We will tick states from root to lowest active state.
```csharp
public class MyHSM<TId, TState> : HierarchicalStateMachine<TId, TState>
  where TState : MyBaseState
{
  public void TickStates(float deltaTime)
  {
     TickStatesFromRoot(base.LowestActiveNode);
  }

  // ticking states using recursion
  private void TickStatesFromRoot(StateNode<TState> node, float deltaTime)
  {
    if (node == null || node == base.Root)
      return;

    TickStatesFromRoot(node.Parent, deltaTime);
    node.State.Tick(deltaTime);
  }
}
```

And now let's use the extended machine
```csharp
IStateMachine<string, MyBaseState> m_myExtendedHsm = MyHSM<string, MyBaseState>
  .Create("root", new Root())
  .AddState("root", "someStateName", new SomeState())
  .Build();
```

Inside Unity environment it will look something like this
```csharp
void Update()
{
  m_myExtendedHsm.TickStates(Time.deltaTime);
}
```
