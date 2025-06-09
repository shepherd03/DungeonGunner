using UnityEngine;


[CreateAssetMenu(fileName = "RoomNodeType", menuName = "Scriptable Objects/Dungeon/Room Node Type", order = 0)]
public class RoomNodeTypeSO : ScriptableObject
{
    public string roomNodeTypeName;

    [Header("此房间类型是否可以在编辑界面显示")] public bool displayInNodeGraphEditor;

    [Header("是否为走廊")] public bool isCorridor;

    [Header("是否为南北方向走廊")] public bool isCorridorNS;

    [Header("是否为东西方向走廊")] public bool isCorridorEW;

    [Header("是否为入口")] public bool isEntrance;

    [Header("是否为Boss房")] public bool isBossRoom;

    [Header("是否未分配")] public bool isNone;

    #region Validation

#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(roomNodeTypeName), roomNodeTypeName);
    }
#endif

    #endregion
}