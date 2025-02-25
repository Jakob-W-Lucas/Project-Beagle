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
    Vertex CHeading;
    Room CRoom;

    Vertex[] Ends = new Vertex[2];

    protected override Status OnStart()
    {
        Target = Agent.Value.Target;

        if (!Target.Heading || Target.Origin.Room == Target.Heading.Room)
        {
            Ends = new Vertex[2] { Target.Room.Vertices.First(), Target.Room.Vertices.Last() };
        }
        else
        {
            Ends = new Vertex[2] { Target.Origin, Target.Heading };
        }

        COrigin = Target.Origin;
        CHeading = Target.Heading;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Target.Origin == COrigin && Target.Heading == CHeading) return Status.Running;

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

        COrigin = Target.Origin;
        CHeading = Target.Heading;

        return Status.Running;
    }
}

