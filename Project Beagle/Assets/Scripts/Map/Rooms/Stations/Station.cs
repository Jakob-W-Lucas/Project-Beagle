using System.Collections.Generic;
using UnityEngine;

/*

Station abstact class

*/
[RequireComponent(typeof(Vertex))]
public abstract class Station : MonoBehaviour
{
    // Position of the station in the map
    public Vertex Vertex { get; private set; }
    // Room the station belongs to
    public Room Room => Vertex.Room;
    private List<Agent> _agents;
    [SerializeField] private int _capacity;

    public abstract void DebugStation();

    public virtual bool Avaliable => _capacity != _agents.Count;

    public virtual bool Occupy(Agent a)
    {
        if (_capacity == _agents.Count || _agents.Contains(a)) return false;

        _agents.Add(a);

        return true;
    }

    public virtual bool Vacate(Agent a)
    {
        if (!_agents.Contains(a)) return false;

        _agents.Remove(a);

        return true;
    }
    
    // Set up the station for appropriate path finding
    public void ConfigureStation(Room room)
    {
        _agents = new List<Agent>( _capacity );

        Vertex = GetComponent<Vertex>();
        
        Vertex.Room = room;
        Vertex.Station = this;

        // Station does not exist in the global map, therefore -1 g_ID
        Vertex.g_ID = -1;

        if (Vertex == null)
        {
            Debug.LogWarning("Station vertex has not been properly configured");
            return;
        }

        // Add each edge between the station and the room vertices
        foreach (Vertex v in room.Vertices)
        {
            Vertex.AddEdge(v);
            v.AddEdge(Vertex);
        }
    }
}
