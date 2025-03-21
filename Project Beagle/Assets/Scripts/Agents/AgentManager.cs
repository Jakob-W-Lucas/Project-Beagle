using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class FollowAgent
{
    public Agent Target;
    public float HardDistance;
    public float SoftDistance;
}

public class AgentManager : MonoBehaviour
{
    [SerializeField] private float _delayUpdateTime = 0.5f;
    [SerializeField] private Agent[] _agents;
    public OuterMap OuterMap;
    private Map _map;

    # region Updates

    private void Start() 
    {
        _map = OuterMap.Map;

        StartCoroutine(DelayUpdate());
    }

    private void FixedUpdate() 
    {
        foreach (Agent a in _agents)
        {
            if (!a.isActiveAndEnabled || !OuterMap.Map.Configured) continue;

            UpdatePosition(a);
        }
}

    private IEnumerator DelayUpdate()
    {
        // Run tasks on a delay to avoid lots of computation
        while (true)
        {
            foreach (Agent a in _agents)
            {
                if (a == null) continue;
                if (!a.enabled || a.Brain == null || a.Sensor == null) continue;

                // Update the sensor and content, then get a new action
                a.Sensor?.UpdatePerception();
                a.Brain.ChooseAction();
            }
            
            yield return new WaitForSeconds(_delayUpdateTime);
        }
    }
    
    # endregion

    # region "Movement"

    // Move an agent towards its next target position
    void UpdatePosition(Agent a)
    {   
        NavigationState nav = a.Navigation;
        if (nav.Heading == null) return;
        
        // Continue to move towards the heading vertex if distance is greater than 0.01 in either cardinal direction
        if (NaiveDistanceCheck(a.transform.position, nav.Heading.Position)) {
            a.transform.position = Vector2.MoveTowards(a.transform.position, nav.Heading.Position, nav.MovementSpeed * Time.fixedDeltaTime);

            //nav.DebugPathSegment();
            return; 
        }

        nav.UpdateOrigin(nav.Heading);

        a.transform.position = nav.Heading.Position;
    }

    bool NaiveDistanceCheck(Vector2 a, Vector2 b) {
        // Naive check for distance, faster computation
        return Math.Abs(a.x - b.x) > 0.01f || Math.Abs(a.y - b.y) > 0.01f;
    }

    bool OptimalDistanceCheck(Vector2 a, Vector2 b) {
        // Better check for distance, slower computation
        return Vector2.SqrMagnitude(a - b) > 0.001f;
    }

    void OnDrawGizmos()
    {
        foreach (Agent a in _agents)
        {
            if (a.GetState() == ActionState.Idle || a.Navigation?.Pointer == null) continue;

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(a.Navigation.Pointer.Position, 0.1f);
        }
    }

    # endregion
}
