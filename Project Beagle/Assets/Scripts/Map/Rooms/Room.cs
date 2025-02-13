using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(Collider2D))]
public abstract class Room : MonoBehaviour
{
    // Verticies to enter and exit any room
    private Map _map;
    public Vertex[] Verticies { get; private set; }
    private Station[] _stations;
    private Dictionary<Type, List<Station>> _lookupStations = new Dictionary<Type, List<Station>>();
    private List<Vertex> _stationVerticies = new List<Vertex>();
    private Collider2D _bounds;
    public abstract void DebugRoom();

    private void OnEnable() 
    {
        _bounds = GetComponent<Collider2D>();

        _stations = GetComponentsInChildren<Station>();

        if (_stations.Length == 0) return;

        foreach (Station station in _stations)
        {
            station.ConfigureStation(this);
            AddStationToLookup(station);

            _stationVerticies.Add(station.Vertex);
        }

        for (int i = 0; i < _stationVerticies.Count; i++)
        {
            _stationVerticies[i].ID = i;
        }

        _map = new Map(_stationVerticies);
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
        Verticies = GetComponentsInChildren<Vertex>();

        foreach (Vertex v in Verticies)
        {
            v.Room = this;
            v.ConfigureVertex();
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        foreach (Vertex vertex in _stationVerticies)
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
}
