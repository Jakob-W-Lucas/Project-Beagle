using UnityEngine;

[CreateAssetMenu(menuName = "UtilityAI/Actions/IdleAction")]
public class IdleAction : AIAction
{
    public override void Execute(Context context)
    {
        Debug.Log("Idle");
    }
}
