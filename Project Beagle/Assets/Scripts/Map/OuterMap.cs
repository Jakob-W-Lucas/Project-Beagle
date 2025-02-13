using System.Collections.Generic;
using UnityEngine;
using System;

public class OuterMap : MonoBehaviour
{
    private Map _map;
    private Room[] _rooms;
    private Dictionary<Type, List<Room>> _lookupRooms = new Dictionary<Type, List<Room>>();
    private List<Vertex> _roomVerticies = new List<Vertex>();

    private void OnEnable() {
        
        _rooms = GetComponentsInChildren<Room>();

        if (_rooms.Length == 0) return;

        foreach (Room room in _rooms)
        {   
            room.ConfigureRoom();
            AddRoomToLookup(room);
            
            _roomVerticies.AddRange(room.Verticies);
        }

        for (int i = 0; i < _roomVerticies.Count; i++)
        {
            _roomVerticies[i].ID = i;
        }

        _map = new Map(_roomVerticies);
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

        foreach (Vertex vertex in _roomVerticies)
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
