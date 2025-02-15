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

    public void FollowPath(Route route)
    {
        Route.Clear();
        Heading = null;

        foreach (Vertex v in route.Vertices)
        {
            Route.Enqueue(v);
        }

        Heading = Route.Dequeue();
    }
}
