using System.Collections.Generic;
using UnityEngine;

public class QuadTree
{
    private class Node
    {
        public Rect Bounds;
        public List<Edge> Edges = new List<Edge>();
        public Node[] Children;

        public Node(Rect bounds)
        {
            Bounds = bounds;
            Children = null;
        }
    }

    private Node root;
    private Vertex[] Vertices;
    private int maxEdgesPerNode = 8;

    public QuadTree(Rect bounds, Vertex[] vertices, int maxEdges = 8)
    {
        root = new Node(bounds);
        Vertices = vertices;
        maxEdgesPerNode = maxEdges;
    }

    public void Insert(Edge e)
    {
        Insert(root, e);
    }

    private void Insert(Node node, Edge edge)
    {
        if (!node.Bounds.Overlaps(GetEdgeBounds(edge))) return;

        if (node.Edges.Count < maxEdgesPerNode || node.Children == null)
        {
            node.Edges.Add(edge);
        }
        else
        {
            if (node.Children == null)
                Subdivide(node);
            
            foreach (var child in node.Children)
                Insert(child, edge);
        }
    }

    private void Subdivide(Node node)
    {
        float halfWidth = node.Bounds.width / 2f;
        float halfHeight = node.Bounds.height / 2f;
        Vector2 center = node.Bounds.center;

        node.Children = new Node[4]
        {
            new Node(new Rect(node.Bounds.xMin, node.Bounds.yMin, halfWidth, halfHeight)),
            new Node(new Rect(center.x, node.Bounds.yMin, halfWidth, halfHeight)),
            new Node(new Rect(node.Bounds.xMin, center.y, halfWidth, halfHeight)),
            new Node(new Rect(center.x, center.y, halfWidth, halfHeight))
        };

        foreach (var edge in node.Edges)
            foreach (var child in node.Children)
                Insert(child, edge);

        node.Edges.Clear();
    }

    public List<Edge> Query(Rect bounds)
    {
        List<Edge> result = new List<Edge>();
        Query(root, bounds, result);
        return result;
    }

    private void Query(Node node, Rect bounds, List<Edge> result)
    {
        if (!node.Bounds.Overlaps(bounds)) return;

        result.AddRange(node.Edges);

        if (node.Children != null)
            foreach (var child in node.Children)
                if (child.Bounds.Overlaps(bounds)) // Only check relevant children
                    Query(child, bounds, result);
    }

    private Rect GetEdgeBounds(Edge edge)
    {
        Vector2 min = Vector2.Min(edge.StartPos, edge.EndPos);
        Vector2 max = Vector2.Max(edge.StartPos, edge.EndPos);
        return new Rect(min, max - min);
    }

    public Edge IsPointOnEdge(Vector2 point, float tolerance = 0.02f) => GetEdgeAtPoint(point, tolerance);

    public Edge IsPointOnEdge(Agent agent, float tolerance = 0.02f)
    {
        Edge edge = GetEdgeAtPoint(agent.transform.position, tolerance);
        
        if (edge != null && edge.Start.g_ID == edge.End.g_ID)
            edge = agent.Navigation.CurrentEdge;

        return edge;
    }

    public Edge GetEdgeAtPoint(Vector2 point, float tolerance = 0.02f)
    {
        Rect searchArea = new Rect(point.x - tolerance, point.y - tolerance, tolerance * 2, tolerance * 2);
        var candidates = Query(searchArea);

        int[] vertexEdgeArray = new int[Vertices.Length];
        Edge detectedEdge = null;

        foreach (var edge in candidates)
        {
            if (PointOnSegment(point, edge.StartPos, edge.EndPos, tolerance))
            {
                vertexEdgeArray[edge.Start.g_ID] += 1;
                vertexEdgeArray[edge.End.g_ID] += 1;
                
                detectedEdge = edge;
            }
        }

        for (int i = 0; i < Vertices.Length; i++)
        {
            if (vertexEdgeArray[i] > 2 && Vector2.Distance(point, Vertices[i].Position) <= tolerance)
            {
                return new Edge(Vertices[i], Vertices[i]); // Return the vertex if multiple edges are detected near it
            }
        }

        return detectedEdge; // Return single edge if only one was found, otherwise return a vertex
    }

    private bool PointOnSegment(Vector2 p, Vector2 a, Vector2 b, float tolerance)
    {
        Vector2 ab = b - a;
        Vector2 ap = p - a;

        float magnitudeAB = ab.sqrMagnitude;
        float projection = Mathf.Clamp(Vector2.Dot(ap, ab) / magnitudeAB, 0f, 1f);

        Vector2 closest = a + projection * ab;
        float distance = Vector2.Distance(p, closest);

        return distance <= tolerance;
    }

    public void DebugDraw(Color color)
    {
        DebugDrawNode(root, color);
    }

    private void DebugDrawNode(Node node, Color color)
    {
        if (node == null) return;

        Debug.DrawLine(new Vector3(node.Bounds.xMin, node.Bounds.yMin), new Vector3(node.Bounds.xMax, node.Bounds.yMin), color);
        Debug.DrawLine(new Vector3(node.Bounds.xMax, node.Bounds.yMin), new Vector3(node.Bounds.xMax, node.Bounds.yMax), color);
        Debug.DrawLine(new Vector3(node.Bounds.xMax, node.Bounds.yMax), new Vector3(node.Bounds.xMin, node.Bounds.yMax), color);
        Debug.DrawLine(new Vector3(node.Bounds.xMin, node.Bounds.yMax), new Vector3(node.Bounds.xMin, node.Bounds.yMin), color);

        if (node.Children != null)
        {
            foreach (var child in node.Children)
            {
                DebugDrawNode(child, color);
            }
        }
    }
}
