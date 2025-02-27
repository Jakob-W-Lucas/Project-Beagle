using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections.Generic;
using UnityUtils;
using System.Linq;
using Unity.VisualScripting;

public class Point
{
    public Vertex Vertex;
    public Vector2 Position;
    public bool IsPointer;
    public bool IsStation;

    public Point(Vertex v)
    {
        Vertex = v;
        Position = v.Position;
        IsPointer = v.IsPointer;
        IsStation = v.IsStation;
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

    Vertex[] Ends = new Vertex[2];

    protected override Status OnStart()
    {
        Target = Agent.Value.Follow.Target;

        if (!Target.Heading || Target.Origin.Room == Target.Heading.Room)
        {
            Ends = new Vertex[2] { Target.Room.Vertices.First(), Target.Room.Vertices.Last() };
        }
        else
        {
            Ends = new Vertex[2] { Target.Origin, Target.Heading };
        }

        Stack.Enqueue(new Point(Ends[0]));
        //Stack.Enqueue(Ends[1]);

        COrigin = Target.Origin;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Agent.Value.GetNextHeading();

        MoveAlongStack(Agent.Value.Follow.HardDistance);

        if (Target.Origin == COrigin) return Status.Running;

        if (Target.Origin.IsPointer)
        {
            if (Stack.Count > 0 && Stack.Last().IsPointer)
            {
                if (Vector2.Distance(Target.Origin.Position, Stack.ElementAt(1).Position) > 
                Vector2.Distance(Stack.Last().Position, Stack.ElementAt(1).Position))
                {
                    Stack.Dequeue();
                    Stack.Enqueue(new Point(Target.Origin));
                }
            }
            else
            {
                Stack.Enqueue(new Point(Target.Origin));
            }
        }
        else if (Stack.Count > 0 && Stack.Last().IsPointer)
        {
            Stack.Dequeue();
        }

        if (Target.Origin.IsStation)
        {
            if (Stack.Count > 0 && Stack.Last().IsStation)
            {
                if (Vector2.Distance(Target.Origin.Position, Stack.ElementAt(1).Position) > 
                Vector2.Distance(Stack.Last().Position, Stack.ElementAt(1).Position))
                {
                    Stack.Dequeue();
                    Stack.Enqueue(new Point(Target.Origin));
                }
            }
            else
            {
                Stack.Enqueue(new Point(Target.Origin));
            }
        }
        else if (Stack.Count > 0 && Stack.Last().IsStation)
        {
            Stack.Dequeue();
        }

        if (Stack.Count > 10)
        {
            Vertex vertex = Stack.Dequeue().Vertex;
            vertex.GetComponent<SpriteRenderer>().color = Color.gray;
        }
        
        Stack.Enqueue(new Point(Target.Origin));
        
        foreach (Point p in Stack)
        {
            p.Vertex.GetComponent<SpriteRenderer>().color = Color.red;
        }

        COrigin = Target.Origin;

        return Status.Running;
    }

    void MoveAlongStack(float distance)
    {
        float remainingDistance = distance;

        Point origin = Stack.Last();
        float segmentDistance = Vector2.Distance(Target.transform.position, origin.Position);

        if (Vector2.Distance(Target.transform.position, Agent.Value.Pointer.Position) < remainingDistance && 
                POLine(Target.transform.position, origin.Position, Agent.Value.Pointer.Position)) return;

        if (remainingDistance - segmentDistance <= 0)
        {
            Vector2 direction = (origin.Position - (Vector2)Target.transform.position).normalized;
            Vector2 newPosition = (Vector2)Target.transform.position + direction * remainingDistance;
            Agent.Value.SetPointer(Target.Room, newPosition);
            return;
        }

        remainingDistance -= segmentDistance;

        for (int i = Stack.Count - 1; i > 0; i--)
        {
            Point current = Stack.ElementAt(i);
            Point next = Stack.ElementAt(i - 1);
            
            segmentDistance = Vector2.Distance(current.Position, next.Position);

            if (Vector2.Distance(current.Position, Agent.Value.Pointer.Position) < remainingDistance && 
                POLine(current.Position, next.Position, Agent.Value.Pointer.Position)) return;
            
            if (remainingDistance - segmentDistance <= 0)
            {
                Vector2 direction = (next.Position - current.Position).normalized;
                Vector2 newPosition = current.Position + direction * remainingDistance;
                Agent.Value.SetPointer(Target.Room, newPosition);
                return;
            }

            remainingDistance -= segmentDistance;
        }

        if (remainingDistance > 0)
        {
            Agent.Value.SetPointer(Target.Room, Stack.ElementAt(0).Position);
        }
    }

    bool POLine(Vector2 pos1, Vector2 pos2, Vector2 point)
    {
        float m = (pos2.y - pos1.y) / (pos2.x - pos1.x);
        float c = pos1.y - m * pos1.x;

        return point.y == m * point.x + c;
    }

    void SetVertexColor()
    {
        if (Ends[0]) Ends[0].GetComponent<SpriteRenderer>().color = Color.gray;
        if (Ends[1]) Ends[1].GetComponent<SpriteRenderer>().color = Color.gray;

        if (!Target.Heading || Target.Origin.Room == Target.Heading.Room)
        {
            Ends = new Vertex[2] { Target.Room.Vertices.First(), Target.Room.Vertices.Last() };
        }
        else
        {
            Ends = new Vertex[2] { Target.Origin, Target.Heading };
        }

        Ends[0].GetComponent<SpriteRenderer>().color = Color.blue;
        Ends[1].GetComponent<SpriteRenderer>().color = Color.red;
    }
}

