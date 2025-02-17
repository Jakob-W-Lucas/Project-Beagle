using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;
using System.Linq;
using System.Text;
using UnityEngine.Rendering.VirtualTexturing;

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

    // Sets up rooms for pathfinding
    public void ConfigureRoom()
    {
        RoomEnterRoutes = new Route[_vertices.Length][];

        for (int i = 0; i < _vertices.Length; i++)
        {
            _vertices[i].Room = this;
            _vertices[i].ConfigureVertex();

            // r_ID ID of the vertex relative to the room
            _vertices[i].r_ID = i + _stations.Length;
            // p_ID pseudo ID used for room pathfinding
            _vertices[i].p_ID = i + 1;

            RoomEnterRoutes[i] = new Route[_stations.Length]; 
        }

        ConfigureStations();
    }

    // Sets up stations for pathfinding
    public void ConfigureStations()
    {
        RoomExitRoutes = new Route[_stations.Length][];

        if (_stations.Length == 0) return;

        for (int i = 0; i < _stations.Length; i++)
        {
            _stationVertices.Add(_stations[i].Vertex);
            _stations[i].ConfigureStation(this);

            AddStationToLookup(_stations[i]);

            _stations[i].Vertex.r_ID = i;
            _stations[i].Vertex.p_ID = 0;

            RoomExitRoutes[i] = new Route[_vertices.Length];
        }

        SetStationRoutes();
    }

    # endregion

    # region BFS

    /*

    Bredth-First Search pathfinding method:

    Each station needs to know the paths to get to every room vertex, and each room vertex needs 
    To know how to get to every station. 

    RoomEnterRoutes[][]: [RoomVertex1] -> [StationVertex1, ... , StationVertexN]
                         [     ...   ] -> [ ... ]
                         [RoomVertexN] -> [ ... ]
    
    RoomEnterRoutes[][]: [StationVertex1] -> [RoomVertex1, ... , RoomVertexN]
                         [     ...      ] -> [ ... ]
                         [StationVertexN] -> [ ... ]

    Enables pathfinding to know how to get out of a room from a station and how to get into a room 
    from a station.

    Complexity:
        (One BFS): O(2(Vertices.Length + 1)) => O(Vertices.Length)
    
        (All routes): O(Vertices.Length * Stations.Length)

    */

    private void SetStationRoutes()
    {
        for (int i = 0; i < _stations.Length; i++)
        {
            List<Vertex> s_vertices = new List<Vertex>() { _stations[i].Vertex };
            s_vertices.AddRange( _vertices );

            BFS(s_vertices, i);
        }
    }

    // Bredth-First Search, input: All room vertices + 1 destination station, Index for route array
    private void BFS(List<Vertex> v, int index)
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
                if (!v.Contains(e.End)) continue;

                int n = e.End.p_ID;
                
                if (dist[n] == Mathf.Infinity)
                {
                    dist[n] = dist[u] + e.Weight;
                    pred[n] = u;
                    queue.Enqueue(n);
                }
            }
        }

        for (int i = 0; i < _vertices.Length; i++)
        {
            (Route en, Route ex) = GetPath(0, i + 1, dist[i + 1], pred, v);

            RoomEnterRoutes[i][index] = en;
            RoomExitRoutes[index][i] = ex;
        }
    }

    // Get enter and exit pathways
    private (Route, Route) GetPath(int s, int u, float dist, int[] pred, List<Vertex> v)
    {
        List<Vertex> path = new List<Vertex> { v[u] };
        while (u != s)
        {
            path.Add(v[pred[u]]);
            u = pred[u];
        } 

        List<Vertex> enterPath = new List<Vertex>( path );

        // Exit path is reverse of enter path
        path.Reverse();

        return (new Route(enterPath, dist), new Route(path, dist));
    }
    
    # endregion

    # region Utility

    public void PrintAllRoutes()
    {
        PrintRoutes($"Room: {this.gameObject.name} Entering routes: \n", RoomEnterRoutes);
        PrintRoutes($"Room: {this.gameObject.name} Exiting routes: \n", RoomExitRoutes);
    }

    public void PrintRoutes(string prolog, Route[][] routes)
    {
        StringBuilder str = new StringBuilder( prolog );

        for (int i = 0; i < routes.Length; i++)
        {
            str.Append($"The routes of vertex {i}: \n");
            for (int j = 0; j < routes[0].Length; j++)
            {
                str.Append($"    Towards vertex {j}: " + GetRouteString(routes[i][j]) + "\n");
            }
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
}
