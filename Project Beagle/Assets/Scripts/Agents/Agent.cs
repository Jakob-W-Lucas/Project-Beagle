using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Agent : MonoBehaviour
{
    public Queue<Vertex> Route { get; private set; } = new Queue<Vertex>();
    public Vertex Origin;
    public Vertex Heading;
    public float Speed = 0.5f;

    public void FollowPath(List<Vertex> vertices)
    {
        foreach (Vertex v in vertices)
        {
            Route.Enqueue(v);
        }

        Heading = Route.Dequeue();
    }
}
