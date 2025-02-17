using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;
using System.Linq;

public class OuterMap : MonoBehaviour
{
    public Map Map { get; private set; }
    private Room[] _rooms;
    private Dictionary<Type, List<Room>> _lookupRooms = new Dictionary<Type, List<Room>>();
    private Dictionary<Type, List<Station>> _lookupStations = new Dictionary<Type, List<Station>>();
    private List<Vertex> _roomVertices = new List<Vertex>();

    # region Initialization

    private void OnEnable() {
        
        _rooms = GetComponentsInChildren<Room>();

        if (_rooms.Length == 0) return;

        int n = 0;
        foreach (Room room in _rooms)
        {   
            room.ConfigureRoom();
            AddToLookup(room);
            
            foreach (Vertex v in room.Vertices)
            {
                v.g_ID = n;
                n++;

                _roomVertices.Add(v);
            }
        }

        Map = new Map(_roomVertices.ToArray());
    }

    private void AddToLookup(Room r)
    {
        Type t = r.GetType();

        if (!_lookupRooms.ContainsKey(t))
        {
            _lookupRooms[t] = new List<Room>();
        }

        _lookupRooms[t].Add(r);

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

    private Route CompareRoutes(Route route, Route other) => route.CompareTo(other) == 1 ? route : other;

    public Route TravelToStation<T>(Vertex s, Station u_st = null) where T : Station
    {
        if (u_st && s.Room == u_st.Room) return null;

        bool s_station = s.g_ID == -1;

        List<Station> stations = u_st ? new List<Station>() { u_st } : _lookupStations[typeof(T)];

        Route contender = new Route();

        if (stations.Contains(s.Station)) return null;

        foreach (Station st in stations)
        {
            if (s_station) {
                contender = CompareRoutes(contender, StationToStation(s, st.Vertex)); 
                continue;
            }
            
            contender = CompareRoutes(contender, RoomToStation(s, st.Vertex));
        }

        return contender;
    }

    public Route TravelToRoom<T>(Vertex s, Room u_room = null) where T : Room
    {
        if (s.Room == u_room) return null;

        bool s_station = s.g_ID == -1;

        List<Room> rooms = u_room ? new List<Room>() { u_room } : _lookupRooms[typeof(T)];

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

    private Route RoomToRoom(Vertex s, Room room)
    {
        Route contender = null;

        foreach (Vertex r_V2 in room.Vertices)
            {
                Route roomRoute = Map.Routes[s.g_ID][r_V2.g_ID];

                float totalDist = roomRoute.Distance;

                contender = CompareRoutes(contender, roomRoute);
            }
        return contender;
    }

    private Route RoomToStation(Vertex s, Vertex u)
    {
        Route roomRoute = null;
        Route enterRoute = null;

        float dist = Mathf.Infinity;

        foreach (Route r_S2 in u.Room.RoomExitRoutes[u.r_ID])
        {
            Vertex entrance = r_S2.Vertices.Last();

            Route p_roomRoute = Map.Routes[s.g_ID][entrance.g_ID];
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

    private Route StationToRoom(Vertex s, Room room)
    {
        Route exitRoute = null;
        Route roomRoute = null;

        float dist = Mathf.Infinity;

        foreach (Route r_S1 in s.Room.RoomExitRoutes[s.r_ID])
        {
            foreach (Vertex r_V2 in room.Vertices)
            {
                Vertex exit = r_S1.Vertices.Last();

                Route p_roomRoute = Map.Routes[exit.g_ID][r_V2.g_ID];

                float totalDist = r_S1.Distance + roomRoute.Distance;

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

    private Route StationToStation(Vertex s, Vertex u)
    {
        Route enterRoute = null;
        Route roomRoute = null;
        Route exitRoute = null;

        float dist = Mathf.Infinity;

        foreach (Route r_S1 in s.Room.RoomExitRoutes[s.r_ID])
        {
            foreach (Route r_S2 in u.Room.RoomExitRoutes[u.r_ID - u.Room.Stations.Length])
            {
                Vertex exit = r_S1.Vertices.Last();
                Vertex entrance = r_S2.Vertices.Last();

                Route p_roomRoute = Map.Routes[exit.g_ID][entrance.g_ID];
                Route p_exitRoute = u.Room.RoomEnterRoutes[entrance.r_ID - u.Room.Stations.Length][u.r_ID];

                float totalDist = r_S1.Distance + roomRoute.Distance + r_S2.Distance;

                if (totalDist < dist)
                {
                    enterRoute = r_S1;
                    roomRoute = p_roomRoute;
                    exitRoute = p_exitRoute;
                    dist = totalDist;
                }
            }
        }

        return enterRoute.Join(roomRoute).Join(exitRoute);
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
