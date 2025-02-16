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
                v.ID = n;
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

    // public Route TravelToRoom<T>(Vertex s, Room u_room = null) where T : Room
    // {
    //     List<Room> rooms = u_room ? new List<Room>() { u_room } : _lookupRooms[typeof(T)];

    //     Route contender = null;
    //     float d = Mathf.Infinity;

    //     bool lookup = Map.HasVertex(s);

    //     foreach (Room r in rooms)
    //     {
    //         Route p_route = lookup ? r.GetRouteToRoom(s) : StationToRoom(s.Station, r);

    //         if (p_route == null || p_route.Distance > d) continue;

    //         contender = p_route;
    //         d = p_route.Distance;
    //     }

    //     return contender;
    // }

    // public Route TravelToStation<S>(Vertex s, Station u_st = null) where S : Station
    // {
    //     List<Station> stations = u_st ? new List<Station>() { u_st } : _lookupStations[typeof(S)];

    //     Route contender = null;
    //     float d = Mathf.Infinity;

    //     bool lookup = Map.HasVertex(s);

    //     foreach (Station st in stations)
    //     {
    //         Route p_route = lookup ? RoomToStation(s, st) : StationToStation(s.Station, st);

    //         if (p_route == null || p_route.Distance > d) continue;

    //         contender = p_route;
    //         d = p_route.Distance;
    //     }

    //     return contender;
    // }

    // Get the route from the source to the room of type T with lowest total distance
    // public Route FromRoomTypeToRoom<T>(Vertex s) where T : Room
    // {
    //     List<Room> rooms = GetRoomsOfType<T>();

    //     Route contender = null;
    //     float dist = Mathf.Infinity;

    //     foreach (Room r in rooms)
    //     {
    //         Route p_route = r.GetRouteToRoom(s);

    //         if (p_route.Distance > dist) continue;

    //         contender = p_route;
    //         dist = p_route.Distance;
    //     }

    //     return contender;
    // }

    // The shortest route between a station and a station
    // private Route StationToStation(Station s_st, Station u_st)
    // {
    //     // Get the route from the start station to each room vertex
    //     Route[] startRoutes = s_st.Room.RouteFromStation(s_st);

    //     Route s_contender = null;
    //     Route u_contender = null;
    //     float d = Mathf.Infinity;

    //     foreach (Route startRoute in startRoutes)
    //     {
    //         // Get the route from the room vertex to the target station
    //         Route endRoute = RoomToStation(startRoute.Vertices.Last(), u_st);

    //         if (endRoute == null || startRoute == null) continue;

    //         float totalDistance = startRoute.Distance + endRoute.Distance;

    //         if (totalDistance < d)
    //         {
    //             s_contender = startRoute;
    //             u_contender = endRoute;
    //             d = totalDistance;
    //         }
    //     }

    //     return s_contender.Join(u_contender);
    // }

    // private Route StationToRoom(Station s, Room r)
    // {
    //     Route contender = null;
    //     float d = Mathf.Infinity;

    //     foreach (Vertex v in r.Vertices)
    //     {
    //         Route p_route = RoomToStation(v, s);

    //         if (p_route == null || p_route.Distance > d) continue;

    //         contender = p_route;
    //         d = p_route.Distance;
    //     }

    //     contender.Vertices.Reverse();
    //     return contender;
    // }

    // The shortest route between a room and a station
    // private Route RoomToStation(Vertex s, Station st)
    // {
    //     // Get the route from the room vertex to the station in another room
    //     Route[] routes = st.Room.RouteFromStation(st);

        
    //     for (int i = 0; i < routes.Length; i++)
    //     {
    //         Debug.Log($"Route {i} has vertices:\n");
    //         foreach (Vertex v in routes[i].Vertices)
    //         {
    //             Debug.Log($"        {v.Name},\n");
    //         }
    //     }

    //     if (routes == null) return new Route(new List<Vertex>(), Mathf.Infinity);

    //     Route s_contender = null;
    //     Route u_contender = null;
    //     float d = Mathf.Infinity;

    //     foreach (Route route in routes)
    //     {
    //         // Retrieves the route from the start to a room vertex
    //         Route p_route = Map.Routes[s.GuidID][route.Vertices.Last().GuidID];

    //         if (p_route == null) continue;

    //         float totalDistance = p_route.Distance + route.Distance;

    //         if (totalDistance < d)
    //         {
    //             s_contender = route;
    //             u_contender = p_route;
    //             d = totalDistance;
    //         }
    //     }

    //     s_contender.Vertices.Reverse();
    //     return u_contender.Join(s_contender);
    // }

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
