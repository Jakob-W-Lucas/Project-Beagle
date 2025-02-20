using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Sensor))]
public class Brain : MonoBehaviour
{
    private Agent _agent;
    [SerializeField] private List<AIAction> actions;
    public Context context;
    private Health _health;
    
    private void Awake() 
    {
        _health = GetComponent<Health>();
        context = new Context(this);
        
        foreach (var action in actions)
        {
            action.Initialize(context);
        }
    }

    // Chooses the best Action for the crew to take 
    public AIAction ChooseAction() {
        
        UpdateContent();

        AIAction bestAction = null;
        float highestUtility = float.MinValue;

        foreach (var action in actions) {
            float utility = action.CalcualteUtility(context);
            Debug.Log($"The utility of {action.name} is {utility}");
            if (utility > highestUtility) {
                highestUtility = utility;
                bestAction = action;
            }
        }

        Debug.Log($"Best action is currently: {bestAction.name}");
        return bestAction;
    }

    public void UpdateContent() {
        context.SetData("health", _health.CurrentHealth / 100);
    }
}
