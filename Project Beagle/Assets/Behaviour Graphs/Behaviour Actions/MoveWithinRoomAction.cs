using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityUtils;
using System.Collections.Generic;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MoveWithinRoom", story: "[Agent] moves within Room", category: "Action", id: "4cd7b7849d7ff515c81d1274c3038d9e")]
public partial class MoveWithinRoomAction : Action
{
    [SerializeReference] public BlackboardVariable<Agent> Agent;
    NavigationState Nav;
    Vertex CurrentPointer;
    Room room;
    float x_1;
    float x_2;

    protected override Status OnStart()
    {
        Nav = Agent.Value.Navigation;

        room = Nav.CurrentRoom;

        x_1 = room.Bounds.center.x - room.Bounds.extents.x;
        x_2 = room.Bounds.center.x + room.Bounds.extents.x;

        float rand = UnityEngine.Random.Range(x_1, x_2);

        Nav.SetPointer(room, Agent.Value.transform.position.With(x:rand));

        Nav.PathQueue.Clear();

        Nav.UpdateHeading(Nav.Pointer);

        CurrentPointer = Nav.Pointer;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Nav.Origin != CurrentPointer) return Status.Running;
       
        return Status.Success;
    }
}

