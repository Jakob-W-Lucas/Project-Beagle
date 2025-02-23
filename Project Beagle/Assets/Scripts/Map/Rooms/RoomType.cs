using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Map/Room")]
public class RoomType : ScriptableObject
{
    // Place in database
    public int ID;
    public string Name;
    [TextArea(10, 40)][SerializeField] private string Description;
}
