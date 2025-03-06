using System;
using UnityEngine;

[CreateAssetMenu(menuName = "UtilityAI/Actions/Travel Action")]
public class TravelAction : AIAction
{
    public override void Execute(Context context)
    {
        context.agent.SetState(ActionState.Travel);
    }
}
