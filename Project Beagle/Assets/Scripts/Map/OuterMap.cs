using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;
using System.Linq;

public class OuterMap : MonoBehaviour
{
    public Map Map { get; private set; }
    [SerializeField] private Room[] _rooms;
    private Dictionary<RoomType, List<Room>> _lookupRooms = new Dictionary<RoomType, List<Room>>();
    private Dictionary<Type, List<Station>> _lookupStations = new Dictionary<Type, List<Station>>();
    private List<Vertex> _roomVertices = new List<Vertex>();

    # region Initialization

    private void OnEnable() {
        
        if (_rooms.Length == 0) return;

        int n = 0;
        foreach (Room room in _rooms)
        {   
            if (!room.isActiveAndEnabled) continue;
            
            room.ConfigureRoom();
            AddToLookup(room);
            
            foreach (Vertex v in room.Vertices)
            {
                v.g_ID = n;
                Debug.Log($"add vertex {v.g_ID}");
                n++;

                _roomVertices.Add(v);
            }
        }

        Map = new Map(_roomVertices.ToArray());
    }

    private void AddToLookup(Room r)
    {
        if (!_lookupRooms.ContainsKey(r.Type))
        {
            _lookupRooms[r.Type] = new List<Room>();
        }

        _lookupRooms[r.Type].Add(r);
        Debug.Log($"Added: {r.Type} to room");

        foreach (Station s in r.Stations)
        {
            AddStationToLookup(s);
        }
    }

    private void AddStationToLookup(Station s)
    {
        Type t = s.GetType();

        if (!_lookupStations.ContainsKey(t))
        {
            _lookupStations[t] = new List<Station>();
        }

        _lookupStations[t].Add(s);
    }

    # endregion

    # region Querying

    // Returns the path from any vertex to any station
    public Route TravelToStation(Vertex s, Type T, Station u_st = null)
    {
        if (u_st && s.Room == u_st.Room) return null;

        bool s_station = s.g_ID == -1;

        List<Station> stations = u_st ? new List<Station>() { u_st } : _lookupStations[T];

        Route contender = new Route();

        if (stations.Contains(s.Station)) return null;

        foreach (Station st in stations)
        {
            // Ensure that the station to travel to is avaliable before getting a route
            if (!st.Avaliable) continue;
            
            if (s_station) {
                contender = CompareRoutes(contender, StationToStation(s, st.Vertex)); 
                continue;
            }
            
            contender = CompareRoutes(contender, RoomToStation(s, st.Vertex));
        }

        return contender;
    }

    // Returns the path from any vertex to any room
    public Route TravelToRoom(Vertex s, RoomType T, Room u_room = null)
    {
        if ((u_room && s.Room == u_room) || s == null) return null;

        bool s_station = s.g_ID == -1;

        List<Room> rooms = u_room ? new List<Room>() { u_room } : _lookupRooms[T];

        Route contender = new Route();

        foreach (Room r in rooms)
        {
            if (s_station) {
                contender = CompareRoutes(contender, StationToRoom(s, r)); 
                continue;
            }
            
            contender = CompareRoutes(contender, RoomToRoom(s, r));
        }

        return contender;
    }

    // Get the route between a room vertex to another room vertex
    private Route RoomToRoom(Vertex s, Room room)
    {
        if (s.Room == room) return new Route(s, s);

        Route contender = new Route();

        // For every vertex in the destination room, check source -> room Vertex
        foreach (Vertex r_V2 in room.Vertices)
        {
            Route roomRoute = Map.Routes[s.g_ID][r_V2.g_ID];

            float totalDist = roomRoute.Distance;

            contender = CompareRoutes(contender, roomRoute);
        }

        return contender;
    }

    // Get the route between a room vertex and a station vertex
    private Route RoomToStation(Vertex s, Vertex u)
    {
        Route roomRoute = null;
        Route enterRoute = null;

        float dist = Mathf.Infinity;

        // Compare the route from the destination station to the source room
        foreach (Route r_S2 in u.Room.RoomExitRoutes[u.r_ID])
        {
            // Node to enter the destination room
            Vertex entrance = r_S2.Vertices.Last();

            // Route from source -> entrance
            Route p_roomRoute = Map.Routes[s.g_ID][entrance.g_ID];
            // Route from entrance -> station (u)
            Route p_enterRoute = u.Room.RoomEnterRoutes[entrance.r_ID - u.Room.Stations.Length][u.r_ID];

            float totalDist = p_roomRoute.Distance + r_S2.Distance;

            if (totalDist < dist)
            {
                roomRoute = p_roomRoute;
                enterRoute = p_enterRoute;
                dist = totalDist;
            }
        }

        return roomRoute.Join(enterRoute);
    }

    // Get the route between a station vertex and a room vertex
    private Route StationToRoom(Vertex s, Room room)
    {
        Route exitRoute = null;
        Route roomRoute = null;

        float dist = Mathf.Infinity;

        // Compare the routes out of the current room from the current station with the destination room vertices
        foreach (Route r_S1 in s.Room.RoomExitRoutes[s.r_ID])
        {
            foreach (Vertex r_V2 in room.Vertices)
            {
                // Vertex to exit the room from the current station
                Vertex exit = r_S1.Vertices.Last();
                // Route from the exit to the destination room vertex
                Route p_roomRoute = Map.Routes[exit.g_ID][r_V2.g_ID];

                float totalDist = r_S1.Distance + p_roomRoute.Distance;

                if (totalDist < dist)
                {
                    exitRoute = r_S1;
                    roomRoute = p_roomRoute;
                    dist = totalDist;
                }
            }
        }

        return exitRoute.Join(roomRoute);
    }

    // Get the route between a station vertex and a station vertex
    private Route StationToStation(Vertex s, Vertex u)
    {
        if (s.Room == u.Room) return new Route(s, u);

        Route exitRoute = null;
        Route roomRoute = null;
        Route enterRoute = null;

        float dist = Mathf.Infinity;

        // Compare the current station exit routes to the destination station entry routes
        foreach (Route r_S1 in s.Room.RoomExitRoutes[s.r_ID])
        {
            foreach (Route r_S2 in u.Room.RoomExitRoutes[u.r_ID])
            {
                // Exit and entry vertices
                Vertex exit = r_S1.Vertices.Last();
                Vertex entrance = r_S2.Vertices.Last();

                // Route between the exit and the entry vertices
                Route p_roomRoute = Map.Routes[exit.g_ID][entrance.g_ID];
                // The route from the entrance vertex to the destination station
                Route p_enterRoute = u.Room.RoomEnterRoutes[entrance.r_ID - u.Room.Stations.Length][u.r_ID];

                float totalDist = r_S1.Distance + p_roomRoute.Distance + r_S2.Distance;

                if (totalDist < dist)
                {
                    exitRoute = r_S1;
                    roomRoute = p_roomRoute;
                    enterRoute = p_enterRoute;
                    dist = totalDist;
                }
            }
        }

        return exitRoute.Join(roomRoute).Join(enterRoute);
    }

    // Get the list of rooms with type <T>
    public List<Room> GetRoomsOfType(RoomType T)
    {
        if (_lookupRooms.ContainsKey(T))
        {
            return _lookupRooms[T];
        }

        return new List<Room>();
    }

    # endregion

    # region Utility

    private Route CompareRoutes(Route route, Route other) => route.CompareTo(other) == 1 ? route : other;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        foreach (Vertex vertex in _roomVertices)
        {
            foreach (Edge edge in vertex.Edges)
            {
                if (edge.Enabled && !edge.End.Station)
                {
                    Gizmos.DrawLine(vertex.transform.position, edge.End.transform.position);
                    Debug.DrawLine(vertex.transform.position, edge.End.transform.position, Color.red);
                }
            }
        }
    }

    # endregion
}
