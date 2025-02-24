using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Map/Room")]
public class RoomType : ScriptableObject
{
    [SerializeField] private string _name;
    [TextArea(10, 40)][SerializeField] private string _description;

    string Name => _name;
    string Description => _description;
}
