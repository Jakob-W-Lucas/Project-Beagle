using UnityEngine;

public abstract class Consideration : ScriptableObject
{
    public abstract float Evaluate(Context context);
}
