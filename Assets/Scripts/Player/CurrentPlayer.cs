using UnityEngine;

[CreateAssetMenu(fileName = "CurrentPlayer", menuName = "ScriptableObjects/Player/Current Player")]
public class CurrentPlayer : ScriptableObject
{
    public PlayerDetailsSO playerDetails;
    public string playerName;
}