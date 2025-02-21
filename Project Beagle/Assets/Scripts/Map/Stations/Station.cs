using System.Collections.Generic;
using UnityEngine;

/*

Station abstact class

*/
[RequireComponent(typeof(Vertex))]
public class Station : MonoBehaviour
{
    public StationType Type;
    // Position of the station in the map
    public Vertex Vertex { get; private set; }
    // Room the station belongs to
    public Room Room => Vertex.Room;
    // All agents currently occupying or heading to occupy the station
    private List<Agent> _agents;
    // Capacity of the station
    [SerializeField] private int _capacity;

    public virtual bool Avaliable => _capacity == -1 || _capacity != _agents.Count;

    // Add an agent to the room to have the agent 'occupy' the station
    public virtual bool Occupy(Agent a)
    {
        if (_capacity == -1) return true;

        if (_capacity == _agents.Count || _agents.Contains(a)) return false;

        _agents.Add(a);

        return true;
    }

    // Remove an agent from the agents list to ensure it 'leaves' the room
    public virtual bool Vacate(Agent a)
    {
        if (_capacity == -1) return true;
        
        if (!_agents.Contains(a)) return false;

        _agents.Remove(a);

        return true;
    }
    
    // Set up the station for appropriate path finding
    public void ConfigureStation(Room room)
    {
        _agents = _capacity == -1 ? null : new List<Agent>( _capacity );

        Vertex = GetComponent<Vertex>();

        Vertex.ConfigureVertex(room, transform.position, this);

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
