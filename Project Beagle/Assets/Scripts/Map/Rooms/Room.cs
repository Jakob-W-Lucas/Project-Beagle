using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class Room : MonoBehaviour
{
    // Verticies to enter and exit any room
    private Vertex[] _verticies;
    private Collider2D _bounds;

    public Vertex[] Verticies => _verticies;

    public abstract void DebugRoom();

    private void OnEnable() 
    {
        _bounds = GetComponent<Collider2D>();
    }

    public void ConfigureRoom()
    {
        _verticies = GetComponentsInChildren<Vertex>();

        foreach (Vertex v in _verticies)
        {
            v.Room = this;
            v.ConfigureVertex();
        }
    }
}
