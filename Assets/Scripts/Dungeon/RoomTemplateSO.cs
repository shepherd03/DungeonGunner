using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 房间模板ScriptableObject
/// 定义房间的物理属性、预制体、门道配置和生成点位置
/// 这是地牢生成系统中房间数据的模板，用于创建具体的房间实例
/// 
/// 主要功能：
/// - 存储房间预制体和基础属性
/// - 定义房间边界和门道位置
/// - 提供生成点位置用于放置游戏元素
/// - 自动生成GUID确保唯一性
/// </summary>
[CreateAssetMenu(fileName = "Room_", menuName = "Scriptable Objects/Dungeon/Room")]
public class RoomTemplateSO : ScriptableObject
{
    /// <summary>
    /// 房间模板的全局唯一标识符
    /// 用于在运行时识别和关联房间模板
    /// </summary>
    [HideInInspector] public string guid;

    [Space(10)]
    [Header("房间预制体")]
    /// <summary>
    /// 房间的预制体GameObject
    /// 包含房间的视觉表现、碰撞体、瓦片地图等组件
    /// </summary>
    public GameObject prefab;

    /// <summary>
    /// 上一次的预制体引用，用于检测预制体变化以重新生成GUID
    /// 确保复制ScriptableObject时能生成新的唯一标识
    /// </summary>
    [HideInInspector] public GameObject previousPrefab;

    [Space(10)]
    [Header("配置")]
    /// <summary>
    /// 房间节点类型（入口、走廊、普通房间、Boss房间等）
    /// 决定房间在地牢图中的作用和连接规则
    /// </summary>
    public RoomNodeTypeSO roomNodeType;

    [Header("左下角边界")]
    /// <summary>
    /// 房间的左下角边界坐标（瓦片地图坐标系）
    /// 用于定义房间的占用空间和碰撞检测
    /// </summary>
    public Vector2Int lowerBounds;

    [Header("右上角边界")]
    /// <summary>
    /// 房间的右上角边界坐标（瓦片地图坐标系）
    /// 与lowerBounds一起定义房间的完整边界矩形
    /// </summary>
    public Vector2Int upperBounds;

    [Header("门")]
    [Tooltip("每一个门对应一项")]
    /// <summary>
    /// 房间的门道列表
    /// 定义房间可以连接其他房间的位置和方向
    /// </summary>
    [SerializeField] private List<Doorway> doorwayList;

    #region Tooltip
    [Tooltip("Each possible spawn position (used for enemies and chests) for the room in tilemap coordinates should be added to this array")]
    #endregion Tooltip
    /// <summary>
    /// 房间内的生成点位置数组
    /// 用于在房间中放置敌人、宝箱、道具等游戏元素
    /// 坐标基于瓦片地图坐标系
    /// </summary>
    public Vector2Int[] spawnPositionArray;

    /// <summary>
    /// 获取房间的门道列表
    /// 提供对私有doorwayList的只读访问
    /// </summary>
    public List<Doorway> GetDoorwayList => doorwayList;

    #region 验证器与工具

#if UNITY_EDITOR

    /// <summary>
    /// Unity编辑器验证方法
    /// 在Inspector中修改属性时自动调用，确保数据的有效性
    /// 
    /// 验证内容：
    /// 1. 自动生成或更新GUID（当首次创建或预制体改变时）
    /// 2. 验证门道列表的有效性
    /// 3. 验证生成点数组的有效性
    /// </summary>
    private void OnValidate()
    {
        // 如果GUID为空或预制体发生变化，重新生成GUID
        if (guid == "" || previousPrefab != prefab)
        {
            guid = GUID.Generate().ToString();
            previousPrefab = prefab;
            EditorUtility.SetDirty(this);
        }

        // 验证门道列表的有效性
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(doorwayList), doorwayList);

        // 验证生成点数组的有效性
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(spawnPositionArray), spawnPositionArray);
    }

    /// <summary>
    /// 自动生成房间边界
    /// 基于预制体中的瓦片地图组件自动计算房间的边界范围
    /// 
    /// 算法流程：
    /// 1. 检查预制体是否存在
    /// 2. 获取预制体中所有的Tilemap组件
    /// 3. 遍历所有Tilemap，计算包含所有瓦片的最小边界矩形
    /// 4. 设置lowerBounds和upperBounds
    /// 
    /// 注意：upperBounds会减去Vector2Int.one，因为边界计算采用左下角包含、右上角不包含的规则
    /// </summary>
    public void GenerateBounds()
    {
        // 检查预制体是否存在
        if (prefab == null)
        {
            Debug.LogWarning("请先填入正确的Prefab");
            return;
        }
        
        // 获取预制体中所有的瓦片地图组件
        Tilemap[] tilemaps = prefab.GetComponentsInChildren<Tilemap>();
        Vector2Int tempLowerBounds = Vector2Int.zero;
        Vector2Int tempUpperBounds = Vector2Int.zero;
        
        // 遍历所有瓦片地图，计算包含所有瓦片的边界
        foreach (var tilemap in tilemaps)
        {
            // 计算最小边界（左下角）
            tempLowerBounds = new Vector2Int(Math.Min(tempLowerBounds.x,tilemap.cellBounds.xMin), Math.Min(tempLowerBounds.y, tilemap.cellBounds.yMin));
            // 计算最大边界（右上角）
            tempUpperBounds = new Vector2Int(Math.Max(tempUpperBounds.x,tilemap.cellBounds.xMax), Math.Max(tempUpperBounds.y, tilemap.cellBounds.yMax));
        }
        
        // 设置最终边界
        lowerBounds = tempLowerBounds;
        // 由于边界计算采用左下角包含、右上角不包含的规则，因此要减去1
        upperBounds = tempUpperBounds - Vector2Int.one;
    }

#endif

    #endregion Validation
}