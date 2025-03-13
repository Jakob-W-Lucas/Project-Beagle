using System;
using Unity.Behavior;

[BlackboardEnum]
public enum ActionState
{
    None,
    Idle,
    Travel,
    Follow,
    Attack
}
