using System.Collections.Generic;
using UnityUtils;

public class Context
{
    public Brain brain;
    public Sensor sensor;
    public Agent agent;

    readonly Dictionary<string, object> Data = new();

    public Context(Brain brain)
    {
        this.brain = brain;
        this.sensor = brain.gameObject.GetComponent<Sensor>();
        this.agent = brain.gameObject.GetComponent<Agent>();
    }

    public T GetData<T>(string key) => Data.TryGetValue(key, out var value) ? (T)value : default;
    public void SetData(string key, object value) => Data[key] = value; 
}
