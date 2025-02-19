using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Context : MonoBehaviour
{
    public Brain brain;
    public Sensor sensor;

    readonly Dictionary<string, object> Data = new();

    public Context(Brain brain)
    {
        // Precondition null check

        this.brain = brain;
        this.sensor = brain.gameObject.GetComponent<Sensor>();
    }

    public T GetData<T>(string key) => Data.TryGetValue(key, out var value) ? (T)value : default;
    public void SetData(string key, object value) => Data[key] = value; 
}
