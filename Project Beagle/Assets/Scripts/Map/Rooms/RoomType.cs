using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Map/Room")]
public class RoomType : ScriptableObject , ILocation
{
    [SerializeField] private string _name;
    [TextArea(10, 40)][SerializeField] private string _description;

    string ILocation.Name() => _name;
    string ILocation.Description() => _description;
}
