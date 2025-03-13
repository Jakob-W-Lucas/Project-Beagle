using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.UI;
using Unity.Behavior;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Health), typeof(Rigidbody2D))]
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

    public ActionState GetState()
    {
        if (_behaviorAgent == null) return ActionState.None;

        ActionState state; 
        _behaviorAgent.Graph.BlackboardReference.GetVariableValue("State", out state);
        return state;
    }
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
    public Queue<Vertex> PathQueue { get; } = new Queue<Vertex>();
    public Vertex Origin { get; private set; }
    public Vertex Heading { get; private set; }
    public Vertex Pointer { get; private set; }
    public Station CurrentStation { get; private set; }
    public Room CurrentRoom { get; private set; }
    private readonly float _movementSpeed;
    public float MovementSpeed => _movementSpeed;
    public Edge CurrentEdge { get; private set; }
    public Vertex[] PathSegment { get; private set; }
    public Vertex[] LastPathSegment;

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
    }

    public void SetPointer(Room room, Vector2 position)
    {
        Pointer.ConfigureVertex(room, position);
    }

    public void UpdatePathSegment(Edge e)
    {   
        if (e == null || e.Start == null || e.End == null) 
        {
            PathSegment = new Vertex[2] { Origin, Origin };
            return;
        }

        Vertex[] vertices = e.Vertices;

        if (vertices[1].Position.x < vertices[0].Position.x)
        {
            vertices.Reverse();
        }
        else if (vertices[1].Position.x == vertices[0].Position.x &&
            vertices[1].Position.y < vertices[0].Position.y)
        {
            vertices.Reverse();
        }
        
        CurrentEdge = e;
        PathSegment = vertices;
    }

    private void UpdateStationOccupation(Station newStation)
    {
        if (CurrentStation == newStation) return;
        
        CurrentStation?.Vacate(_a);
        newStation?.Occupy(_a);
        CurrentStation = newStation;
    }

    // Creates an agent path to follow
    public void FollowPath(Route route)
    {
        if (route?.Vertices.Count == 0) return;

        PathQueue.Clear();
        Heading = null;

        // Queue all of the vertices to travel 
        foreach (Vertex v in route.Vertices)
        {
            PathQueue.Enqueue(v);
        }

        // Begin the pathfinding
        Heading = PathQueue.Dequeue();
        UpdateStationOccupation(route.Vertices.Last().Station);
    }

    public void MoveThroughPath()
    {
        if (Heading != Origin || (Vector2)_a.transform.position != Heading.Position) return;

        UpdateHeading(PathQueue.Count > 0 ? PathQueue.Dequeue() : null);
    }

    public void DebugPathSegment()
    {
        if (LastPathSegment != null && PathSegment != null && LastPathSegment.SequenceEqual(PathSegment)) return;

        if (LastPathSegment != null)
        {
            foreach (Vertex v in LastPathSegment)
            {
                if (v == null) continue;
                v.GetComponent<SpriteRenderer>().color = Color.gray;
            }
        }
        
        foreach (Vertex v in PathSegment)
        {
            if (v == null) continue;
            v.GetComponent<SpriteRenderer>().color = Color.blue;
        }

        LastPathSegment = PathSegment;
    }
}
