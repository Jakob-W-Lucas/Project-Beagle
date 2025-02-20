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
    [SerializeReference] public BlackboardVariable<RoomType> Room;

    protected override Status OnStart()
    {
        Route originRoute = Map.Value.TravelToRoom(Agent.Value.Origin, Room);

        if (!Agent.Value.Heading) {

            if (originRoute == null) return Status.Failure;

            Agent.Value.FollowPath(originRoute);
            return Status.Running;
        }

        Route headingRoute = Map.Value.TravelToRoom(Agent.Value.Heading, Room);

        if (originRoute == null || headingRoute == null) return Status.Failure;

        Route bestRoute = originRoute.Distance < headingRoute.Distance ? originRoute : headingRoute;

        Agent.Value.FollowPath(bestRoute);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Agent.Value.Origin == Agent.Value.Heading) {
            // Get the next vertex to travel to along the route
            Agent.Value.UpdateHeading(Agent.Value.Route.Dequeue());
        }

        // If there are no more vertices to travel to we can stop updating the position
        if (Agent.Value.Route.Count == 0) {
            Agent.Value.UpdateHeading(null);
            return Status.Success;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        Debug.Log($"Agent {Agent.Value.name} has arrived at {Room.Value}");
    }
}

