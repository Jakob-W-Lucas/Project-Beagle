using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityUtils;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MoveWithinRoom", story: "[Agent] moves within Room", category: "Action", id: "4cd7b7849d7ff515c81d1274c3038d9e")]
public partial class MoveWithinRoomAction : Action
{
    [SerializeReference] public BlackboardVariable<Agent> Agent;
    Room Room;

    protected override Status OnStart()
    {
        Room = Agent.Value.Room;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        // if (Agent.Value.Heading != null) return Status.Running;

        // float x_1 = Room.Bounds.center.x - Room.Bounds.extents.x;
        // float x_2 = Room.Bounds.center.x + Room.Bounds.extents.x;

        // float rand = UnityEngine.Random.Range(x_1, x_2);

        // Agent.Value.UpdateHeading(new Vertex(Agent.Value.transform.position.With(x:rand), Room, -1, -1));

        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

