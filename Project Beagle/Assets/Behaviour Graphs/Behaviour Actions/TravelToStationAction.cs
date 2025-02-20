using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "TravelToStation", story: "[Agent] Travels to [Station]", category: "Action", id: "b10e0383a378c7f80360616ad919e89a")]
public partial class TravelToStationAction : Action
{
    [SerializeReference] public BlackboardVariable<OuterMap> Map;
    [SerializeReference] public BlackboardVariable<Agent> Agent;
    [SerializeReference] public BlackboardVariable<Station> Station;

    protected override Status OnStart()
    {
        if (!Station.Value) return Status.Failure;

        Route originRoute = Map.Value.TravelToStation(Agent.Value.Origin, Station.GetType());

        if (!Agent.Value.Heading) {
            Agent.Value.FollowPath(originRoute);
            return Status.Running;
        }

        Route headingRoute = Map.Value.TravelToStation(Agent.Value.Heading, Station.GetType());

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
        Debug.Log($"Agent {Agent.Value.name} has arrived at {Station.Value.Vertex.Name}");
    }
}

