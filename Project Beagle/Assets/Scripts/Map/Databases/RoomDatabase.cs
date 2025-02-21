using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map/RoomDatabase")]
public class RoomDatabase : ScriptableObject
{
    public RoomType[] roomTypes;
    Dictionary<RoomType, int> lookup;
    
    private void OnEnable() 
    {
        if (lookup == null) lookup = new Dictionary<RoomType, int>();

        for (int i = 0; i < roomTypes.Length; i++)
        {
            if (lookup.TryGetValue(roomTypes[i], out var none)) continue;

            lookup.Add(roomTypes[i], i);

            Debug.Log($"Database adding {roomTypes[i]}");
        }
    }

    public int GetIndex(RoomType t)
    {
        if (lookup.TryGetValue(t, out var index))
        {
            return index;
        }

        return -1;
    }
}
