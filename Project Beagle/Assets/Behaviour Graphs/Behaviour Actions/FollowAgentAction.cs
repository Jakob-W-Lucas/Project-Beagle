using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Linq;
using System.Collections.Generic;
using Unity.VisualScripting;

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
        if (Agent.Value.Heading == Agent.Value.Origin && (Vector2)Agent.Value.transform.position == Agent.Value.Origin.Position) {

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

        if (Target.Origin.Position == (Vector2)Agent.Value.transform.position) 
        {
            Agent.Value.Route = new Queue<Vertex>(Target.Route);
            Agent.Value.UpdateHeading(Target.Heading);

            return Status.Running;
        }

        ChangeRoutes();
        
        return Status.Running;
    }

    void ChangeRoutes()
    {
        Vertex current = null;
        if (Target.Route.Count == 0)
        {
            current = Target.Heading ? Target.Heading : Target.Origin;
        }
        else
        {
            current = Target.Route.Last();
        }

        if (Target.Heading == CurrentTargetDestination) return;

        CurrentTargetDestination = Target.Heading;
        
        Route originRoute = Map.Value.TravelToVertex(Agent.Value.Origin, CurrentTargetDestination);
        
        if (Agent.Value.Heading == null) {

            if (originRoute == null) return;

            Agent.Value.FollowPath(originRoute);
            return;
        }

        Route headingRoute = Map.Value.TravelToVertex(Agent.Value.Heading, CurrentTargetDestination);

        if (originRoute == null || headingRoute == null) return;

        Route bestRoute = originRoute.Distance < headingRoute.Distance ? originRoute : headingRoute;

        Agent.Value.FollowPath(bestRoute);
    }
}

