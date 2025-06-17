using UnityEngine;

[DisallowMultipleComponent]
public class Health : MonoBehaviour
{
    private int _startHealth;
    private int _currentHealth;

    public int StartHealth
    {
        get => _startHealth;
        set
        {
            _startHealth = value;
            _currentHealth = _startHealth;
        }
    }
}