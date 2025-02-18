using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        if (Origin == s) return;

        Origin = s;
    }

    private void OccupyStation(Station st)
    {
        if (st != Station)
        {
            if (Station) Station.Vacate(this);
            
            st.Occupy(this);
            Station = st;
        }
    }

    private void VacateStation()
    {
        if (Station)
        {
            Station.Vacate(this);
            Station = null;
        }
    }

    // Creates an agent path to follow
    public void FollowPath(Route route)
    {
        if (route == null || route.Vertices.Count == 0) return;
        
        Route.Clear();
        Heading = null;

        // Queue all of the vertices to travel 
        foreach (Vertex v in route.Vertices)
        {
            Route.Enqueue(v);
        }

        // Begin the pathfinding
        Heading = Route.Dequeue();

        // Ensure the destination station is changed prior to travelling
        Station st = route.Vertices.Last().Station;
        if (st)
        {
            OccupyStation(st);
            return;
        }

        VacateStation();
    }
}
