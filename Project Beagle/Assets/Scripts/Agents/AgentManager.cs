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

    private Route GetAgentRoute(Agent a)
    {   
        int rand = UnityEngine.Random.Range(0, 15);

        Route fromOrigin = RandomRoute(a.Origin, rand);

        if (a.Heading == null) return fromOrigin;

        Route fromHeading = RandomRoute(a.Heading, rand);

        return fromOrigin.Distance < fromHeading.Distance ? fromOrigin : fromHeading;
    }

    private Route RandomRoute(Vertex s, int rand)
    {
        if (rand == 0)
        {
            return OuterMap.TravelToStation<Cage>(s);
        }
        if (rand == 1)
        {
            return OuterMap.TravelToStation<Desk>(s);
        }
        if (rand == 2)
        {
            return OuterMap.TravelToStation<Bed>(s);
        }
        if (rand == 3)
        {
            return OuterMap.TravelToStation<ChartTable>(s);
        }
        if (rand == 4)
        {
            return OuterMap.TravelToStation<Basin>(s);
        }
        if (rand == 5)
        {
            return OuterMap.TravelToStation<Toilet>(s);
        }
        if (rand == 6)
        {
            return OuterMap.TravelToStation<Cannon>(s);
        }
        if (rand == 7)
        {
            return OuterMap.TravelToStation<Hammock>(s);
        }
        if (rand == 8)
        {
            return OuterMap.TravelToStation<Table>(s);
        }
        if (rand == 9)
        {
            return OuterMap.TravelToRoom<Brig>(s);
        }
        if (rand == 10)
        {
            return OuterMap.TravelToRoom<Dock>(s);
        }
        if (rand == 11)
        {
            return OuterMap.TravelToRoom<CaptainsQuaters>(s);
        }
        if (rand == 12)
        {
            return OuterMap.TravelToRoom<GunDeck>(s);
        }
        if (rand == 13)
        {
            return OuterMap.TravelToRoom<Forecastle>(s);
        }

        return OuterMap.TravelToRoom<Galley>(s);
    }

    # endregion

    # region "Movement"

    void UpdatePosition(Agent a)
    {
        if (a.Heading == null) 
        {
            if (a.Origin)
            {
                a.FollowPath(GetAgentRoute(a));
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
