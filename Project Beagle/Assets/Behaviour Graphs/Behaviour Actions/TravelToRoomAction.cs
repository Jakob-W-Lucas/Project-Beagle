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
        Route originRoute = Map.Value.TravelToRoom(Agent.Value.Origin, Room);

        if (Agent.Value.Heading == null) {

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
        if (Agent.Value.Room && Agent.Value.Room.Type == Room.Value) return Status.Success;

        if (Agent.Value.Origin == Agent.Value.Heading) {

            // If there are no more vertices to travel to we can stop updating the position
            if (Agent.Value.Route.Count == 0) 
            {
                Agent.Value.UpdateHeading(null);
            }
            else
            {
                // Get the next vertex to travel to along the route
                Agent.Value.UpdateHeading(Agent.Value.Route.Dequeue());
            }
        }

        return Status.Running;
    }
}

