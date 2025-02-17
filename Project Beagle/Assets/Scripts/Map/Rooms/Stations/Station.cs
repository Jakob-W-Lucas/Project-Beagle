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

    public abstract void DebugStation();
    
    // Set up the station for appropriate path finding
    public void ConfigureStation(Room room)
    {
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
