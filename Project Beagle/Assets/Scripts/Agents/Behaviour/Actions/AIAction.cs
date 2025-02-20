using UnityEngine;

public abstract class AIAction : ScriptableObject
{
    public Consideration consideration;

    public virtual void Initialize(Context context)
    {
        // Optional initialization logic
    }
    public float CalcualteUtility(Context context) => consideration.Evaluate(context);
    public abstract void Execute(Context context);
}
