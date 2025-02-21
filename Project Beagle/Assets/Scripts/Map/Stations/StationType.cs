using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Map/Station")]
public class StationType : ScriptableObject
{
    public string Name;
    [TextArea(10, 40)][SerializeField] private string Description;
}
