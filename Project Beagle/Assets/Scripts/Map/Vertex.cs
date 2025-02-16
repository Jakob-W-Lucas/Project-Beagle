using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;

public class Vertex : MonoBehaviour
{
    public Guid GuidID { get; private set; }
    // Global room ID
    public int ID = -1;
    // ID used for room vertices and stations
    public int p_ID;
    public string Name { get; private set; }
    public List<Edge> Edges { get; private set; } = new List<Edge>();
    public Room Room;
    public Station Station;
    [SerializeField] private float _vertexReach = 5f;

    public void SetId()
    {
        GuidID = Guid.NewGuid();
    }

    public void SetName(int n_vertex, int m_station = -1)
    {
        Name = m_station > -1 ? $"R-{Room.name}-S-{m_station}" : $"R-{Room.name}-V-{n_vertex}";
    }

    public void ConfigureVertex()
    {
        SetId();

        Collider2D s_coll = GetComponent<Collider2D>();

        Collider2D[] surrounding_edges = Physics2D.OverlapCircleAll(this.transform.position, _vertexReach);

        foreach (Collider2D c in surrounding_edges)
        {
            if (c == s_coll || c.GetComponent<Station>()) continue;

            if (c.TryGetComponent<Vertex>(out Vertex vertex)) AddEdge(vertex);
        }
    }

    public void AddEdge(Vertex end)
    {
        Edge edge = new Edge(this, end);

        if (!Edges.Contains(edge))
        {
            Edges.Add(edge);
        }
    }
}

[System.Serializable]
public class Edge
{
    [SerializeField] public Guid Id { get; private set; }
    [SerializeField] private bool _enabled;
    [SerializeField] private Vertex _start;
    [SerializeField] private Vertex _end;
    private float _weight;

    public Edge(Vertex start, Vertex end, bool enabled = true)
    {
        _enabled = enabled;

        Id = Guid.NewGuid();
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
        if (obj is Edge other)
        {
            return (_start == other._start && _end == other._end) ||
                   (_start == other._end && _end == other._start);
        }
        return false;
    }

    public override int GetHashCode()
    {
        // Ensure the hash code is the same for both directions of the edge
        int hash1 = _start.GetHashCode() ^ _end.GetHashCode();
        int hash2 = _end.GetHashCode() ^ _start.GetHashCode();
        return hash1 ^ hash2;
    }
}