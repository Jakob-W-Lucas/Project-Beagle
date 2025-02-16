using UnityEngine;

[RequireComponent(typeof(Vertex))]
public abstract class Station : MonoBehaviour
{
    public Vertex Vertex { get; private set; }
    public Room Room => Vertex.Room;

    public abstract void DebugStation();
    
    public void ConfigureStation(Room room, int ID)
    {
        Vertex = GetComponent<Vertex>();
        Vertex.r_ID = ID;
        Vertex.p_ID = 0;
        
        Vertex.Room = room;
        Vertex.Station = this;

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
