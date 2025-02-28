using System.Collections.Generic;
using System.Linq;
using Unity.Behavior;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class Agent : MonoBehaviour
{
    public BehaviorGraphAgent BGAgent;
    [Header("Organs")]
    public Sensor Sensor { get; private set; }
    public Brain Brain { get; private set; }

    // Current route for the agent to follow
    public Queue<Vertex> Route { get; set; } = new Queue<Vertex>();
    public Room Room { get; set; }
    // Current station of the agent (station of origin or station of destination vertex, if travelling)
    public Station Station { get; private set; }
    // Most recently occupied vertex of the agent
    public Vertex Origin { get; private set; }
    // Current vertex agent is travelling to
    public Vertex Heading { get; private set; }
    public Vertex Pointer { get; private set; }

    public GameObject test;
    public FollowAgent Follow;
    public float Speed = 0.5f;

    private void Awake() 
    {
        BGAgent = GetComponent<BehaviorGraphAgent>();
        Brain = GetComponent<Brain>();
        Sensor = GetComponent<Sensor>();
        Pointer = GetComponent<Vertex>();
    }

    private void Update() {
        if (test) test.transform.position = Pointer.Position;
    }

    // Set the origin of the agent (current vertex)
    public void UpdateOrigin(Vertex s)
    {
        if (Origin == s) return;

        Origin = s;
        Room = s.Room;
    }

    public void UpdateHeading(Vertex u)
    {
        if (Heading == u) return;

        Heading = u;
    }

    public void SetPointer(Room room, Vector2 position)
    {
        Pointer.ConfigureVertex(room, position);
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

    public void GetNextHeading()
    {
        if (Heading == Origin && (Vector2)transform.position == Heading.Position) {

            // If there are no more vertices to travel to we can stop updating the position
            if (Route.Count == 0) 
            {
                UpdateHeading(null);
            }
            else
            {
                // Get the next vertex to travel to along the route
                UpdateHeading(Route.Dequeue());
            }
        }
    }
}
