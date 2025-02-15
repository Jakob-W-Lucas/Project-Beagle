using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;
using System.Linq;

[RequireComponent(typeof(Collider2D))]
public abstract class Room : MonoBehaviour
{
    // Verticies to enter and exit any room
    private Map _map;
    [SerializeField] private Vertex[] _vertices;
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
        }

        Vertex[] v = new Vertex[_vertices.Length + _stationVertices.Count];
        _vertices.CopyTo(v, 0);
        _stationVertices.CopyTo(v, _vertices.Length);

        _map = new Map(v);
    }

    # endregion

    # region Querying

    // Get the route from the source to the room vertex with the lowest total distance
    public Route GetRouteToRoom(Vertex s)
    {
        Route contender = null;
        float dist = Mathf.Infinity;

        foreach (Vertex v in _vertices)
        {
            Route p_route = _map.Routes[s.ID][v.ID];

            if (p_route.Distance > dist) continue;

            contender = p_route;
            dist = p_route.Distance;
        }

        return contender;
    }

    // Get the routes from a station to each room vertex
    public Route[] RouteFromStation(Station st) => _map.Routes[st.Vertex.ID].Values.ToArray();

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
