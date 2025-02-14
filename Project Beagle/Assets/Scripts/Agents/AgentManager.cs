using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    [SerializeField] private float _delayUpdateTime = 0.5f;
    [SerializeField] private Agent[] _agents;
    public OuterMap OuterMap;

    # region Updates

    private void Start() 
    {
        StartCoroutine(DelayUpdate());
    }

    private void FixedUpdate() 
    {
        foreach (Agent a in _agents)
        {
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
                if (!a.enabled /*|| a.Brain == null || a.Sensor == null*/) continue;

                // Update the sensor and content, then get a new action
                //a.Sensor?.UpdatePerception();
                //a.UpdateContent();
                //a.CurrentAction = a.Brain.ChooseAction();
            }
            
            yield return new WaitForSeconds(_delayUpdateTime);
        }
    }
    
    # endregion

    # region "Movement"

    void UpdatePosition(Agent a)
    {
        if (a.Heading == null) return;

        if (NaiveDistanceCheck(a.transform.position, a.Heading.transform.position))
        {
            a.transform.position = Vector2.MoveTowards(a.transform.position, a.Heading.transform.position, a.Speed * Time.fixedDeltaTime);
            return; 
        }
        
        if (a.Route.Count == 0)
        {
            a.Heading = null;

            //a.FollowPath(OuterMap.Map.Routes[a.Origin][UnityEngine.Random.Range(0, 1)].Verticies);
            return;
        }

        a.Origin = a.Heading;
        a.Heading = a.Route.Dequeue();
    }

    bool NaiveDistanceCheck(Vector2 a, Vector2 b) {
        // Naive check for distance, faster computation
        return Math.Abs(a.x - b.x) > 0.01f || Math.Abs(a.y - b.y) > 0.01f;
    }

    bool OptimalDistanceCheck(Vector2 a, Vector2 b)
    {
        // Better check for distance, slower computation
        return Vector2.SqrMagnitude(a - b) > 0.001f;
    }

    # endregion
}
