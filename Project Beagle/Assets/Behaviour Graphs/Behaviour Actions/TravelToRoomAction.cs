using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Unity.VisualScripting;
using System.Linq;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "TravelToRoom", story: "[Agent] Travels to Room [Room]", category: "Action", id: "0828c149b271b9b7e3b6d14ed37cc6aa")]
public partial class TravelToRoomAction : Action
{
    [SerializeReference] public BlackboardVariable<Agent> Agent;
    [SerializeReference] public BlackboardVariable<RoomType> Room;
    [SerializeReference] public BlackboardVariable<OuterMap> Map;
    protected override Status OnStart()
    {
        Agent.Value.FollowPath(Map.Value.Travel(Agent.Value, Room));

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Agent.Value.Room && Agent.Value.Room.Type == Room.Value) return Status.Success;

        Agent.Value.GetNextHeading();

        return Status.Running;
    }
}

