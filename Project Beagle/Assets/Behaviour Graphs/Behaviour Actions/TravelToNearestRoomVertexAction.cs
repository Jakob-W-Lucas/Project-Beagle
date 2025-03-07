using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections.Generic;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "TravelToNearestRoomVertex", story: "[Agent] travels to nearest room vertex", category: "Action", id: "c6a6ed3c8e9571d51edec9eb2de2a191")]
public partial class TravelToNearestRoomVertexAction : Action
{
    [SerializeReference] public BlackboardVariable<Agent> Agent;
    [SerializeReference] public BlackboardVariable<OuterMap> Map;
    NavigationState Nav;
    Room room;
    List<Vertex> vertices;

    protected override Status OnStart()
    {
        if (!ValidateDependencies()) return Status.Failure;
        
        Nav = Agent.Value.Navigation;
        // If wanting to use this function later for something else, this should be removed
        // As is exists for initialization
        if (Nav.Origin) return Status.Success;

        room = Nav.CurrentRoom;

        vertices = 
            room == null ? 
            vertices = new List<Vertex>{ Map.Value.Map.GetNearestVertex(Agent.Value.transform.position) } :
            vertices = room.NearestWithinRoom(Agent.Value.transform.position);

        return Status.Running;
    }

    private bool ValidateDependencies()
    {
        if (Map == null)
        {
            Debug.LogError("Missing outer map configuration!");
        }

        return Map != null;
    }

    protected override Status OnUpdate()
    {
        if (Nav.Origin == vertices[0]) {
            Nav.UpdateOrigin(vertices[0]);
            return Status.Success;
        }

        if (Nav.Heading != vertices[0])
        {
            Nav.UpdateHeading(vertices[0]);
        }

        return Status.Running;
    }
}

