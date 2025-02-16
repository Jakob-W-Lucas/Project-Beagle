using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text;

[System.Serializable]
public class Route
{
    public List<Vertex> Vertices;
    public float Distance;

    public Route(List<Vertex> vertices, float distance)
    {
        Vertices = vertices;
        Distance = distance;
    }

    public Route Join(Route other)
    {
        if (other == null || other.Vertices.Count == 0)
        {
            return new Route(new List<Vertex>(this.Vertices), this.Distance);
        }

        List<Vertex> joinedVertices = new List<Vertex>(this.Vertices);
        if (joinedVertices.Last() == other.Vertices.First())
        {
            joinedVertices.AddRange(other.Vertices.Skip(1));
        }
        else
        {
            joinedVertices.AddRange(other.Vertices);
        }

        float joinedDistance = this.Distance + other.Distance;
        return new Route(joinedVertices, joinedDistance);
    }
}

public class Map
{
    public Dictionary<Guid, Dictionary<Guid, Route>> Routes { get; private set; } = new Dictionary<Guid, Dictionary<Guid, Route>>();
    private Dictionary<Guid, Vertex> _vertexLookup = new Dictionary<Guid, Vertex>();
    private Vertex[] _vertices;
    public bool Configured { get; private set; }

    public Map(Vertex[] v)
    {
        _vertices = v;

        Dictionary<Guid, Vertex> keyValues = new Dictionary<Guid, Vertex>();

        for (int i = 0; i < v.Length; i++)
        {
            keyValues.Add(v[i].GuidID, v[i]);
        }

        _vertexLookup = keyValues;

        ComputeAllPairsShortestPaths();

        Configured = true;

        // Debugging
        Debug.Log(PrintRoutes());
    }

    # region Querying

    public Vertex GetVertexFromIndex(int n) => _vertices[n];

    public Route RandomDestination(Vertex s)
    {
        int i = UnityEngine.Random.Range(0, _vertices.Length);
        return Routes[s.GuidID][_vertices[i].GuidID];
    }

    public Route SetDestination(Vertex s, int n) => Routes[s.GuidID][_vertices[n].GuidID];

    public Vertex GetNearestVertex(Vector2 pos)
    {
        Vertex contender = null;
        float dist = Mathf.Infinity;

        foreach (Vertex v in _vertices)
        {
            float d_dist = Vector2.Distance(v.transform.position, pos);
            if (d_dist > dist) {
                continue;
            }

            contender = v;
            dist = d_dist;
        }

        return contender;
    }

    # endregion

    # region Repeated BFS - All pairs shortest paths 

    // Perform BFS for each node and store distances in the matrix
    public void ComputeAllPairsShortestPaths()
    {
        Dictionary<Guid, Dictionary<Guid, Route>> routes = new Dictionary<Guid, Dictionary<Guid, Route>>();
        
        foreach (Vertex v in _vertices)
        {
            routes.Add(v.GuidID, BFS(v.ID));
        }

        Routes = routes;
    }

    private Dictionary<Guid, Route> BFS(int s)
    {
        float[] dist = new float[_vertices.Length];
        int[] pred = new int[_vertices.Length];

        for (int i = 0; i < _vertices.Length; i++)
        {
            dist[i] = Mathf.Infinity;
            pred[i] = -1;
        }

        Queue<int> queue = new Queue<int>();
        queue.Enqueue(s);
        dist[s] = 0;

        while (queue.Count > 0)
        {
            int u = queue.Dequeue();
            foreach (Edge e in _vertices[u].Edges)
            {
                if (!HasVertex(e.End)) continue;

                int v = e.End.ID;
                
                if (dist[v] == Mathf.Infinity)
                {
                    dist[v] = dist[u] + e.Weight;
                    pred[v] = u;
                    queue.Enqueue(v);
                }
            }
        }

        Dictionary<Guid, Route> routes = new Dictionary<Guid, Route>();
        for (int i = 0; i < _vertices.Length; i++)
        {
            routes.Add(_vertices[i].GuidID, GetPath(s, i, dist[i], pred));
        }

        return routes;
    }

    private Route GetPath(int s, int u, float dist, int[] pred)
    {
        List<Vertex> path = new List<Vertex> { _vertices[u] };
        while (u != s)
        {
            path.Add(_vertices[pred[u]]);
            u = pred[u];
        }   

        path.Reverse();

        return new Route(path, dist);
    }

    # endregion

    # region Utility

    public bool HasVertex(Vertex n) => _vertexLookup.ContainsKey(n.GuidID);

    public string PrintRoutes()
    {
        StringBuilder str = new StringBuilder();

        int i = 0;
        int j = 0;
        foreach (Vertex v1 in _vertices)
        {
            str.Append($"The routes of vertex {i}: \n");
            foreach (Vertex v2 in _vertices)
            {
                str.Append($"    Towards vertex {j}: " + GetRouteString(Routes[v1.GuidID][v2.GuidID]) + "\n");
                j++;
            }
            i++;
        }

        return str.ToString();
    }

    public StringBuilder GetRouteString(Route r)
    {
        StringBuilder str = new StringBuilder();
        str.Append($"Total distance: {r.Distance} with path: ");
        for (int i = 0; i < r.Vertices.Count; i++)
        {
            str.Append($"{r.Vertices[i].Name} -> ");
        }
        return str;
    }

    # endregion
}