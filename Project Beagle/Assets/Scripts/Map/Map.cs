using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text;

[System.Serializable]
public class Route : IComparable<Route>
{
    public List<Vertex> Vertices;
    public float Distance;

    public Route(List<Vertex> vertices, float distance)
    {
        Vertices = vertices;
        Distance = distance;
    }

    public Route(Vertex s, Vertex u)
    {
        Vertices = new List<Vertex>() { s, u };
        Distance = Vector2.Distance(s.transform.position, u.transform.position);
    }

    public Route()
    {
        Vertices = new List<Vertex>();
        Distance = Mathf.Infinity;
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

    public int CompareTo(Route other)
    {
        if (other == null) return 1;

        if (Distance == other.Distance) return 0;
        
        return Distance < other.Distance ? 1 : -1;
    }
}

public class Map
{
    public Route[][] Routes;
    private Vertex[] _vertices;
    public bool Configured { get; private set; }

    public Map(Vertex[] v)
    {
        _vertices = v;

        ComputeAllPairsShortestPaths();

        Configured = true;

        // Debugging
        PrintRoutes();
    }

    # region Querying

    public Vertex GetVertexFromIndex(int n) => _vertices[n];

    public Route RandomDestination(Vertex s)
    {
        int i = UnityEngine.Random.Range(0, _vertices.Length);
        return Routes[s.g_ID][UnityEngine.Random.Range(0, _vertices.Length)];
    }

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
        Route[][] routes = new Route[_vertices.Length][];
        
        for (int i = 0; i < _vertices.Length; i++)
        {
            routes[i] = BFS(_vertices[i].g_ID);
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
                if (e.End.g_ID == -1) continue;

                int v = e.End.g_ID;
                
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

    public void PrintRoutes()
    {
        StringBuilder str = new StringBuilder( "Routes: \n" );

        for (int i = 0; i < _vertices.Length; i++)
        {
            str.Append($"The routes of vertex {i}: \n");
            for (int j = 0; j < _vertices.Length; j++)
            {
                str.Append($"    Towards vertex {j}: " + GetRouteString(Routes[i][j]) + "\n");
                j++;
            }
            i++;
        }

        Debug.Log(str.ToString());
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