using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Linq;
using System.Collections.Generic;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "FollowAgent", story: "[Agent] follows Target", category: "Action", id: "f2b14d4f02aa7c8ffdeba446869c9008")]
public partial class FollowAgentAction : Action
{
    [SerializeReference] public BlackboardVariable<Agent> Agent;
    [SerializeReference] public BlackboardVariable<OuterMap> Map;
    Agent Target;
    Vertex CurrentTargetDestination;
    bool caught;

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

        if (caught)
        {
            Agent.Value.Route = new Queue<Vertex>(Target.Route);
            Agent.Value.UpdateHeading(Target.Heading);
            
            return Status.Running;
        }

        if (Target.Origin == Agent.Value.Origin) 
        {
            caught = true;
            return Status.Running;
        }

        if (Target.Route.Last() != CurrentTargetDestination) ChangeRoutes();

        return Status.Running;
    }

    void ChangeRoutes()
    {
        CurrentTargetDestination = Target.Route.Last();

        if (Target.Room == Agent.Value.Room)
        {
            Agent.Value.FollowPath(new Route(Agent.Value.Origin, Target.Origin));
            return;
        }

        if (CurrentTargetDestination.Station) 
        {
            Agent.Value.FollowPath(
                Map.Value.TravelToStation(
                    Agent.Value.Origin, 
                    null, 
                    CurrentTargetDestination.Station
                )
            );

            return;
        }

        Agent.Value.FollowPath(
            Map.Value.TravelToRoom(
                Agent.Value.Origin, 
                null, 
                CurrentTargetDestination.Room
            )
        );
    }
}

