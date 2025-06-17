using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 房间数据类
/// 存储地牢中单个房间的所有相关信息，包括位置、连接关系、状态等
/// 这是地牢生成系统的核心数据结构之一
/// </summary>
public class Room
{
    /// <summary>房间的唯一标识符</summary>
    public string id;
    
    /// <summary>房间模板的GUID，用于关联RoomTemplateSO</summary>
    public string templateID;
    
    /// <summary>房间的预制体，用于实例化游戏对象</summary>
    public GameObject prefab;
    
    /// <summary>房间节点类型（入口、走廊、普通房间、Boss房间等）</summary>
    public RoomNodeTypeSO roomNodeType;
    
    /// <summary>房间在世界坐标中的左下角边界</summary>
    public Vector2Int lowerBounds;
    
    /// <summary>房间在世界坐标中的右上角边界</summary>
    public Vector2Int upperBounds;
    
    /// <summary>房间模板的原始左下角边界（用于位置计算）</summary>
    public Vector2Int templateLowerBounds;
    
    /// <summary>房间模板的原始右上角边界（用于位置计算）</summary>
    public Vector2Int templateUpperBounds;
    
    /// <summary>房间内的生成点位置数组（用于放置敌人、道具等）</summary>
    public Vector2Int[] spawnPositionArray;
    
    /// <summary>子房间ID列表（连接到此房间的其他房间）</summary>
    public List<string> childRoomIDList;
    
    /// <summary>父房间ID（此房间连接的上级房间）</summary>
    public string parentRoomID;
    
    /// <summary>房间的门道列表（用于连接其他房间）</summary>
    public List<Doorway> doorwayList;
    
    /// <summary>房间是否已被定位（在地牢生成过程中使用）</summary>
    public bool isPositioned = false;
    
    /// <summary>实例化的房间组件引用</summary>
    public InstantiatedRoom instantiatedRoom;
    
    /// <summary>房间是否已被照亮</summary>
    public bool isLit = false;
    
    /// <summary>房间是否已清除所有敌人</summary>
    public bool isClearedOfEneymies = false;
    
    /// <summary>房间是否曾被访问过</summary>
    public bool isPreviouslyVisied = false;

    /// <summary>
    /// 默认构造函数
    /// 初始化房间的基本集合属性
    /// </summary>
    public Room()
    {
        childRoomIDList = new List<string>();
        doorwayList = new List<Doorway>();
    }

    /// <summary>
    /// 基于房间模板和房间节点创建房间
    /// 这是主要的构造函数，用于在地牢生成过程中创建房间实例
    /// 
    /// 初始化过程：
    /// 1. 从房间模板复制基础属性（预制体、边界、生成点等）
    /// 2. 从房间节点获取ID和父子关系
    /// 3. 深拷贝门道列表以避免引用共享
    /// 4. 设置访问状态（入口房间默认已访问）
    /// </summary>
    /// <param name="roomTemplate">房间模板，定义房间的物理属性</param>
    /// <param name="roomNode">房间节点，定义房间在图中的逻辑关系</param>
    public Room(RoomTemplateSO roomTemplate, RoomNodeSO roomNode):this()
    {
        // 从模板复制基础属性
        templateID = roomTemplate.guid;
        id = roomNode.id;
        prefab = roomTemplate.prefab;
        roomNodeType = roomTemplate.roomNodeType;
        lowerBounds = roomTemplate.lowerBounds;
        upperBounds = roomTemplate.upperBounds;
        spawnPositionArray = roomTemplate.spawnPositionArray;
        templateLowerBounds = roomTemplate.lowerBounds;
        templateUpperBounds = roomTemplate.upperBounds;

        // 初始化集合
        childRoomIDList = new List<string>(childRoomIDList);
        // 深拷贝门道列表，避免多个房间共享同一门道引用
        doorwayList = roomTemplate.GetDoorwayList.Select(doorway => doorway.DeepCopy()).ToList();

        // 设置父子关系和访问状态
        if (roomNode.parentRoomNodeIDList.Count == 0)
        {
            // 入口房间没有父房间，默认已访问
            parentRoomID = "";
            isPreviouslyVisied = true;
        }
        else
        {
            // 设置父房间ID（取第一个父节点）
            parentRoomID = roomNode.parentRoomNodeIDList[0];
        }
    }

    /// <summary>
    /// 检测当前房间是否与另一个房间重叠
    /// 使用轴对齐包围盒（AABB）碰撞检测算法
    /// 
    /// 重叠条件：
    /// - X轴方向有重叠 AND Y轴方向有重叠
    /// - 使用房间的lowerBounds和upperBounds进行检测
    /// 
    /// 这是地牢生成中防止房间重叠的关键方法
    /// </summary>
    /// <param name="otherRoom">要检测的另一个房间</param>
    /// <returns>如果两个房间重叠返回true，否则返回false</returns>
    public bool IsOverlapping(Room otherRoom)
    {
        // 检测X轴方向是否重叠
        bool isOverlappingX = MathUtilities.IsOverlappingInterval(lowerBounds.x, upperBounds.x, otherRoom.lowerBounds.x,
            otherRoom.upperBounds.x);

        // 检测Y轴方向是否重叠
        bool isOverlappingY = MathUtilities.IsOverlappingInterval(lowerBounds.y, upperBounds.y, otherRoom.lowerBounds.y,
            otherRoom.upperBounds.y);
        
        // 只有当X和Y轴都重叠时，房间才真正重叠
        return isOverlappingX && isOverlappingY;
    }
}