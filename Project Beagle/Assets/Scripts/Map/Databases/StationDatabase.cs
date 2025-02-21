using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map/StationDatabase")]
public class StationDatabase : ScriptableObject
{
    public StationType[] stationTypes;
    Dictionary<StationType, int> lookup;
    
    private void OnEnable() 
    {
        if (lookup == null) lookup = new Dictionary<StationType, int>();

        for (int i = 0; i < stationTypes.Length; i++)
        {
            if (lookup.TryGetValue(stationTypes[i], out var none)) continue;

            lookup.Add(stationTypes[i], i);

            Debug.Log($"Database adding {stationTypes[i]}");
        }
    }

    public int GetIndex(StationType t)
    {
        if (lookup.TryGetValue(t, out var index))
        {
            return index;
        }

        return -1;
    }
}
