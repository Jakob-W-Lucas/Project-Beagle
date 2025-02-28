using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections.Generic;
using UnityUtils;
using System.Linq;
using Unity.VisualScripting;

public static class PathMath
{
    const float Epsilon = 0.01f;
    
    public static bool IsPointBetweenPoints(Vector2 a, Vector2 b, Vector2 point)
    {
        if (Mathf.Approximately(a.x, b.x))
            return WithinBounds(a, b, point) && 
                Mathf.Abs(point.x - a.x) < Epsilon;
            
        float slope = (b.y - a.y) / (b.x - a.x);
        float intercept = a.y - slope * a.x;
        float calculatedY = slope * point.x + intercept;

        return WithinBounds(a, b, point) &&
            Mathf.Abs(point.y - calculatedY) < Epsilon;
    }

    public static bool WithinBounds(Vector2 a, Vector2 b, Vector2 point) => point.x > Mathf.Min(a.x, b.x) && point.x < Mathf.Max(a.x, b.x) &&
        point.y > Mathf.Min(a.y, b.y) && point.y < Mathf.Max(a.y, b.y);
}

public class Point
{
    public Vertex Vertex;
    public Vertex[] Between;
    public Vector2 Position;
    public bool IsPointer;
    public bool IsStation;

    public Point(Agent a, OuterMap map)
    {
        Vertex = a.Origin;
        Between = map.GetNearestRoomVertices(a);
        Position = Vertex.Position;
        IsPointer = Vertex.IsPointer;
        IsStation = Vertex.IsStation;
    }

    public Point(Vertex l, Vertex r)
    {
        Vertex = null;
        Between = new Vertex[2] { l, r };
        Position = Vertex.Position;
        IsPointer = Vertex.IsPointer;
        IsStation = Vertex.IsStation;
    }
}

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "FollowAgent", story: "[Agent] follows Target", category: "Action", id: "f2b14d4f02aa7c8ffdeba446869c9008")]
public partial class FollowAgentAction : Action
{
    [SerializeReference] public BlackboardVariable<Agent> Agent;
    [SerializeReference] public BlackboardVariable<OuterMap> Map;
    Agent Target;
    Vertex COrigin;
    public Queue<Point> Stack = new Queue<Point>();

    protected override Status OnStart()
    {
        Target = Agent.Value.Follow.Target;
        Stack.Enqueue(new Point(Target, Map));

        return Status.Running;
    }
    protected override Status OnUpdate()
    {
        Agent.Value.GetNextHeading();
        MoveAlongStack(Agent.Value.Follow.HardDistance);

        if (Target.Origin == COrigin) 
            return Status.Running;

        UpdatePathStack();
        VisualizePath();
        MaintainStackSize();

        return Status.Running;
    }

    float GetPositionOnSegment(Vector2 s, Vector2 u, float distance)
    {
        float remainingDistance = distance;

        float segmentDistance = Vector2.Distance(s, u);

        // if (Vector2.Distance(s, Agent.Value.Pointer.Position) < remainingDistance && 
        //         PathMath.IsPointBetweenPoints(s, u, Agent.Value.Pointer.Position)) return 0;

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
        Point origin = Stack.Last();
        float remainingDistance = GetPositionOnSegment(Target.transform.position, origin.Position, distance);
        
        if (remainingDistance <= 0) return;

        for (int i = Stack.Count - 1; i > 0; i--)
        {
            Point current = Stack.ElementAt(i);
            Point next = Stack.ElementAt(i - 1);
            
            remainingDistance = GetPositionOnSegment(current.Position, next.Position, remainingDistance);

            if (remainingDistance <= 0) return;
        }

        Agent.Value.SetPointer(Target.Room, Stack.First().Position);
    }

    void UpdatePathStack()
    {
        if (Target.Origin.IsPointer || Target.Origin.IsStation) return;

        Point newPoint = new Point(Target, Map);
        
        /* DUMMY CODE FOR HARD FOLLOW

        Use for a follow function where the followers will follow the exact path of the target

        if (Stack.Count > 0)
        {
            Point last = Stack.Last();
            bool shouldReplace = (Target.Origin.IsPointer || Target.Origin.IsStation) && (last.IsPointer || last.IsStation) &&
                                Vector2.Distance(newPoint.Position, Stack.ElementAt(1).Position) > 
                                Vector2.Distance(last.Position, Stack.ElementAt(1).Position);

            if (shouldReplace)
            {
                Stack.Dequeue();
                Stack.Enqueue(newPoint);
                return;
            }

            
        }

        */

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
}

