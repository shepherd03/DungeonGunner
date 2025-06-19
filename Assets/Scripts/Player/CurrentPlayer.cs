using UnityEngine;

[CreateAssetMenu(fileName = "CurrentPlayer", menuName = "Scriptable Objects/Player/Current Player")]
public class CurrentPlayer : ScriptableObject
{
    public PlayerDetailsSO playerDetails;
    public string playerName;
}