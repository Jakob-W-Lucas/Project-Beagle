using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map/RoomDatabase")]
public class RoomDatabase : ScriptableObject
{
    public RoomType[] roomTypes;
    Dictionary<string, RoomType> lookup = new Dictionary<string, RoomType>();

    private void Awake() 
    {
        for (int i = 0; i < roomTypes.Length; i++)
        {
            if (lookup.TryGetValue(roomTypes[i].Name, out var none))
            {
                continue;
            }

            lookup.Add(roomTypes[i].Name, roomTypes[i]);
        }
    }

    public RoomType GetRoom(string str) 
    {
        if (lookup.TryGetValue(str, out var room))
        {
            return room;
        }

        return null;
    }
}
