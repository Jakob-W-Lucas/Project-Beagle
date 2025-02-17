using UnityEngine;

[RequireComponent(typeof(Vertex))]
public abstract class Station : MonoBehaviour
{
    public Vertex Vertex { get; private set; }
    public Room Room => Vertex.Room;

    public abstract void DebugStation();
    
    public void ConfigureStation(Room room)
    {
        Vertex = GetComponent<Vertex>();
        
        Vertex.Room = room;
        Vertex.Station = this;

        Vertex.g_ID = -1;

        if (Vertex == null)
        {
            Debug.LogWarning("Station vertex has not been properly configured");
            return;
        }

        foreach (Vertex v in room.Vertices)
        {
            Vertex.AddEdge(v);
            v.AddEdge(Vertex);
        }
    }
}
