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
    NavigationState Nav;
    protected override Status OnStart()
    {
        Nav = Agent.Value.Navigation;
        Route route = Map.Value.Travel(Agent.Value, Station);

        if (route.Vertices.Count == 0) return Status.Failure;

        Nav.FollowPath(route);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Nav.Origin.Station && Nav.Origin.Station.Type == Station.Value) return Status.Success;

        Nav.MoveThroughPath();

        return Status.Running;
    }
}

