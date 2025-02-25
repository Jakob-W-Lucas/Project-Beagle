using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;
using System.Linq;
using Unity.VisualScripting;

public class OuterMap : MonoBehaviour
{
    public RoomDatabase roomDatabase;
    public StationDatabase stationDatabase;
    public Map Map { get; private set; }
    [SerializeField] private Room[] _rooms;
    private List<Room>[] _lookupRooms;
    private List<Station>[] _lookupStations;
    private List<Vertex> _roomVertices = new List<Vertex>();

    # region Initialization

    private void OnEnable() {
        
        _lookupRooms = new List<Room>[roomDatabase.roomTypes.Length];
        _lookupStations = new List<Station>[stationDatabase.stationTypes.Length];

        if (_rooms.Length == 0) return;

        int n = 0;
        foreach (Room room in _rooms)
        {  
            room.ConfigureRoom();
            AddToLookup(room);
            
            foreach (Vertex v in room.Vertices)
            {
                v.g_ID = n;
                _roomVertices.Add(v);

                n++;
            }
        }

        Map = new Map(_roomVertices.ToArray());
    }

    private void AddToLookup(Room r)
    {
        int index = roomDatabase.GetIndex(r.Type);

        if (index == -1) 
        {
            Debug.LogWarning($"The room of type {r.Type}, does not exist in the current database");
            return;
        }

        if (_lookupRooms[index] == null)
        {
            _lookupRooms[index] = new List<Room>();
        }

        _lookupRooms[index].Add(r);

        foreach (Station s in r.Stations)
        {
            AddStationToLookup(s);
        }
    }

    private void AddStationToLookup(Station s)
    {
        int index = stationDatabase.GetIndex(s.Type);

        if (index == -1) 
        {
            Debug.LogWarning($"The room of type {s.Type}, does not exist in the current database");
            return;
        }

        if (_lookupStations[index] == null)
        {
            _lookupStations[index] = new List<Station>();
        }

        _lookupStations[index].Add(s);
    }

    # endregion

    # region Querying

    public List<Vertex> SetVertices(Agent a)
    {
        List<Vertex> vertices;
        if (a.Origin.r_ID == -1 || a.Heading.r_ID == -1)
        {
            vertices = GetNearestRoomVertex(a).ToList();
        }
        else
        {
            vertices = new List<Vertex>() { a.Origin };
            if (a.Heading != null) vertices.Add(a.Heading);
        }

        return vertices;
    }

    /// <summary>
    /// Determines the best route for an agent to travel based on the provided action.
    /// </summary>
    /// <param name="a">The agent for which the travel route is being determined.</param>
    /// <param name="action">A function that takes a vertex and returns a route.</param>
    /// <returns>The route with the shorter distance between the agent's origin and heading.</returns>
    
    public Route Travel(Agent a, StationType T, Station u_st = null)
    {
        Route route = new Route();
        foreach (Vertex v in SetVertices(a))
        {
            route = CompareRoutes(route, TravelToStation(v, T, u_st));
        }

        return route;
    }

    public Route Travel(Agent a, RoomType T, Room u_room = null)
    {
        Route route = new Route();
        foreach (Vertex v in SetVertices(a))
        {
            route = CompareRoutes(route, TravelToRoom(v, T, u_room));
        }

        return route;
    }

    // Returns the path from any vertex to any station
    public Route TravelToStation(Vertex s, StationType T, Station u_st = null)
    {
        if ((u_st && s.Station == u_st) || (s.Station && s.Station.Type == T)) return null;

        bool s_station = s.g_ID == -1;

        int index = T ? stationDatabase.GetIndex(T) : -1;

        if (T && index == -1)
        {
            Debug.LogWarning($"The room of type {T}, does not exist in the current database");
            return null;
        }

        List<Station> stations = u_st ? new List<Station>() { u_st } : _lookupStations[index];

        Route contender = new Route();

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
        if (s == null) return null;

        bool s_station = s.g_ID == -1;

        int index = T ? roomDatabase.GetIndex(T) : -1;

        if (T && index == -1)
        {
            Debug.LogWarning($"The room of type {T}, does not exist in the current database");
            return null;
        }

        List<Room> rooms = u_room ? new List<Room>() { u_room } : _lookupRooms[index];

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
        Route roomRoute = new Route();
        Route enterRoute = new Route();

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
        Route exitRoute = new Route();
        Route roomRoute = new Route();

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

    /// <summary>
    /// Retrieves the closest room vertices to the left and right of the agent
    /// </summary>
    /// <param name="a">The agent for which the travel route is being determined.</param>
    /// <returns>The cloest vertex to the left and the right of the agent.</returns>

    public Vertex[] GetNearestRoomVertex(Agent a)
    {
        List<Vertex> vertices = new List<Vertex>{ Map.GetNearestVertex(a.transform.position) };
        Vertex[] leftRightVertices = new Vertex[2] { vertices[0], null};

        int direction = vertices[0].Position.x > a.transform.position.x ? 1 : -1;
        foreach (Vertex v in vertices)
        {
            int newDirection = v.Position.x > a.transform.position.x ? 1 : -1;
            if (newDirection != direction)
            {
                leftRightVertices[1] = v;
                break;
            }
        }

        return leftRightVertices;
    }

    # endregion

    # region Utility

    public Route CompareRoutes(Route route, Route other) => route.CompareTo(other) == 1 ? route : other;

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
