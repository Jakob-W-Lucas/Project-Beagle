using UnityEditor.Rendering;
using UnityEngine;

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

        Transform agentTransform = context.agent.transform;

        bool isInRange = false;//agentTransform.InRangeOf(targetTransform, maxDistance, maxAngle);
        if (!isInRange) return 0f;

        Vector3 directionToTarget = targetTransform.position - agentTransform.position;
        float distanceToTarget = 0;//directionToTarget.With(z:0).magnitude;

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
