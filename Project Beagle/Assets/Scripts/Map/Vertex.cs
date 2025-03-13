using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;

public class Vertex : MonoBehaviour
{
    public Vector2 Position;
    // Global room ID
    public int g_ID = -1;
    // ID used for room vertices and stations
    public int r_ID = -1;
    // Pseudo ID used for in room pathfinding
    public int p_ID = -1;
    public List<Edge> Edges { get; private set; } = new List<Edge>();
    public Room Room;
    public Station Station;
    // Distance a vertex will look for edges
    [SerializeField] private float _vertexReach = 5f;
    public string Name => Station ? $"R-{Room.name}-S-{r_ID}" : $"R-{Room.name}-V-{r_ID}";

    public bool IsRoom => g_ID != -1;
    public bool IsStation => g_ID == -1 && r_ID != -1;
    public bool IsPointer => g_ID == -1 && r_ID == -1;

    // Get all the surrounding vertices and add them to the current edges
    public void ConfigureVertex(Room room, int r_ID, int p_ID, Station station = null)
    {
        this.Room = room;
        this.r_ID = r_ID;
        this.p_ID = p_ID;
        this.Station = station;
        this.Position = transform.position;

        GatherSurroundingVertices();
    }

    public void ConfigureVertex(Room room, Vector2 position, Station station = null, bool getSurrounding = false)
    {
        this.Room = room;
        this.r_ID = -1;
        this.p_ID = -1;
        this.Station = station;
        this.Position = position;

        if (getSurrounding) GatherSurroundingVertices();
    }

    private void GatherSurroundingVertices()
    {
        Collider2D s_coll = GetComponent<Collider2D>();

        Collider2D[] surrounding_edges = Physics2D.OverlapCircleAll(this.transform.position, _vertexReach);

        foreach (Collider2D c in surrounding_edges)
        {
            if (c == s_coll || c.GetComponentInParent<Station>() || c.GetComponentInParent<Agent>()) continue;

            if (c.TryGetComponent<Vertex>(out Vertex vertex)) 
            {
                AddEdge(vertex);
            }
        }
    }

    // Creates only the forward going edge to a destination
    public void AddEdge(Vertex end)
    {
        Edge edge = new Edge(this, end);

        if (!Edges.Contains(edge))
        {
            Edges.Add(edge);
        }
    }

    public override bool Equals(object obj)
    {
        return obj is Vertex other && 
            this.Position == other.Position;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(g_ID, r_ID);
    }
}

[System.Serializable]
public class Edge
{
    [SerializeField] private bool _enabled;
    [SerializeField] private Vertex _start;
    [SerializeField] private Vertex _end;
    public Vertex[] Vertices => new Vertex[2] { _start, _end };
    private float _weight;
    public Vector2 StartPos => _start.Position;
    public Vector2 EndPos => _end.Position;
    public Vector2 EdgeVector => _end.Position - _start.Position;

    public Edge(Vertex start, Vertex end, bool enabled = true)
    {
        _enabled = enabled;

        _start = start;
        _end = end;

        _weight = Vector2.Distance(_start.transform.position, _end.transform.position);
    }

    public bool Enabled => _enabled;
    public Vertex Start => _start;
    public Vertex End => _end;
    public float Weight => _weight;

    public override bool Equals(object obj)
    {
        if (obj is not Edge other) return false;
        //if (ReferenceEquals(this, other)) return true; // Quick check
        return (_start.Equals(other._start) && _end.Equals(other._end)) ||
            (_start.Equals(other._end) && _end.Equals(other._start));
    }
    
    public override int GetHashCode()
    {
        int h1 = _start.GetHashCode();
        int h2 = _end.GetHashCode();
        // Order-independent hash combination
        return h1 < h2 ? 
            HashCode.Combine(h1, h2) : 
            HashCode.Combine(h2, h1);
    }
}