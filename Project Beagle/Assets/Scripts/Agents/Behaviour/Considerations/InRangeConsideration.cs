using UnityEditor.Rendering;
using UnityEngine;
using UnityUtils;

[CreateAssetMenu(menuName = "UtilityAI/Considerations/InRange")]
public class InRangeConsideration : Consideration
{
    public float maxDistance = 10f;
    public float maxAngle = 360f;
    public string targetTag = "Target";
    public AnimationCurve curve;

    public override float Evaluate(Context context) {
        if (!context.sensor.targetTags.Contains(targetTag)) {
            context.sensor.AddTag(targetTag);
        }

        Transform targetTransform = context.sensor.GetClosestTarget(targetTag);
        if (targetTransform == null) return 0f;
        
        Vector2 targetPosition = (Vector2)targetTransform.position;

        Vector2 agentTransform = context.agent.transform.position;

        bool isInRange = agentTransform.InRangeOf(targetPosition, maxDistance, maxAngle);
        if (!isInRange) return 0f;

        float distanceToTarget = Vector2.Distance(targetPosition, agentTransform);

        float normalizedDistance = Mathf.Clamp01(distanceToTarget / maxDistance);

        float utility = curve.Evaluate(normalizedDistance);
        return Mathf.Clamp01(utility);
    }

    void Reset()
    {
        curve = new AnimationCurve (
            new Keyframe(0f, 1f),
            new Keyframe(1f, 0f)
        );
    }
}
