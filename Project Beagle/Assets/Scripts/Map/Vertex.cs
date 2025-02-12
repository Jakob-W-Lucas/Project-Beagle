using UnityEngine;
using System;
using System.Collections.Generic;

public class Vertex : MonoBehaviour
{
    [SerializeField] public Guid Id { get; private set; }
    public List<Edge> Edges { get; private set; }

    void Start()
    {
        Id = Guid.NewGuid();
        Edges = new List<Edge>();
    }

    public void AddEdge(Vertex end)
    {
        Edge edge = new Edge(this, end);
        Edge reverseEdge = new Edge(end, this);

        if (!Edges.Contains(edge))
        {
            Edges.Add(edge);
        }

        if (!end.Edges.Contains(reverseEdge))
        {
            end.Edges.Add(reverseEdge);
        }
    }
}

[System.Serializable]
public class Edge
{
    [SerializeField] public Guid Id { get; private set; }

    [SerializeField] private Vertex _start;
    [SerializeField] private Vertex _end;
    private float _weight;

    public Edge(Vertex start, Vertex end)
    {
        Id = Guid.NewGuid();
        _start = start;
        _end = end;

        _weight = Vector2.Distance(_start.transform.position, _end.transform.position);
    }

    public Vertex Start => _start;
    public Vertex End => _end;

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