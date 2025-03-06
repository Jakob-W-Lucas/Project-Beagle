using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Health))]
public class Agent : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private float _speed = 0.5f;
    
    [Header("Dependencies")]
    [SerializeField] private BehaviorGraphAgent _behaviorAgent;
    [SerializeField] private FollowAgent _followAgentConfig;

    public NavigationState Navigation { get; private set; }
    public Sensor Sensor { get; private set; }
    public Brain Brain { get; private set; }
    public Room CurrentRoom => Navigation.CurrentRoom;
    public FollowAgent FollowConfig => _followAgentConfig;

    public void SetState(ActionState actionState) => _behaviorAgent.Graph.BlackboardReference.SetVariableValue("State", actionState);

    private void Awake() 
    {
        _behaviorAgent = GetComponent<BehaviorGraphAgent>();
        Brain = GetComponent<Brain>();
        Sensor = GetComponent<Sensor>();
        Navigation = new NavigationState(this, _speed);
    }
}

[System.Serializable]
public class NavigationState
{
    private Agent _a;
    public Queue<Vertex> Route { get; } = new Queue<Vertex>();
    public Vertex Origin { get; private set; }
    public Vertex Heading { get; private set; }
    public Vertex Pointer { get; private set; }
    public Station CurrentStation { get; private set; }
    public Room CurrentRoom { get; private set; }
    public Vertex[] PathSegment;
    private readonly float _movementSpeed;
    public float MovementSpeed => _movementSpeed;

    public NavigationState(Agent a, float speed)
    {
        _a = a;
        Pointer = a.GetComponent<Vertex>();
        _movementSpeed = speed;
    }
    // Set the origin of the agent (current vertex)
    public void UpdateOrigin(Vertex s)
    {
        if (Origin == s) return;

        Origin = s;
        CurrentRoom = s.Room;
    }

    public void UpdateHeading(Vertex u)
    {
        if (Heading == u) return;

        Heading = u;

        if (u) PathSegment = GetInBetween(u);
    }

    public void SetPointer(Room room, Vector2 position)
    {
        Pointer.ConfigureVertex(room, position);
    }

    // Vacate current station (if it exists) and occupy the new station
    private void OccupyStation(Station st)
    {
        if (st != CurrentStation)
        {
            if (CurrentStation) CurrentStation.Vacate(_a);
            
            st.Occupy(_a);
            CurrentStation = st;
        }
    }

    // Vacate the current station
    private void VacateStation()
    {
        if (CurrentStation)
        {
            CurrentStation.Vacate(_a);
            CurrentStation = null;
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
        if (Heading == Origin && (Vector2)_a.transform.position == Heading.Position) {

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

    public Vertex[] GetInBetween(Vertex u)
    {
        Vector2 pos = (Vector2)_a.transform.position;
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
}
