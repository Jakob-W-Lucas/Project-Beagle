using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class Agent : MonoBehaviour
{
    [Header("Behaviour Agent")]
    [SerializeField] private BehaviorGraphAgent _behaviourAgent;

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
    public Vertex[] Between;
    public FollowAgent Follow;
    public Agent temp;
    public float Speed = 0.5f;

    # region "Initialization"

    private void Awake() 
    {
        _behaviourAgent = GetComponent<BehaviorGraphAgent>();
        Brain = GetComponent<Brain>();
        Sensor = GetComponent<Sensor>();
        Pointer = GetComponent<Vertex>();

        temp = Follow.Target;
    }

    # endregion

    # region "Travel"
    
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

        if (u) Between = GetInBetween(u);
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

    # endregion

    # region "Utility"

    public void SetState(ActionState actionState) => _behaviourAgent.Graph.BlackboardReference.SetVariableValue("State", actionState);

    public Vertex[] GetInBetween(Vertex u)
    {
        Vector2 pos = (Vector2)transform.position;
        LayerMask layerMask = LayerMask.GetMask("Room");
        Vector2 direction = (u.Position - pos).normalized;

        // Cast forward ray (in direction away from u)
        RaycastHit2D f_hit = Physics2D.Raycast(pos, -direction, 20f, layerMask);
        Vertex forward = f_hit.collider?.GetComponent<Vertex>();

        // Cast backward ray (towards u) with improved origin offset
        Vertex backward = null;
        if (f_hit.collider != null)
        {
            RaycastHit2D[] b_hits = Physics2D.RaycastAll(pos, direction, 20f, layerMask);
            
            foreach (var hit in b_hits)
            {
                // Compare colliders instead of hit objects
                if (hit.collider != null && hit.collider != f_hit.collider)
                {
                    backward = hit.collider.GetComponent<Vertex>();
                    break;
                }
            }
        }

        return new Vertex[2] { forward, backward };
    }

    # endregion
}
