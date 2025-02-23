using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "TrackAgent", story: "[Agent] tracks Target", category: "Action", id: "8660a7da1d1bac7fb16d69beef4e4663")]
public partial class TrackAgentAction : Action
{
    [SerializeReference] public BlackboardVariable<Agent> Agent;
    [SerializeReference] public BlackboardVariable<OuterMap> Map;
    Agent Target;
    Vertex CurrentTargetOrigin;

    protected override Status OnStart()
    {
        Target = Agent.Value.Target;
        
        if (Target == null) return Status.Failure;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
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

        if (Target.Origin == CurrentTargetOrigin) return Status.Running;

        CurrentTargetOrigin = Target.Origin;

        if (Target.Room == Agent.Value.Room)
        {
            Agent.Value.FollowPath(new Route(Agent.Value.Origin, Target.Origin));
            return Status.Running;
        }

        if (CurrentTargetOrigin.Station) 
        {
            Agent.Value.FollowPath(
                Map.Value.TravelToStation(
                    Agent.Value.Origin, 
                    null, 
                    CurrentTargetOrigin.Station
                )
            );

            return Status.Running;
        }

        Agent.Value.FollowPath(
            Map.Value.TravelToRoom(
                Agent.Value.Origin, 
                null, 
                CurrentTargetOrigin.Room
            )
        );
        
        return Status.Running;
    }
}

