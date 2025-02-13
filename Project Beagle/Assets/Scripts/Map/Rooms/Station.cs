using UnityEngine;

public abstract class Station : MonoBehaviour
{
    public Vertex Vertex { get; private set; }

    public void ConfigureStation(Room room)
    {
        foreach (Vertex v in room.Verticies)
        {
            Vertex.AddEdge(v);
        }
    }
}
