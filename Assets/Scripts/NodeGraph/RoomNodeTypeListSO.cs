using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "RoomTypeListSO", menuName = "Scriptable Objects/Dungeon/Room Node Type List")]
public class RoomNodeTypeListSO : ScriptableObject
{
    [Space(10)]
    [Header("房间类型列表")]
    [Tooltip("用于替代枚举的，应当填入此游戏的所有房间类型")]
    public List<RoomNodeTypeSO> list;
    
    #region Validation

#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(list), list);
    }
#endif

    #endregion
    
}