using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "TravelToStation", story: "[Agent] Travels to Station [Station]", category: "Action", id: "b10e0383a378c7f80360616ad919e89a")]
public partial class TravelToStationAction : Action
{
    [SerializeReference] public BlackboardVariable<Agent> Agent;
    [SerializeReference] public BlackboardVariable<StationType> Station;
    [SerializeReference] public BlackboardVariable<OuterMap> Map;
    protected override Status OnStart()
    {
        Route originRoute = Map.Value.TravelToStation(Agent.Value.Origin, Station);

        if (Agent.Value.Heading == null) {

            if (originRoute == null) return Status.Failure;

            Agent.Value.FollowPath(originRoute);
            return Status.Running;
        }

        Route headingRoute = Map.Value.TravelToStation(Agent.Value.Heading, Station);

        if (originRoute == null || headingRoute == null) return Status.Failure;

        Route bestRoute = originRoute.Distance < headingRoute.Distance ? originRoute : headingRoute;

        Agent.Value.FollowPath(bestRoute);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Agent.Value.Origin.Station && Agent.Value.Origin.Station.Type == Station.Value) 
        {
            Debug.Log($"Agent station: {Agent.Value.Station.Type} and type: {Station.Value}");
            return Status.Success;
        }

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

