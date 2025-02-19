using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Sensor))]
public class Brain : MonoBehaviour
{
    [SerializeField] private List<AIAction> actions;
    public Context context;
    
    private void Awake() 
    {
        context = new Context(this);

        foreach (var action in actions)
        {
            action.Initialize(context);
        }
    }

    // Chooses the best Action for the crew to take 
    public AIAction ChooseAction() {
        
        AIAction bestAction = null;
        float highestUtility = float.MinValue;

        foreach (var action in actions) {
            float utility = action.CalcualteUtility(context);
            if (utility > highestUtility) {
                highestUtility = utility;
                bestAction = action;
            }
        }

        return bestAction;
    }
}
