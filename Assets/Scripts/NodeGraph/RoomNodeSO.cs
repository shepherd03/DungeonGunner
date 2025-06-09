using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class RoomNodeSO : ScriptableObject
{
    public string id;
    public List<string> parentRoomNodeIDList = new List<string>();
    public List<string> childRoomNodeIDList = new List<string>();
    public RoomNodeGraphSO belongRoomNodeGraph;
    public RoomNodeTypeSO roomNodeType;
    public RoomNodeTypeListSO roomNodeTypeList;

    #region 编辑器代码

#if UNITY_EDITOR
    [HideInInspector] public Rect rect;
    [HideInInspector] public bool isLeftClickDragging;
    [HideInInspector] public bool isSelected;

    public void Initialize(Rect rect, RoomNodeGraphSO nodeGraph, RoomNodeTypeSO roomNodeType)
    {
        this.rect = rect;
        this.id = Guid.NewGuid().ToString();
        this.name = "RoomNode";
        this.belongRoomNodeGraph = nodeGraph;
        this.roomNodeType = roomNodeType;

        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    public void Draw(GUIStyle roomNodeStyle)
    {
        GUILayout.BeginArea(rect, roomNodeStyle);

        EditorGUI.BeginChangeCheck();

        //有父节点或者为入口节点，则设置为固定label样式
        if (parentRoomNodeIDList.Count > 0 || roomNodeType.isEntrance)
        {
            EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);
        }
        else
        {
            int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);

            int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());

            roomNodeType = roomNodeTypeList.list[selection];
            
            //TODO:?
            RoomNodeTypeSO selectedRoomNodeType = roomNodeTypeList.list[selected];
            RoomNodeTypeSO selectionRoomNodeType = roomNodeTypeList.list[selection];

            if (selectedRoomNodeType.isCorridor && !selectionRoomNodeType.isCorridor
                || selectedRoomNodeType.isCorridor && selectionRoomNodeType.isCorridor
                || !selectedRoomNodeType.isBossRoom && selectionRoomNodeType.isBossRoom)
            {
                if (childRoomNodeIDList.Count > 0)
                {
                    for(int i = childRoomNodeIDList.Count - 1; i>= 0;i--)
                    {
                        RoomNodeSO childRoomNode = belongRoomNodeGraph.GetRoomNode(childRoomNodeIDList[i]);

                        if (childRoomNode != null)
                        {
                            RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);
                            childRoomNode.RemoveParentRoomNodeIDFromRoomNode(id);
                        }
                    }
                }
            }
        }

        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(this);

        GUILayout.EndArea();
    }

    public string[] GetRoomNodeTypesToDisplay()
    {
        string[] roomArray = new string[roomNodeTypeList.list.Count];

        for (int i = 0; i < roomArray.Length; i++)
        {
            if (belongRoomNodeGraph.roomNodeTypeList.list[i].displayInNodeGraphEditor)
            {
                roomArray[i] = belongRoomNodeGraph.roomNodeTypeList.list[i].roomNodeTypeName;
            }
        }

        return roomArray;
    }

    public void ProcessEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;
            default:
                break;
        }
    }

    private void ProcessMouseDownEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            ProcessLeftMouseClickDownEvent();
        }
        else if (currentEvent.button == 1)
        {
            ProcessRightMouseClickDownEvent(currentEvent);
        }
    }

    private void ProcessLeftMouseClickDownEvent()
    {
        // 使当前资产文件在编辑器中变为选中
        Selection.activeObject = this;
        isSelected = !isSelected;
    }

    private void ProcessRightMouseClickDownEvent(Event currentEvent)
    {
        belongRoomNodeGraph.SetNodeToDrawConnectionLineForm(this, currentEvent.mousePosition);
    }

    private void ProcessMouseUpEvent(Event currentEvent)
    {
        if (isLeftClickDragging)
        {
            isLeftClickDragging = false;
        }
    }

    private void ProcessMouseDragEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent);
        }
    }

    private void ProcessLeftMouseDragEvent(Event currentEvent)
    {
        isLeftClickDragging = true;
        DragNode(currentEvent.delta);
        GUI.changed = true;
    }

    public void DragNode(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }

    public bool AddChildRoomNodeIDToRoomNode(string childRoomNodeID)
    {
        if (IsChildRoomValid(childRoomNodeID))
        {
            childRoomNodeIDList.Add(childRoomNodeID);
            return true;
        }

        return false;
    }

    private bool IsChildRoomValid(string childRoomNodeID)
    {
        bool isConnectedBossNodeAlready = false;
        //检查是否已经有房间和当前房间图表的boos房连接
        foreach (var roomNode in belongRoomNodeGraph.roomNodeList)
        {
            if (roomNode.roomNodeType.isBossRoom && roomNode.parentRoomNodeIDList.Count > 0)
            {
                isConnectedBossNodeAlready = true;
                break;
            }
        }

        RoomNodeSO childRoomNode = belongRoomNodeGraph.GetRoomNode(childRoomNodeID);

        //如果该节点为boos房 并且 已经有房间和boss房链接了 
        if (childRoomNode.roomNodeType.isBossRoom && isConnectedBossNodeAlready) return false;

        if (childRoomNode.roomNodeType.isNone) return false;

        //该节点已经为自己的子节点了
        if (childRoomNodeIDList.Contains(childRoomNodeID)) return false;

        //不能为自己
        if (id == childRoomNodeID) return false;

        if (parentRoomNodeIDList.Contains(childRoomNodeID)) return false;

        //每个节点只能有一个父节点
        if (childRoomNode.parentRoomNodeIDList.Count > 0) return false;

        //子节点和自己都为走廊也不能连接
        if (childRoomNode.roomNodeType.isCorridor && roomNodeType.isCorridor) return false;
        
        //如果子节点为走廊 并且 子节点的走廊数量（每一个房间都对应了一个走廊）已经达到上限
        if(childRoomNode.roomNodeType.isCorridor && childRoomNodeIDList.Count >= Settings.maxChildCorridors) return false;
        
        //入口不能是子节点
        if (childRoomNode.roomNodeType.isEntrance) return false;
        
        //如果已经连接到房间了 并且 要连接的子节点不是一个走廊  
        if(!childRoomNode.roomNodeType.isCorridor && childRoomNodeIDList.Count > 0) return false;

        return true;
    }

    public bool AddParentRoomNodeIDToRoomNode(string parentRoomNodeID)
    {
        parentRoomNodeIDList.Add(parentRoomNodeID);
        return true;
    }

    public bool RemoveChildRoomNodeIDFromRoomNode(string childRoomNodeID)
    {
        if (childRoomNodeIDList.Contains(childRoomNodeID))
        {
            childRoomNodeIDList.Remove(childRoomNodeID);
            return true;
        }
        return false;
    }

    public bool RemoveParentRoomNodeIDFromRoomNode(string parentRoomNodeID)
    {
        if (parentRoomNodeIDList.Contains(parentRoomNodeID))
        {
            parentRoomNodeIDList.Remove(parentRoomNodeID);
            return true;
        }
        return false;
    }

#endif

    #endregion
}