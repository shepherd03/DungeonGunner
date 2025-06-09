using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Object = UnityEngine.Object;

public class RoomNodeGraphEditor : EditorWindow
{
    public static RoomNodeGraphSO currentRoomNodeGraph;

    /// <summary>
    /// 当前正在操作的房间节点
    /// </summary>
    private RoomNodeSO currentRoomNode;

    private RoomNodeTypeListSO roomNodeTypeList;

    private GUIStyle roomNodeStyle;
    private GUIStyle roomNodeSelectedStyle;

    private Vector2 graphOffeset;
    private Vector2 graphDrag;

    private float gridLarge = 100f;
    private float gridSmall = 25f;

    private float connectionLineWidth = 3f;
    private float connectingLineArrowSize = 6f;

    private const float nodeWidth = 160f;
    private const float nodeHeight = 75f;
    private const int nodePadding = 25;
    private const int nodeBorder = 12;

    [MenuItem("房间节点编辑器", menuItem = "Window/地牢编辑/房间节点编辑器")]
    private static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor");
    }

    private void OnEnable()
    {
        Selection.selectionChanged += InspectorSelectionChanged;

        roomNodeStyle = new GUIStyle();
        roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        roomNodeStyle.normal.textColor = Color.white;
        roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        roomNodeSelectedStyle = new GUIStyle();
        roomNodeSelectedStyle.normal.background = EditorGUIUtility.Load("node1 on") as Texture2D;
        roomNodeSelectedStyle.normal.textColor = Color.white;
        roomNodeSelectedStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeSelectedStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= InspectorSelectionChanged;
    }

    /// <summary>
    /// 双击一个node graph时，打开房间节点编辑器
    /// </summary>
    [OnOpenAsset(0)]
    public static bool OnDoubleClickAsset(int instanceID, int line)
    {
        RoomNodeGraphSO roomNodeGraph = EditorUtility.InstanceIDToObject(instanceID) as RoomNodeGraphSO;
        if (roomNodeGraph != null)
        {
            // TODO:可能有问题
            // TODO:可能有问题
            roomNodeGraph.roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
            OpenWindow();

            currentRoomNodeGraph = roomNodeGraph;

            return true;
        }

        return false;
    }

    private void OnGUI()
    {
        if (currentRoomNodeGraph != null)
        {
            DrawBackgroundGrid(gridSmall, 0.2f, Color.gray);
            DrawBackgroundGrid(gridLarge, 0.3f, Color.gray);

            DrawDraggedLine();

            ProcessEvents(Event.current);

            DrawRoomConnections();

            DrawRoomNodes();
        }

        if (GUI.changed)
        {
            Repaint();
        }
    }

    private void DrawBackgroundGrid(float gridSize, float gridOpacity, Color gridColor)
    {
        int verticalLineCount = Mathf.CeilToInt((this.position.width + gridSize) / gridSize);
        int horizontalLineCount = Mathf.CeilToInt((this.position.height + gridSize) / gridSize);

        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);
        graphOffeset += graphDrag * 0.5f;

        Vector3 gridOffset = new Vector3(graphOffeset.x % gridSize, graphOffeset.y % gridSize, 0);

        for (int i = 0; i < verticalLineCount; i++)
        {
            Handles.DrawLine(new Vector3(gridSize * i, -gridSize, 0) + gridOffset,
                new Vector3(gridSize * i, position.height + gridSize, 0) + gridOffset);
        }

        for (int j = 0; j < horizontalLineCount; j++)
        {
            Handles.DrawLine(new Vector3(-gridSize, j * gridSize, 0) + gridOffset,
                new Vector3(position.width + gridSize, gridSize * j, 0) + gridOffset);
        }

        Handles.color = Color.white;
    }

    private void DrawDraggedLine()
    {
        if (currentRoomNodeGraph.linePosition != Vector2.zero)
        {
            Handles.DrawBezier(currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center,
                currentRoomNodeGraph.linePosition,
                currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition,
                Color.white, null, connectionLineWidth);
        }
    }

    private void ProcessEvents(Event currentEvent)
    {
        graphDrag = Vector2.zero;

        // 当前房间为空 或者 没有在拖拽房间
        if (currentRoomNode == null || !currentRoomNode.isLeftClickDragging)
        {
            currentRoomNode = IsMouseOverRoomNode(currentEvent);
        }

        // 当前房间为空 或者 
        if (currentRoomNode == null || currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            PrecessRoomNodeGraphEvents(currentEvent);
        }
        else
        {
            currentRoomNode.ProcessEvents(currentEvent);
        }

        // 快捷键处理
        ProcessShortcutKeys(currentEvent);
    }

    private void ProcessShortcutKeys(Event currentEvent)
    {
        // 只处理键盘事件
        if (currentEvent.type != EventType.KeyDown && currentEvent.type != EventType.KeyUp)
            return;

        switch (currentEvent.keyCode)
        {
            case KeyCode.N: // 新建节点
                if (currentEvent.type == EventType.KeyDown && currentEvent.modifiers == EventModifiers.None)
                {
                    CreateRoomNode(currentEvent.mousePosition);
                    currentEvent.Use(); // 标记事件已处理
                }

                break;

            case KeyCode.A:
                if (currentEvent.type == EventType.KeyDown &&
                    currentEvent.control && // 检查是否按下了 Ctrl
                    currentEvent.modifiers == EventModifiers.Control) // 确保只有 Ctrl 被按下
                {
                    SelectAllRoomNode();
                    currentEvent.Use();
                }

                break;

            case KeyCode.Delete: // 删除选中节点
                if (currentEvent.type == EventType.KeyDown)
                {
                    DeleteSelectedRoomNode();
                    currentEvent.Use();
                }

                break;

            case KeyCode.Backspace: // 删除选中节点(备用)
                if (currentEvent.type == EventType.KeyDown && currentEvent.modifiers == EventModifiers.None)
                {
                    DeleteSelectedRoomNode();
                    currentEvent.Use();
                }

                break;

            case KeyCode.L: // 删除选中节点间的连接
                if (currentEvent.type == EventType.KeyDown && currentEvent.modifiers == EventModifiers.None)
                {
                    DeleteSelectedRoomNodeLinks();
                    currentEvent.Use();
                }

                break;

            case KeyCode.F: // 聚焦入口
                if (currentEvent.type == EventType.KeyDown && currentEvent.modifiers == EventModifiers.None)
                {
                    FocusOnEntrance();
                    currentEvent.Use();
                }

                break;
        }
    }

    /// <summary>
    /// 检查鼠标所在的房间节点
    /// </summary>
    private RoomNodeSO IsMouseOverRoomNode(Event currentEvent)
    {
        foreach (var roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.rect.Contains(currentEvent.mousePosition))
            {
                return roomNode;
            }
        }

        return null;
    }

    private void PrecessRoomNodeGraphEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;
            default:
                break;
        }
    }

    private void ProcessMouseDownEvent(Event currentEvent)
    {
        if (currentEvent.button == 1)
        {
            ShowContextMenu(currentEvent.mousePosition);
        }
        else if (currentEvent.button == 0)
        {
            ClearLineDrag();
            ClearAllSelectedRoomNodes();
        }
    }

    private void ProcessMouseDragEvent(Event currentEvent)
    {
        if (currentEvent.button == 1)
        {
            ProcessRightMouseDragEvent(currentEvent);
        }
        else if (currentEvent.button == 2)
        {
            ProcessMidMouseDragEvent(currentEvent);
        }
    }

    private void ProcessMouseUpEvent(Event currentEvent)
    {
        if (currentEvent.button == 1 && currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            RoomNodeSO roomNode = IsMouseOverRoomNode(currentEvent);
            if (roomNode != null)
            {
                // 加入当前选中节点的子节点列表
                if (currentRoomNodeGraph.roomNodeToDrawLineFrom.AddChildRoomNodeIDToRoomNode(roomNode.id))
                {
                    roomNode.AddParentRoomNodeIDToRoomNode(currentRoomNodeGraph.roomNodeToDrawLineFrom.id);
                }
            }

            ClearLineDrag();
        }
    }

    private void ProcessMidMouseDragEvent(Event currentEvent)
    {
        graphDrag = currentEvent.delta;

        for (int i = 0; i < currentRoomNodeGraph.roomNodeList.Count; i++)
        {
            currentRoomNodeGraph.roomNodeList[i].DragNode(currentEvent.delta);
        }

        GUI.changed = true;
    }

    private void ProcessRightMouseDragEvent(Event currentEvent)
    {
        if (currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            DragConnectingLine(currentEvent.delta);
            GUI.changed = true;
        }
    }

    private void DragConnectingLine(Vector2 delta)
    {
        currentRoomNodeGraph.linePosition += delta;
    }

    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();

        menu.AddItem(new GUIContent("新建房间节点"), false, CreateRoomNode, mousePosition);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("选取全部房间"), false, SelectAllRoomNode);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("删除选中的房间节点间的连接"), false, DeleteSelectedRoomNodeLinks);
        menu.AddItem(new GUIContent("删除选中的房间"), false, DeleteSelectedRoomNode);
        menu.AddItem(new GUIContent("聚焦于入口"), false, FocusOnEntrance);

        menu.ShowAsContext();
    }

    private void ClearLineDrag()
    {
        currentRoomNodeGraph.roomNodeToDrawLineFrom = null;
        currentRoomNodeGraph.linePosition = Vector2.zero;
        GUI.changed = true;
    }

    private void ClearAllSelectedRoomNodes()
    {
        foreach (var roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.isSelected = false;

                GUI.changed = true;
            }
        }
    }

    private void CreateRoomNode(object mousePositionObject)
    {
        //如果没有节点，创建一个入口节点
        if (currentRoomNodeGraph.roomNodeList.Count == 0)
        {
            CreateRoomNode(new Vector2(200f, 200f), roomNodeTypeList.list.Find(x => x.isEntrance));
        }

        //创建一个空节点
        CreateRoomNode(mousePositionObject, roomNodeTypeList.list.Find(x => x.isCorridor));
    }

    private void CreateRoomNode(object mousePositionObject, RoomNodeTypeSO roomNodeType)
    {
        Vector2 mousePosition = (Vector2)mousePositionObject;
        RoomNodeSO roomNode = CreateInstance<RoomNodeSO>();

        currentRoomNodeGraph.roomNodeList.Add(roomNode);

        roomNode.Initialize(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), currentRoomNodeGraph,
            roomNodeType);

        AssetDatabase.AddObjectToAsset(roomNode, currentRoomNodeGraph);

        AssetDatabase.SaveAssets();

        //重新加载节点字典
        currentRoomNodeGraph.OnValidate();
    }

    private void SelectAllRoomNode()
    {
        foreach (var roomNode in currentRoomNodeGraph.roomNodeList)
        {
            roomNode.isSelected = true;
        }

        GUI.changed = true;
    }

    /// <summary>
    /// 移除选中的房间之间的连接
    /// </summary>
    private void DeleteSelectedRoomNodeLinks()
    {
        foreach (var roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected && roomNode.childRoomNodeIDList.Count > 0)
            {
                for (int i = roomNode.childRoomNodeIDList.Count - 1; i >= 0; i--)
                {
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(roomNode.childRoomNodeIDList[i]);

                    if (childRoomNode != null && childRoomNode.isSelected)
                    {
                        roomNode.RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }
            }
        }

        ClearAllSelectedRoomNodes();
    }


    private void DeleteSelectedRoomNode()
    {
        Queue<RoomNodeSO> roomNodeDeleteQueue = new Queue<RoomNodeSO>();
        foreach (var roomNode in currentRoomNodeGraph.roomNodeList)
        {
            //不删除入口
            if (roomNode.isSelected && !roomNode.roomNodeType.isEntrance)
            {
                roomNodeDeleteQueue.Enqueue(roomNode);

                //删除子房间的连接
                foreach (var childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(childRoomNodeID);
                    if (childRoomNode != null)
                    {
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }

                //删除父房间的连接
                foreach (var parentRoomNodeID in roomNode.parentRoomNodeIDList)
                {
                    RoomNodeSO parentRoomNode = currentRoomNodeGraph.GetRoomNode(parentRoomNodeID);
                    if (parentRoomNode)
                    {
                        parentRoomNode.RemoveChildRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }
            }
        }

        while (roomNodeDeleteQueue.Count > 0)
        {
            RoomNodeSO roomNodeToDelete = roomNodeDeleteQueue.Dequeue();
            //从字典和列表中移除
            currentRoomNodeGraph.roomNodeDictionary.Remove(roomNodeToDelete.id);
            currentRoomNodeGraph.roomNodeList.Remove(roomNodeToDelete);
            AssetDatabase.RemoveObjectFromAsset(roomNodeToDelete);
        }

        AssetDatabase.SaveAssets();
    }

    private void DrawRoomConnections()
    {
        foreach (var roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.childRoomNodeIDList.Count > 0)
            {
                foreach (var childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    if (currentRoomNodeGraph.roomNodeDictionary.ContainsKey(childRoomNodeID))
                    {
                        DrawConnectionLine(roomNode, currentRoomNodeGraph.roomNodeDictionary[childRoomNodeID]);

                        GUI.changed = true;
                    }
                }
            }
        }
    }

    private void DrawConnectionLine(RoomNodeSO parentRoomNode, RoomNodeSO childRoomNode)
    {
        Vector2 startPosition = parentRoomNode.rect.center;
        Vector2 endPosition = childRoomNode.rect.center;
        Vector2 midPosition = (startPosition + endPosition) / 2;

        Vector2 direction = endPosition - startPosition;

        //箭头 new Vector2(-direction.y, direction.x) 是方向向量的垂直向量（旋转90度）
        Vector2 arrowTailPoint1 =
            midPosition - new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;
        Vector2 arrowTailPoint2 =
            midPosition + new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;
        Vector2 arrowHeadPoint = midPosition + direction.normalized * connectingLineArrowSize;

        Handles.DrawBezier(arrowTailPoint1, arrowHeadPoint,
            arrowTailPoint1, arrowHeadPoint, Color.white, null,
            connectionLineWidth);

        Handles.DrawBezier(arrowTailPoint2, arrowHeadPoint,
            arrowTailPoint2, arrowHeadPoint, Color.white, null,
            connectionLineWidth);

        Handles.DrawBezier(startPosition, endPosition,
            startPosition, endPosition, Color.white, null,
            connectionLineWidth);
        GUI.changed = true;
    }

    private void DrawRoomNodes()
    {
        foreach (var roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (!roomNode.isSelected)
            {
                roomNode.Draw(roomNodeStyle);
            }
            else
            {
                roomNode.Draw(roomNodeSelectedStyle);
            }
        }

        GUI.changed = true;
    }

    public void FocusOnEntrance()
    {
        RoomNodeSO entrance = null;
        foreach (var roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.roomNodeType.isEntrance)
            {
                entrance = roomNode;
                break;
            }
        }

        if (entrance == null)
        {
            Debug.LogWarning("当前暂无Entrance");
            return;
        }

        Vector2 offset = -entrance.rect.center + this.position.center;
        graphOffeset += offset / 2;
        foreach (var roomNode in currentRoomNodeGraph.roomNodeList)
        {
            roomNode.DragNode(offset);
        }
    }

    /// <summary>
    /// 当编辑器的选中项变化时调用
    /// </summary>
    private void InspectorSelectionChanged()
    {
        RoomNodeGraphSO roomNodeGraph = Selection.activeObject as RoomNodeGraphSO;

        if (roomNodeGraph != null)
        {
            currentRoomNodeGraph = roomNodeGraph;
            GUI.changed = true;
        }
    }
}