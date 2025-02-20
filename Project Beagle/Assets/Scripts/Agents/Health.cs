using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float _health;
    public float CurrentHealth => _health;
}
