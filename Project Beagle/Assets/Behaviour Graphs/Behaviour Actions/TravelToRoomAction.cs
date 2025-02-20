using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "TravelToRoom", story: "[Agent] Travels to [Room]", category: "Action", id: "0828c149b271b9b7e3b6d14ed37cc6aa")]
public partial class TravelToRoomAction : Action
{
    [SerializeReference] public BlackboardVariable<OuterMap> Map;
    [SerializeReference] public BlackboardVariable<Agent> Agent;
    [SerializeReference] public BlackboardVariable<Room> Room;

    protected override Status OnStart()
    {
        if (!Room.Value) return Status.Failure;

        Route originRoute = Map.Value.TravelToRoom(Agent.Value.Origin, Room.GetType());

        if (!Agent.Value.Heading) {
            Agent.Value.FollowPath(originRoute);
            return Status.Running;
        }

        Route headingRoute = Map.Value.TravelToRoom(Agent.Value.Heading, Room.GetType());

        Route bestRoute = originRoute.Distance < headingRoute.Distance ? originRoute : headingRoute;

        Agent.Value.FollowPath(bestRoute);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Agent.Value.Route.Count > 0) {
            return Status.Running;
        }

        return Status.Success;
    }

    protected override void OnEnd()
    {
        Debug.Log($"Agent {Agent.Value.name} has arrived at {Room.Value.name}");
    }
}

