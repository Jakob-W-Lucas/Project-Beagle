using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections.Generic;
using System.Linq;
using static UnityUtils.PathExtensions;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "FollowAgent", story: "[Agent] follows Target", category: "Action", id: "f2b14d4f02aa7c8ffdeba446869c9008")]
public partial class FollowAgentAction : Action
{
    [SerializeReference] public BlackboardVariable<Agent> Agent;
    [SerializeReference] public BlackboardVariable<OuterMap> Map;
    public Queue<Point> Stack = new Queue<Point>();
    Agent Target;
    Vertex COrigin;
    Vertex[] CurrentBetween = new Vertex[2];
    Vertex[] PointerBetween;

    protected override Status OnStart()
    {
        if (Agent.Value.Follow.Target == null) return Status.Failure;

        Target = Agent.Value.Follow.Target;

        return Status.Running;
    }
    protected override Status OnUpdate()
    {
        if (Agent.Value.Follow.Target == null) return Status.Success;

        Agent.Value.GetNextHeading();

        if (Agent.Value.Heading == null) Agent.Value.UpdateHeading(Agent.Value.Pointer);

        MoveAlongStack(Agent.Value.Follow.HardDistance);

        if (PointerBetween != null && PointerBetween.Length == 2 && PointerBetween[1] != null && PointerBetween[1].Position.x > PointerBetween[0].Position.x)
        {
            Array.Reverse(PointerBetween);
        }
        
        UpdatePath();

        if (Target.Origin == COrigin) 
            return Status.Running;

        UpdatePathStack();
        //VisualizePath();
        MaintainStackSize();

        return Status.Running;
    }

    void UpdatePath()
    {
        if (PointerBetween == null || CurrentBetween == null || Agent.Value.Between == null) return;

        if (ShouldUpdatePath())
        {
            Agent.Value.Route.Clear();
            Agent.Value.UpdateHeading(Agent.Value.Pointer);
            return;
        }

        if (CurrentBetween.SequenceEqual(PointerBetween)) return;
        
        Route best = new Route();
        foreach (Vertex v1 in Agent.Value.Between)
        {
            if (v1 == null) continue;

            foreach (Vertex v2 in PointerBetween)
            {
                if (v2 == null) continue;

                Route p_best = Map.Value.Map.Routes[v1.g_ID][v2.g_ID];
                p_best.Join(new Route(v2, Agent.Value.Pointer));
                best = CompareRoutes(best, p_best);
            }
        }

        Agent.Value.FollowPath(best);
        
        CurrentBetween = PointerBetween;
    }

    bool ShouldUpdatePath() =>
    
        Agent.Value.GetInBetween(Agent.Value.Pointer).SequenceEqual(PointerBetween) ||

        Vector2.Distance(Agent.Value.transform.position, Agent.Value.Pointer.Position) < 0.25f;
    

    float GetPositionOnSegment(Vector2 s, Vector2 u, float distance)
    {
        float remainingDistance = distance;

        float segmentDistance = Vector2.Distance(s, u);

        if (remainingDistance <= segmentDistance)
        {
            Vector2 direction = (u - s).normalized;
            Vector2 newPosition = s + direction * remainingDistance;
            Agent.Value.SetPointer(Target.Room, newPosition); 
        }

        return remainingDistance - segmentDistance;
    }

    void MoveAlongStack(float distance)
    {
        if (Stack == null || Stack.Count == 0) return;

        Vertex origin = Stack.Last().Vertex;
        float remainingDistance = GetPositionOnSegment(Target.transform.position, origin.Position, distance);
        PointerBetween = Target.Between;

        if (remainingDistance <= 0) return;

        for (int i = Stack.Count - 1; i > 0; i--)
        {
            Point current = Stack.ElementAt(i);
            Point next = Stack.ElementAt(i - 1);
            
            remainingDistance = GetPositionOnSegment(current.Position, next.Position, remainingDistance);
            PointerBetween = new Vertex[2] { current.Vertex, next.Vertex };

            if (remainingDistance <= 0) return;
        }

        Vertex first = Stack.First().Vertex;
        Agent.Value.SetPointer(Target.Room, first.Position);
        PointerBetween = new Vertex[2] { first, null };
    }

    void UpdatePathStack()
    {
        if (!Target.Origin.IsRoom) return;

        Point newPoint = new Point(Target);

        Stack.Enqueue(newPoint);
    }

    void VisualizePath()
    {
        foreach (Point p in Stack)
        {
            p.Vertex.GetComponent<SpriteRenderer>().color = 
                (p.IsPointer || p.IsStation) ? Color.blue : Color.red;
        }
    }

    void MaintainStackSize()
    {
        while (Stack.Count > 10)
        {
            Point removed = Stack.Dequeue();
            removed.Vertex.GetComponent<SpriteRenderer>().color = Color.gray;
        }
        
        COrigin = Target.Origin;
    }

    protected override void OnEnd()
    {
        Agent.Value.Between = PointerBetween;
    }
}