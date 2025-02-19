using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Agent : MonoBehaviour
{
    [Header("Brain")]
    public AIAction CurrentAction;
    public Sensor Sensor { get; private set; }
    public Brain Brain { get; private set; }
    public float CurrentHealth;

    // Current route for the agent to follow
    public Queue<Vertex> Route { get; private set; } = new Queue<Vertex>();
    // Current station of the agent (station of origin or station of destination vertex, if travelling)
    public Station Station;
    // Most recently occupied vertex of the agent
    public Vertex Origin { get; private set; }
    // Current vertex agent is travelling to
    public Vertex Heading;
    public float Speed = 0.5f;

    private void Awake() 
    {
        Brain = GetComponent<Brain>();
        Sensor = GetComponent<Sensor>();
    }

    // Set the origin of the agent (current vertex)
    public void UpdateOrigin(Vertex s)
    {
        if (Origin == s) return;

        Origin = s;
    }

    // Vacate current station (if it exists) and occupy the new station
    private void OccupyStation(Station st)
    {
        if (st != Station)
        {
            if (Station) Station.Vacate(this);
            
            st.Occupy(this);
            Station = st;
        }
    }

    // Vacate the current station
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
