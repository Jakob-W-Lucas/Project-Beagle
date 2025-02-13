using UnityEngine;

[RequireComponent(typeof(Vertex))]
public abstract class Station : MonoBehaviour
{
    public Vertex Vertex { get; private set; }
    
    public void ConfigureStation(Room room)
    {
        Vertex = GetComponent<Vertex>();

        if (Vertex == null)
        {
            Debug.LogWarning("Station vertex has not been properly configured");
            return;
        }

        foreach (Vertex v in room.Verticies)
        {
            Vertex.AddEdge(v);
        }
    }
}
