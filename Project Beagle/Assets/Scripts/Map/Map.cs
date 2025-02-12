using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Map: MonoBehaviour
{
    
    private Dictionary<Type, List<Room>> _lookupRooms = new Dictionary<Type, List<Room>>();
    private Room[] _rooms;
    private List<Vertex> _verticies = new List<Vertex>();

    private void OnEnable() {
        
        _rooms = GetComponentsInChildren<Room>();

        if (_rooms.Length == 0) return;

        foreach (Room room in _rooms)
        {   
            room.ConfigureRoom();
            _verticies.AddRange(room.Verticies);
            AddRoomToLookup(room);
        }
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
}
