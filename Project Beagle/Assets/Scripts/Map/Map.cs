using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text;

[System.Serializable]
public class Route
{
    public List<Vertex> Verticies;
    public float TotalDist;

    public Route(List<Vertex> vertices, float totalDist)
    {
        Verticies = vertices;
        TotalDist = totalDist;
    }
}

public class Map
{
    public Route[][] Routes { get; private set; }
    private Dictionary<Guid, Vertex> _vertexLookup = new Dictionary<Guid, Vertex>();
    private Vertex[] _vertices;
    public Map(Vertex[] v)
    {
        _vertices = v;

        Dictionary<Guid, Vertex> keyValues = new Dictionary<Guid, Vertex>();

        for (int i = 0; i < v.Length; i++)
        {
            keyValues.Add(v[i].ID, v[i]);
            v[i].p_ID = i; // Ensure p_ID is set correctly
        }

        _vertexLookup = keyValues;

        ComputeAllPairsShortestPaths();
        Debug.Log(PrintRoutes());
    }

    # region Repeated BFS - All pairs shortest paths 

    // Perform BFS for each node and store distances in the matrix
    public void ComputeAllPairsShortestPaths()
    {
        Route[][] routes = new Route[_vertices.Length][];
        
        for (int start = 0; start < _vertices.Length; start++)
        {
            routes[start] = BFS(start);
        }

        Routes = routes;
    }

    private Route[] BFS(int s)
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
                if (!_vertexLookup.TryGetValue(e.End.ID, out var key)) continue;

                int v = e.End.p_ID;
                
                if (dist[v] == Mathf.Infinity)
                {
                    dist[v] = dist[u] + e.Weight;
                    pred[v] = u;
                    queue.Enqueue(v);
                }
            }
        }

        Route[] routes = new Route[_vertices.Length];
        for (int i = 0; i < _vertices.Length; i++)
        {
            routes[i] = GetPath(s, i, dist[i], pred);
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

    public string PrintRoutes()
    {
        StringBuilder str = new StringBuilder();

        for (int i = 0; i < Routes.Length; i++)
        {
            str.Append($"The routes of vertex {i}: \n");
            for (int j = 0; j < Routes.Length; j++)
            {
                str.Append($"    Towards vertex {j}: " + GetRouteString(Routes[i][j]) + "\n");
            }
        }

        return str.ToString();
    }

    public StringBuilder GetRouteString(Route r)
    {
        StringBuilder str = new StringBuilder();
        str.Append($"Total distance: {r.TotalDist} with path: ");
        for (int i = 0; i < r.Verticies.Count; i++)
        {
            str.Append($"{r.Verticies[i].Name} -> ");
        }
        return str;
    }

    # endregion
}