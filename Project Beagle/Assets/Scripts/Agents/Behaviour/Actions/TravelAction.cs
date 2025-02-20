using System;
using UnityEngine;

public class TravelAction : AIAction
{
    // [SerializeField] private OuterMap _map;
    // [SerializeField] private Room _targetRoom;
    // [SerializeField] private Station _targetStation;
    public override void Execute(Context context)
    {
        context.agent.BGAgent.SetVariableValue("Action State", ActionState.Travel);
    }

    void Dummy()
    {
        // if (_targetRoom && _targetStation) {
        //     Debug.LogWarning("Travel to action is trying to target a room ({targetRoom}) and a station ({targetStation}) simultaneously");
        //     return;
        // }

        // Route originRoute = _targetStation ? 
        //                     _map.TravelToStation(context.agent.Origin, _targetStation.GetType()) : 
        //                     _map.TravelToRoom(context.agent.Origin, _targetStation.GetType());

        // if (!context.agent.Heading) {
        //     context.agent.FollowPath(originRoute);
        //     return;
        // }

        // Route headingRoute =    _targetStation ? 
        //                         _map.TravelToStation(context.agent.Heading, _targetStation.GetType()) : 
        //                         _map.TravelToRoom(context.agent.Heading, _targetStation.GetType());

        // Route bestRoute = originRoute.Distance < headingRoute.Distance ? originRoute : headingRoute;

        // context.agent.FollowPath(bestRoute);
    }
}
