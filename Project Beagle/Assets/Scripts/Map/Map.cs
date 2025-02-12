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

public class Map: MonoBehaviour
{
    public Route[][] Routes { get; private set; }
    private Dictionary<Type, List<Room>> _lookupRooms = new Dictionary<Type, List<Room>>();
    private Room[] _rooms;
    private List<Vertex> _verticies = new List<Vertex>();

    private void OnEnable() {
        
        _rooms = GetComponentsInChildren<Room>();

        if (_rooms.Length == 0) return;

        foreach (Room room in _rooms)
        {   
            room.ConfigureRoom();
            AddRoomToLookup(room);
            
            _verticies.AddRange(room.Verticies);
        }

        for (int i = 0; i < _verticies.Count; i++)
        {
            _verticies[i].ID = i;
        }

        Routes = ComputeAllPairsShortestPaths();
        
        Debug.Log(PrintRoutes());
    }

    private void AddRoomToLookup(Room room)
    {
        Type roomType = room.GetType();

        if (!_lookupRooms.ContainsKey(roomType))
        {
            _lookupRooms[roomType] = new List<Room>();
        }

        _lookupRooms[roomType].Add(room);
    }

    public List<Room> GetRoomsOfType<T>() where T : Room
    {
        Type roomType = typeof(T);

        if (_lookupRooms.ContainsKey(roomType))
        {
            return _lookupRooms[roomType];
        }

        return new List<Room>();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        foreach (Vertex vertex in _verticies)
        {
            foreach (Edge edge in vertex.Edges)
            {
                if (edge.Enabled)
                {
                    Gizmos.DrawLine(vertex.transform.position, edge.End.transform.position);
                }
            }
        }
    }

    # region Repeated BFS - All pairs shortest paths 

    // Perform BFS for each node and store distances in the matrix
    public Route[][] ComputeAllPairsShortestPaths()
    {
        Route[][] routes = new Route[_verticies.Count][];
        
        for (int start = 0; start < _verticies.Count; start++)
        {
            routes[start] = BFS(start);
        }

        return routes;
    }

    private Route[] BFS(int s)
    {
        float[] dist = new float[_verticies.Count];
        int[] pred = new int[_verticies.Count];

        for (int i = 0; i < _verticies.Count; i++)
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
            foreach (Edge e in _verticies[u].Edges)
            {
                int v = e.End.ID;

                if (dist[v] == Mathf.Infinity)
                {
                    dist[v] = dist[u] + e.Weight;
                    pred[v] = u;
                    queue.Enqueue(v);
                }
            }
        }

        Route[] routes = new Route[_verticies.Count];
        for (int i = 0; i < _verticies.Count; i++)
        {
            routes[i] = GetPath(s, i, dist[i], pred);
        }

        return routes;
    }

    private Route GetPath(int s, int u, float dist, int[] pred)
    {
        List<Vertex> path = new List<Vertex> { _verticies[u] };
        while (u != s)
        {
            path.Add(_verticies[pred[u]]);
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
            str.Append($"{r.Verticies[i].ID} -> ");
        }
        return str;
    }

    # endregion
}