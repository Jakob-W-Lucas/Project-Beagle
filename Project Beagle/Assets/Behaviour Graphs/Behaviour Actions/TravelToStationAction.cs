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
        Agent.Value.FollowPath(Map.Value.Travel(Agent.Value, Station));

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Agent.Value.Origin.Station && Agent.Value.Origin.Station.Type == Station.Value) return Status.Success;

        Agent.Value.GetNextHeading();

        return Status.Running;
    }
}

