using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections.Generic;
using UnityUtils;
using System.Linq;
using Unity.VisualScripting;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "FollowAgent", story: "[Agent] follows Target", category: "Action", id: "f2b14d4f02aa7c8ffdeba446869c9008")]
public partial class FollowAgentAction : Action
{
    [SerializeReference] public BlackboardVariable<Agent> Agent;
    [SerializeReference] public BlackboardVariable<OuterMap> Map;
    Agent Target;
    Vertex COrigin;
    public Queue<Vertex> Stack = new Queue<Vertex>();

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

        Stack.Enqueue(Ends[0]);
        Stack.Enqueue(Ends[1]);

        COrigin = Target.Origin;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Agent.Value.GetNextHeading();

        MoveAlongStack(Agent.Value.Follow.HardDistance);

        if (Target.Origin == COrigin) return Status.Running;

        if (Target.Origin.g_ID == -1 || Target.Origin.r_ID == -1) return Status.Running;

        if (Stack.Count > 5)
        {
            Vertex vertex = Stack.Dequeue();
            vertex.GetComponent<SpriteRenderer>().color = Color.gray;
        }
        
        Stack.Enqueue(Target.Origin);
        
        foreach (Vertex v in Stack)
        {
            v.GetComponent<SpriteRenderer>().color = Color.red;
        }

        COrigin = Target.Origin;

        return Status.Running;
    }

    void MoveAlongStack(float distance)
    {
        float remainingDistance = distance;
        float segmentDistance = Vector2.Distance(Target.transform.position, Stack.Last().Position);

        if (remainingDistance - segmentDistance <= 0)
        {
            Vector2 direction = (Target.Origin.Position - (Vector2)Target.transform.position).normalized;
            Vector2 newPosition = (Vector2)Target.transform.position + direction * remainingDistance;
            Agent.Value.SetPointer(Target.Room, newPosition);
            return;
        }

        remainingDistance -= segmentDistance;

        for (int i = Stack.Count - 1; i > 0; i--)
        {
            Vertex current = Stack.ElementAt(i);
            Vertex next = Stack.ElementAt(i - 1);
            
            segmentDistance = Vector2.Distance(current.Position, next.Position);

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

    void DummyFollow()
    {
        List<Vertex> options = new List<Vertex>() { Agent.Value.Origin, Agent.Value.Heading };
        Route best = new Route();
        foreach (Vertex v in options)
        {   
            if (v == null) continue;

            Route p_best = Map.Value.CompareRoutes(
                Map.Value.GetRoute(v, Ends[0]),
                Map.Value.GetRoute(v, Ends[1])
            );

            best = Map.Value.CompareRoutes(best, p_best);
        }

        Agent.Value.FollowPath(best);
    }
}

