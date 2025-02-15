using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;

public class OuterMap : MonoBehaviour
{
    public Map Map { get; private set; }
    private Room[] _rooms;
    private Dictionary<Type, List<Room>> _lookupRooms = new Dictionary<Type, List<Room>>();
    private List<Vertex> _roomVertices = new List<Vertex>();

    # region Initialization

    private void OnEnable() {
        
        _rooms = GetComponentsInChildren<Room>();

        if (_rooms.Length == 0) return;

        foreach (Room room in _rooms)
        {   
            room.ConfigureRoom();
            AddRoomToLookup(room);
            
            _roomVertices.AddRange(room.Vertices);
        }

        Map = new Map(_roomVertices.ToArray());
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

    # endregion

    # region Utility

    // Get the route from the source to the room of type T with lowest total distance
    public Route GetRouteToRoom<T>(Vertex s) where T : Room
    {
        List<Room> rooms = GetRoomsOfType<T>();

        Route contender = null;
        float dist = Mathf.Infinity;

        foreach (Room r in rooms)
        {
            Route p_route = r.GetRouteToRoom(s);

            if (p_route.TotalDist > dist) continue;

            contender = p_route;
            dist = p_route.TotalDist;
        }

        return contender;
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

    # endregion

    # region Utility

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        foreach (Vertex vertex in _roomVertices)
        {
            foreach (Edge edge in vertex.Edges)
            {
                if (edge.Enabled && !edge.End.GetComponent<Station>())
                {
                    Gizmos.DrawLine(vertex.transform.position, edge.End.transform.position);
                    Debug.DrawLine(vertex.transform.position, edge.End.transform.position, Color.red);
                }
            }
        }
    }

    # endregion
}
