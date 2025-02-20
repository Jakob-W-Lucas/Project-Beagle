using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;

[RequireComponent(typeof(Sensor))]
public class Brain : MonoBehaviour
{
    [SerializeField] private BehaviorGraphAgent _agent;
    [SerializeField] private List<AIAction> actions;
    public Context context;
    private Health _health;
    
    private void Awake() 
    {
        _agent = GetComponent<BehaviorGraphAgent>();
        _health = GetComponent<Health>();
        context = new Context(this);
        
        foreach (var action in actions)
        {
            action.Initialize(context);
        }
    }

    // Chooses the best Action for the crew to take 
    public void ChooseAction() {
        
        UpdateContent();

        AIAction bestAction = null;
        float highestUtility = float.MinValue;

        foreach (var action in actions) {
            float utility = action.CalcualteUtility(context);
            if (utility > highestUtility) {
                highestUtility = utility;
                bestAction = action;
            }
        }

        _agent.SetVariableValue("Action State", bestAction);
    }

    public void UpdateContent() {
        context.SetData("health", _health.CurrentHealth / 100);
    }
}
