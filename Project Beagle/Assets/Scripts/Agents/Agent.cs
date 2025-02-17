using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Agent : MonoBehaviour
{
    public Queue<Vertex> Route { get; private set; } = new Queue<Vertex>();
    public Station Station;
    public Vertex Origin { get; private set; }
    public Vertex Heading;
    public float Speed = 0.5f;

    // Set the origin of the agent (current vertex)
    public void UpdateOrigin(Vertex s)
    {
        Origin = s;
        
        if (s.TryGetComponent<Station>(out var st))
        {
            Station = st;
            return;
        }

        Station = null;
    }

    // Creates an agent path to follow
    public void FollowPath(Route route)
    {
        if (route == null) return;
        
        Route.Clear();
        Heading = null;

        // Queue all of the vertices to travel 
        foreach (Vertex v in route.Vertices)
        {
            Route.Enqueue(v);
        }

        // Begin the pathfinding
        Heading = Route.Dequeue();
    }
}
