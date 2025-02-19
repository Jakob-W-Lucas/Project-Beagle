using UnityEditor.Rendering.LookDev;
using UnityEngine;

[CreateAssetMenu(menuName = "UtilityAI/Considerations/Constant")]
public class ConstantConsideration : Consideration
{
    public float value;
    public override float Evaluate(Context context) => value;
}
