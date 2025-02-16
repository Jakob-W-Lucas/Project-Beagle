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
    public Dictionary<Guid, Dictionary<Guid, Route>> Routes { get; private set; } = new Dictionary<Guid, Dictionary<Guid, Route>>();
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
            _stations[i].ConfigureStation(this);
            AddStationToLookup(_stations[i]);

            _stationVertices.Add(_stations[i].Vertex);

            _stations[i].Vertex.SetName(-1, i);

            List<Vertex> s_vertices = new List<Vertex>() { _stations[i].Vertex };
            s_vertices.AddRange(_vertices);

            Routes.Add(_stations[i].Vertex.GuidID, BFS(s_vertices));
        }

        PrintRoutes();

        Vertex[] v = new Vertex[_vertices.Length + _stationVertices.Count];
        _vertices.CopyTo(v, 0);
        _stationVertices.CopyTo(v, _vertices.Length);
    }

    # endregion

    # region Querying

    // Get the routes from a station to each room vertex
    public Route[] RouteFromStation(Station st) => Routes[st.Vertex.GuidID].Values.ToArray();

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

    // Returns a dictionary, where Guid is the ID of the destination and route is the route from the source input
    private Dictionary<Guid, Route> BFS(List<Vertex> v)
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

        Dictionary<Guid, Route> routes = new Dictionary<Guid, Route>();
        for (int i = 0; i < _vertices.Length; i++)
        {
            routes.Add(_vertices[i].GuidID, GetPath(0, i + 1, dist[i], pred, v));
        }

        return routes;
    }

    private Route GetPath(int s, int u, float dist, int[] pred, List<Vertex> v)
    {
        List<Vertex> path = new List<Vertex> { v[u] };
        while (u != s)
        {
            path.Add(v[pred[u]]);
            u = pred[u];
        } 

        path.Reverse();

        return new Route(path, dist);
    }
    
    # endregion

    # region Utility

    public void PrintRoutes()
    {
        StringBuilder str = new StringBuilder();

        int i = 0;
        int j = 0;
        foreach (Vertex v1 in _stationVertices)
        {
            str.Append($"The routes of vertex {i}: \n");
            foreach (Vertex v2 in _vertices)
            {
                str.Append($"    Towards vertex {j}: " + GetRouteString(Routes[v1.GuidID][v2.GuidID]) + "\n");
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
