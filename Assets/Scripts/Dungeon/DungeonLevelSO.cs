using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DungeonLevel_", menuName = "Scriptable Objects/Dungeon/Dungeon Level")]
public class DungeonLevelSO : ScriptableObject
{
    public string levelName;
    
    public List<RoomTemplateSO> roomTemplateList;
    
    [Space(10)]
    public List<RoomNodeGraphSO> roomNodeGraphList;

    #region 验证

    #if UNITY_EDITOR

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this,nameof(levelName),levelName);

        if (HelperUtilities.ValidateCheckEnumerableValues(this, nameof(roomTemplateList), roomTemplateList))
        {
            return;
        }
        
        if (HelperUtilities.ValidateCheckEnumerableValues(this, nameof(roomNodeGraphList), roomNodeGraphList))
        {
            return;
        }
        
        //首先检查南北走廊、东西走廊以及入口类型
        
        bool isNSCorridor = false;
        bool isEWCorridor = false;
        bool isEntrance = false;

        foreach (var roomTemplate in roomTemplateList)
        {
            if (roomTemplate == null)
            {
                return;
            }

            isNSCorridor = roomTemplate.roomNodeType.isCorridorNS;
            isEWCorridor = roomTemplate.roomNodeType.isCorridorEW;
            isEntrance = roomTemplate.roomNodeType.isEntrance;

            if (!isNSCorridor)
            {
                Debug.Log($"{roomTemplate.name}中没有指定南北走廊");
            }

            if (!isEWCorridor)
            {
                Debug.Log($"{roomTemplate.name}中没有指定东西走廊");
            }

            if (!isEntrance)
            {
                Debug.Log($"{roomTemplate.name}中没有指定入口");
            }
        }

        foreach (var roomNodeGraph in roomNodeGraphList)
        {
            if (roomNodeGraph == null) return;
            foreach (var roomNode in roomNodeGraph.roomNodeList)
            {
                if(roomNode == null) continue;

                RoomNodeTypeSO roomNodeType = roomNode.roomNodeType;
                if (roomNodeType.isEntrance || roomNodeType.isCorridorEW || roomNodeType.isCorridorNS ||
                    roomNodeType.isCorridor || roomNodeType.isNone)
                {
                    continue;
                }
                
                bool isRoomNodeTypeFound = false;

                if (roomTemplateList.Find(roomTemplate => roomTemplate.roomNodeType == roomNodeType) != null)
                {
                    isRoomNodeTypeFound = true;
                    break;
                }

                if (!isRoomNodeTypeFound)
                {
                    Debug.Log($"文件{this.name}:{roomNodeGraph.name}中缺少房间模板类型{roomNode.roomNodeType.roomNodeTypeName}");
                }
            }
        }
    }

#endif

    #endregion
}