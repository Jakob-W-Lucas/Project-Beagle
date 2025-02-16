using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;
using System.Linq;
using System.Text;

[RequireComponent(typeof(Collider2D))]
public abstract class Room : MonoBehaviour
{
    // Routes from any station to any room vertex
    public Route[][] RoomExitRoutes;
    public Route[][] RoomEnterRoutes;
    // Verticies to enter and exit any room
    [SerializeField] private Vertex[] _vertices;
    // Room stations
    [SerializeField] private Station[] _stations;
    private List<Vertex> _stationVertices = new List<Vertex>();
    private Dictionary<Type, List<Station>> _lookupStations = new Dictionary<Type, List<Station>>();
    private Collider2D _bounds;
    public abstract void DebugRoom();

    # region Initialization

    public Vertex[] Vertices => _vertices;
    public Station[] Stations => _stations;

    private void OnEnable() 
    {
        _bounds = GetComponent<Collider2D>();
    }

    private void AddStationToLookup(Station station)
    {
        Type stationType = station.GetType();

        if (!_lookupStations.ContainsKey(stationType))
        {
            _lookupStations[stationType] = new List<Station>();
        }

        _lookupStations[stationType].Add(station);
    }

    public void ConfigureRoom()
    {
        for (int i = 0; i < _vertices.Length; i++)
        {
            _vertices[i].Room = this;
            _vertices[i].ConfigureVertex();

            _vertices[i].SetName(i);

            _vertices[i].p_ID = i + 1;
        }

        ConfigureStations();
    }

    public void ConfigureStations()
    {
        if (_stations.Length == 0) return;

        for (int i = 0; i < _stations.Length; i++)
        {
            _stations[i].ConfigureStation(this, i);
            AddStationToLookup(_stations[i]);

            _stationVertices.Add(_stations[i].Vertex);

            _stations[i].Vertex.SetName(-1, i);
        }

        SetStationRoutes();

        PrintRoutes("Entering routes: \n", RoomEnterRoutes);
        PrintRoutes("Exiting routes: \n", RoomExitRoutes);
    }

    # endregion

    # region Querying

    private List<Station> HasStation<T>() where T : Station {
        if (_lookupStations.TryGetValue(typeof(T), out var stations))
        {
            return stations;
        }

        return null;
    }

    # endregion

    # region Debugging

    void OnDrawGizmos()
    {
        if (_stations == null) return;

        Gizmos.color = Color.blue;

        foreach (Station s in _stations)
        {
            if (s.Vertex == null) continue;
            
            foreach (Edge edge in s.Vertex.Edges)
            {
                if (edge.Enabled)
                {
                    Gizmos.DrawLine(s.Vertex.transform.position, edge.End.transform.position);
                }
            }
        }
    }

    # endregion

    # region BFS

    private void SetStationRoutes()
    {
        for (int i = 0; i < _stations.Length; i++)
        {
            List<Vertex> s_vertices = new List<Vertex>() { _stations[i].Vertex };
            s_vertices.AddRange(_vertices);

            (Route[] enter, Route[] exit) = BFS(s_vertices);

            RoomExitRoutes[i] = enter;
            RoomEnterRoutes[i] = exit; 
        }
    }

    // Returns a dictionary, where Guid is the ID of the destination and route is the route from the source input
    private (Route[], Route[]) BFS(List<Vertex> v)
    {
        float[] dist = new float[v.Count];
        int[] pred = new int[v.Count];

        for (int i = 0; i < v.Count; i++)
        {
            dist[i] = Mathf.Infinity;
            pred[i] = -1;
        }

        Queue<int> queue = new Queue<int>();
        queue.Enqueue(0);
        dist[0] = 0;

        while (queue.Count > 0)
        {
            int u = queue.Dequeue();
            foreach (Edge e in v[u].Edges)
            {
                if (!_vertices.Contains(e.End)) continue;

                int n = e.End.p_ID;
                
                if (dist[n] == Mathf.Infinity)
                {
                    dist[n] = dist[u] + e.Weight;
                    pred[n] = u;
                    queue.Enqueue(n);
                }
            }
        }

        Route[] enter = new Route[v.Count];
        Route[] exit = new Route[v.Count];
        for (int i = 0; i < _vertices.Length; i++)
        {
            (Route en, Route ex) = GetPath(0, i + 1, dist[i], pred, v);
            enter[i] = en;
            exit[i] = ex;
        }

        return (enter, exit);
    }

    private (Route, Route) GetPath(int s, int u, float dist, int[] pred, List<Vertex> v)
    {
        List<Vertex> path = new List<Vertex> { v[u] };
        while (u != s)
        {
            path.Add(v[pred[u]]);
            u = pred[u];
        } 

        List<Vertex> exitPath = new List<Vertex>( path );

        path.Reverse();

        return (new Route(path, dist), new Route(exitPath, dist));
    }
    
    # endregion

    # region Utility

    public void PrintRoutes(string prolog, Route[][] routes)
    {
        StringBuilder str = new StringBuilder( prolog );

        for (int i = 0; i < _stations.Length; i++)
        {
            str.Append($"The routes of vertex {i}: \n");
            for (int j = 0; j < _vertices.Length; j++)
            {
                str.Append($"    Towards vertex {j}: " + GetRouteString(routes[i][j]) + "\n");
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
