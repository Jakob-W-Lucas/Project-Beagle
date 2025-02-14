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
            keyValues.Add(v[i].ID, v[i]);
            v[i].p_ID = i; 
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
        return Routes[s.ID][_vertices[i].ID];
    }

    public Route SetDestination(Vertex s, int n) => Routes[s.ID][_vertices[n].ID];

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
            routes.Add(v.ID, BFS(_vertexLookup[v.ID]));
        }

        Routes = routes;
    }

    private Dictionary<Guid, Route> BFS(Vertex s)
    {
        float[] dist = new float[_vertices.Length];
        int[] pred = new int[_vertices.Length];

        for (int i = 0; i < _vertices.Length; i++)
        {
            dist[i] = Mathf.Infinity;
            pred[i] = -1;
        }

        Queue<int> queue = new Queue<int>();
        queue.Enqueue(s.p_ID);
        dist[s.p_ID] = 0;

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

        Dictionary<Guid, Route> routes = new Dictionary<Guid, Route>();
        for (int i = 0; i < _vertices.Length; i++)
        {
            routes.Add(_vertices[i].ID, GetPath(s.p_ID, i, dist[i], pred));
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

        int i = 0;
        int j = 0;
        foreach (Vertex v1 in _vertices)
        {
            str.Append($"The routes of vertex {i}: \n");
            foreach (Vertex v2 in _vertices)
            {
                str.Append($"    Towards vertex {j}: " + GetRouteString(Routes[v1.ID][v2.ID]) + "\n");
                j++;
            }
            i++;
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