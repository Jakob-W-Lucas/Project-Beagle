using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections.Generic;
using System.Linq;
using UnityUtils;
using Unity.VisualScripting;
using UnityEngine.UIElements;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "FollowAgent", story: "[Agent] follows Target", category: "Action", id: "f2b14d4f02aa7c8ffdeba446869c9008")]
public partial class FollowAgentAction : Action
{
    [SerializeReference] public BlackboardVariable<Agent> Agent;
    [SerializeReference] public BlackboardVariable<OuterMap> Map;
    private NavigationState Nav;
    public Queue<Point> Stack = new Queue<Point>();
    Agent Target;
    Vertex COrigin;
    Edge CurrentEdge;
    Vertex[] CurrentBetween;
    Vertex[] LastBetween;
    Vertex[] lastVertices = new Vertex[2];

    protected override Status OnStart()
    {
        Nav = Agent.Value.Navigation;

        if (Agent.Value.FollowConfig.Target == null) return Status.Failure;

        Target = Agent.Value.FollowConfig.Target;

        return Status.Running;
    }
    protected override Status OnUpdate()
    {
        if (Agent.Value.FollowConfig.Target == null) return Status.Success;

        
        
        Nav.MoveThroughPath();

        MoveAlongStack(Agent.Value.FollowConfig.HardDistance);

        UpdatePath();

        // Only update the stack if the Target has moved past a new room node
        if (Target.Navigation.Origin == COrigin) 
            return Status.Running;

        UpdatePathStack();
        MaintainStackSize();

        return Status.Running;
    }

    void UpdatePath()
    {
        Edge e = Map.Value.QuadTreeInstance.IsPointOnEdge(Nav.Pointer.Position);

        if (ShouldRecalculate(e))
        {
            string str = Vector2.Distance(Agent.Value.transform.position, Nav.Pointer.Position) < 0.02f ? $"It is because of the distance check: Agent: {Agent.Value.transform.position}, Pointer: {Nav.Pointer.Position}" : 
                $"It is because of the edge check. Agent: {Nav?.CurrentEdge?.Start}->{Nav?.CurrentEdge?.End}, Pointer: {e?.Start} -> {e?.End})";
            //Debug.Log("Agent has decided to move to pointer. " + str);
            Nav.PathQueue.Clear();
            Nav.UpdateHeading(Nav.Pointer);
            return;
        }

        if (e != null && CurrentEdge == e) return;
        CurrentEdge = e;

        Nav.FollowPath(Map.Value.Travel(Agent.Value, Target));
    }

    bool ShouldRecalculate(Edge edge) =>
        (Nav != null &&
        Nav.CurrentEdge != null &&
        Nav.CurrentEdge == edge) ||
        Vector2.Distance(Agent.Value.transform.position, Nav.Pointer.Position) < 0.05f;

    
    float GetPositionOnSegment(Vector2 s, Vector2 u, float distance)
    {
        float remainingDistance = distance;

        float segmentDistance = Vector2.Distance(s, u);

        if (remainingDistance <= segmentDistance)
        {
            Vector2 direction = (u - s).normalized;
            Vector2 newPosition = s + direction * remainingDistance;
            Nav.SetPointer(Target.Navigation.CurrentRoom, newPosition); 
        }

        return remainingDistance - segmentDistance;
    }

    void MoveAlongStack(float distance)
    {
        if (Stack == null || Stack.Count == 0) return;

        Vertex origin = Stack.Last().Vertex;
        float remainingDistance = GetPositionOnSegment(Target.transform.position, origin.Position, distance);

        if (remainingDistance <= 0) return;

        for (int i = Stack.Count - 1; i > 0; i--)
        {
            Point current = Stack.ElementAt(i);
            Point next = Stack.ElementAt(i - 1);
            
            remainingDistance = GetPositionOnSegment(current.Position, next.Position, remainingDistance);

            if (remainingDistance <= 0) return;
        }

        Vertex first = Stack.First().Vertex;
        Nav.SetPointer(Target.Navigation.CurrentRoom, first.Position);
    }

    void UpdatePathStack()
    {
        if (!Target.Navigation.Origin.IsRoom) return;

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
        
        COrigin = Target.Navigation.Origin;
    }

    // protected override void OnEnd()
    // {
    //     Nav.PathSegment = PointerBetween;
    // }

    // void UpdatePathStack()
    // {
    //     if (!Target.Navigation.Origin.IsRoom) return;

    //     Point newPoint = new Point(Target);
    //     if (Stack.Count > 0)
    //     {
    //         Point last = Stack.Last();
    //         bool shouldReplace = (Target.Navigation.Origin.IsPointer || Target.Navigation.Origin.IsStation) && (last.IsPointer || last.IsStation) &&
    //                             Vector2.Distance(newPoint.Position, Stack.ElementAt(1).Position) > 
    //                             Vector2.Distance(last.Position, Stack.ElementAt(1).Position);

    //         if (shouldReplace)
    //         {
    //             Stack.Dequeue();
    //             Stack.Enqueue(newPoint);
    //             return;
    //         }
    //     }

    //     Stack.Enqueue(newPoint);
    // }
}