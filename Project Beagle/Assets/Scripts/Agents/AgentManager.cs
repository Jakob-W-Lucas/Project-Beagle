using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        foreach (Agent a in _agents)
        {
            a.UpdateOrigin(_map.GetNearestVertex(a.transform.position));
        }
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

    # region Path Finding

    private Route GetAgentRoute(Agent a, int u)
    {   
        Vertex v = _map.GetVertexFromIndex(u);
        Route fromOrigin = _map.Routes[a.Origin.ID][v.ID];

        if (a.Heading == null) return fromOrigin;

        Route fromHeading = _map.Routes[a.Heading.ID][v.ID];

        return fromOrigin.Distance < fromHeading.Distance ? fromOrigin : fromHeading;
    }

    private void SetRouteToRoom<T>(Agent a) where T : Room
    {
        if (a.Origin.Room.GetType() == typeof(T)) return;

        a.FollowPath(OuterMap.FromRoomTypeToRoom<T>(a.Origin));
    }

    # endregion

    # region "Movement"

    void UpdatePosition(Agent a)
    {
        if (a.Heading == null) 
        {
            if (a.Origin)
            {
                //a.FollowPath(GetAgentRoute(a, UnityEngine.Random.Range(0, 13)));
                a.FollowPath(OuterMap.TravelToStation<Toilet>(a.Origin));
            }
            
            return;
        }

        if (NaiveDistanceCheck(a.transform.position, a.Heading.transform.position)) {
            a.transform.position = Vector2.MoveTowards(a.transform.position, a.Heading.transform.position, a.Speed * Time.fixedDeltaTime);
            return; 
        }

        a.UpdateOrigin(a.Heading);
        
        if (a.Route.Count == 0) {
            a.Heading = null;
            return;
        }

        a.Heading = a.Route.Dequeue();
    }

    bool NaiveDistanceCheck(Vector2 a, Vector2 b) {
        // Naive check for distance, faster computation
        return Math.Abs(a.x - b.x) > 0.01f || Math.Abs(a.y - b.y) > 0.01f;
    }

    bool OptimalDistanceCheck(Vector2 a, Vector2 b) {
        // Better check for distance, slower computation
        return Vector2.SqrMagnitude(a - b) > 0.001f;
    }

    # endregion
}
