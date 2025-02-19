using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class Sensor : MonoBehaviour
{
    private bool _checking = true;
    [SerializeField] private float _detectionRadius;
    [SerializeField] private List<string> targetTags = new();
    [SerializeField] private LayerMask _obstructionMask;
    public List<Transform> _detectedObjects = new(10);

    private void Start() 
    {
        UpdatePerception();
    }

    public void UpdatePerception()
    {
        if (!_checking)
        {  
           Debug.Log("No longer checking surroundings");
           return;
        }

        // Reset the detected objects so we don't process them again
        _detectedObjects.Clear();

        // Get every collider in the detection radius and process the transform
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _detectionRadius);
        foreach (Collider2D c in colliders){
            
            // If the collider is already in the detected list, move on
            if (_detectedObjects.Contains(c.transform)) continue;

            // If a collider is not being obstructed, process it (add to list)
            ProcessTrigger(c, transform => { if (!IsObstructed(c)) _detectedObjects.Add(transform); });
        }
    }

    // Determines if we want to care about a collider
    void ProcessTrigger(Collider2D other, Action<Transform> action) {

        if (other.CompareTag("Untagged")) return;

        // If the collider has a relevant tag process the action
        foreach (string t in targetTags) {
            if (other.CompareTag(t)) {

                if (_detectedObjects.Count == 10)
                {
                    ReplaceFurthestTag(t, other.transform);
                    return;
                }

                action(other.transform);
                
                return;
            }
        }
    }

    // Checks if the collider is obstructed 
    private bool IsObstructed(Collider2D collider)
    {
        Vector2 closestPoint = collider.ClosestPoint(this.transform.position);
        Vector2 directionToTarget = (closestPoint - (Vector2)this.transform.position).normalized;
        float distanceToTarget = Vector2.Distance(closestPoint, this.transform.position);

        // Cast a ray to the closest point on the target checking for objects with the obstruction layer
        // If the ray hits, we know the object is being obstructed 
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position, directionToTarget, distanceToTarget, _obstructionMask);
        if (hit.collider != null)
        {
            Debug.DrawLine(this.transform.position, hit.point, Color.red);
            return true;
        }
        else
        {
            Debug.DrawLine(this.transform.position, closestPoint, Color.green);
            return false;
        }
    }

    // Returns the closet target in the detected objects list with the relevent tag
    public Transform GetClosestTarget(string tag) {

        if (_detectedObjects.Count == 0) return null;

        Transform cloestTarget = null;
        float cloestDistanceSqr = Mathf.Infinity;
        Vector2 currentPosition = transform.position;

        // Compare the distances of each target to find the closest one
        foreach (Transform potentialTarget in _detectedObjects) {
            if (potentialTarget.CompareTag(tag)) {
                Vector2 directionToTarget = (Vector2)potentialTarget.position - currentPosition;
                float dSqrToTarget = directionToTarget.sqrMagnitude;
                if (dSqrToTarget < cloestDistanceSqr) {
                    cloestDistanceSqr = dSqrToTarget;
                    cloestTarget = potentialTarget;
                }
            }
        }
        
        return cloestTarget;
    }

    private void ReplaceFurthestTag(string tag, Transform newTransform)
    {
        if (_detectedObjects.Count == 0) return;

        // Obtains the target tag with the furthest distance
        Transform furthestTarget = null;
        float furthestDistanceSqr = 0;
        Vector2 currentPosition = transform.position;

        // Compare the distances of each target to find the closest one
        foreach (Transform potentialTarget in _detectedObjects) {
            if (potentialTarget.CompareTag(tag)) {
                Vector2 directionToTarget = (Vector2)potentialTarget.position - currentPosition;
                float dSqrToTarget = directionToTarget.sqrMagnitude;
                if (dSqrToTarget > furthestDistanceSqr) {
                    furthestDistanceSqr = dSqrToTarget;
                    furthestTarget = potentialTarget;
                }
            }
        }
        
        // If the new target to add is closer than the furthest 
        Vector2 directionToNewTarget = (Vector2)newTransform.position - currentPosition;
        float dSqrToNewTarget = directionToNewTarget.sqrMagnitude;

        if (dSqrToNewTarget > furthestDistanceSqr)
        {
            return;
        }

        _detectedObjects.Remove(furthestTarget);
        _detectedObjects.Add(newTransform);
    }
}
