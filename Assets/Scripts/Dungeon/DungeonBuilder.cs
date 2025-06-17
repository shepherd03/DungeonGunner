using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

/// <summary>
/// 地牢构建器 - 负责程序化生成地牢的核心类
/// 使用单例模式确保全局唯一性，采用基于房间节点图的算法生成随机地牢
/// 主要功能：
/// 1. 根据房间节点图生成地牢布局
/// 2. 处理房间模板的加载和匹配
/// 3. 检测房间重叠并进行位置调整
/// 4. 实例化最终的地牢游戏对象
/// </summary>
[DisallowMultipleComponent]
public class DungeonBuilder : SingletonMonobehaviour<DungeonBuilder>
{
    /// <summary>
    /// 材质透明度属性的Shader ID，用于控制房间的视觉效果
    /// </summary>
    private static readonly int Alpha = Shader.PropertyToID("_Alpha");

    /// <summary>
    /// 地牢房间字典 - 存储所有已生成的房间数据，键为房间ID
    /// 这是地牢生成过程中的核心数据结构
    /// </summary>
    public Dictionary<string, Room> dungeonBuilderRoomDictionary = new Dictionary<string, Room>();

    /// <summary>
    /// 房间模板字典 - 存储所有可用的房间模板，键为模板GUID
    /// 用于快速查找和获取特定的房间模板
    /// </summary>
    private Dictionary<string, RoomTemplateSO> roomTemplateDictionary = new Dictionary<string, RoomTemplateSO>();

    /// <summary>
    /// 当前关卡的房间模板列表
    /// </summary>
    private List<RoomTemplateSO> roomTemplateList = null;

    /// <summary>
    /// 房间节点类型列表 - 定义了所有可用的房间类型（入口、走廊、普通房间等）
    /// </summary>
    private RoomNodeTypeListSO roomNodeTypeList;

    /// <summary>
    /// 地牢构建是否成功的标志
    /// </summary>
    private bool dungeonBuildSuccessful;

    private void Awake()
    {
        // 加载房间节点类型配置
        LoadRoomNodeTypeList();

        // 设置暗化材质的透明度为完全不透明
        GameResources.Instance.dimmedMaterial.SetFloat(Alpha, 1f);
    }

    /// <summary>
    /// 从游戏资源中加载房间节点类型列表
    /// 这些类型定义了不同种类的房间（入口、走廊、普通房间等）
    /// </summary>
    private void LoadRoomNodeTypeList()
    {
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    /// <summary>
    /// 生成地牢的主要方法 - 地牢构建器的核心功能
    /// 使用多层重试机制确保地牢生成的成功率
    /// 
    /// 算法流程：
    /// 1. 加载当前关卡的房间模板
    /// 2. 外层循环：尝试不同的房间节点图（最多maxDungeonBuildAttempts次）
    /// 3. 内层循环：对选定的节点图进行多次构建尝试（最多maxDungeonRebuildAttemptsForGraph次）
    /// 4. 如果构建成功，实例化所有房间游戏对象
    /// </summary>
    /// <param name="currentDungeonLevel">当前地牢关卡的配置数据</param>
    /// <returns>地牢是否成功生成</returns>
    public bool GenerateDungeon(DungeonLevelSO currentDungeonLevel)
    {
        // 设置当前关卡的房间模板列表
        roomTemplateList = currentDungeonLevel.roomTemplateList;

        // 将房间模板加载到字典中以便快速查找
        LoadRoomTemplatesInDictionary();

        // 初始化构建状态
        dungeonBuildSuccessful = false;

        // 外层重试循环：尝试不同的房间节点图
        int dungeonBuildAttempts = 0;
        while (!dungeonBuildSuccessful && dungeonBuildAttempts < Settings.maxDungeonBuildAttempts)
        {
            dungeonBuildAttempts++;

            // 随机选择一个房间节点图作为地牢布局的基础
            RoomNodeGraphSO roomNodeGraph = SelectRandomRoomNodeGraph(currentDungeonLevel.roomNodeGraphList);

            // 内层重试循环：对选定的节点图进行多次构建尝试
            int dungeonRebuildAttemptsForNodeGraph = 0;
            dungeonBuildSuccessful = false;

            while (!dungeonBuildSuccessful &&
                   dungeonRebuildAttemptsForNodeGraph <= Settings.maxDungeonRebuildAttemptsForGraph)
            {
                // 清理之前的构建尝试
                ClearDungeon();

                dungeonRebuildAttemptsForNodeGraph++;

                // 尝试根据节点图构建地牢
                dungeonBuildSuccessful = AttemptToBuildRandomDungeon(roomNodeGraph);
            }

            // 如果构建成功，实例化所有房间的游戏对象
            if (dungeonBuildSuccessful)
            {
                InstantiateRoomGameobjects();
            }
        }

        return dungeonBuildSuccessful;
    }

    /// <summary>
    /// 将房间模板列表加载到字典中以便快速查找
    /// 使用GUID作为键值，提高查找效率
    /// </summary>
    public void LoadRoomTemplatesInDictionary()
    {
        // 清空现有字典
        roomTemplateDictionary.Clear();

        // 遍历房间模板列表，添加到字典中
        roomTemplateList.ForEach(roomTemplate =>
        {
            // 尝试添加到字典，如果GUID重复则记录日志
            if (!roomTemplateDictionary.TryAdd(roomTemplate.guid, roomTemplate))
            {
                Debug.Log($"房间模板已经存在: {roomTemplate.guid}");
            }
        });
    }

    /// <summary>
    /// 根据房间节点类型随机获取一个匹配的房间模板
    /// 用于为特定类型的房间节点选择合适的视觉表现
    /// </summary>
    /// <param name="roomNodeType">目标房间节点类型</param>
    /// <returns>随机选择的房间模板，如果没有匹配的模板则返回null</returns>
    private RoomTemplateSO GetRandomRoomTemplate(RoomNodeTypeSO roomNodeType)
    {
        // 收集所有匹配指定类型的房间模板
        List<RoomTemplateSO> matchingRoomTemplates = new List<RoomTemplateSO>();

        roomTemplateList.ForEach(roomTemplate =>
        {
            if (roomNodeType == roomTemplate.roomNodeType)
            {
                matchingRoomTemplates.Add(roomTemplate);
            }
        });

        // 如果没有匹配的模板，返回null
        if (matchingRoomTemplates.Count == 0) return null;

        // 随机选择一个匹配的模板
        return matchingRoomTemplates[Random.Range(0, matchingRoomTemplates.Count)];
    }

    /// <summary>
    /// 从房间节点图列表中随机选择一个作为地牢布局的基础
    /// 不同的节点图代表不同的地牢布局风格
    /// </summary>
    /// <param name="roomNodeGraphList">可用的房间节点图列表</param>
    /// <returns>随机选择的房间节点图</returns>
    private RoomNodeGraphSO SelectRandomRoomNodeGraph(List<RoomNodeGraphSO> roomNodeGraphList)
    {
        return roomNodeGraphList[Random.Range(0, roomNodeGraphList.Count)];
    }

    /// <summary>
    /// 根据房间模板ID获取对应的房间模板
    /// 提供对外访问房间模板的接口
    /// </summary>
    /// <param name="roomTemplateID">房间模板的唯一标识符</param>
    /// <returns>对应的房间模板，如果不存在则返回null</returns>
    public RoomTemplateSO GetRoomTemplate(string roomTemplateID)
    {
        return roomTemplateDictionary.GetValueOrDefault(roomTemplateID);
    }

    /// <summary>
    /// 根据房间ID获取对应的房间数据
    /// 提供对外访问房间数据的接口
    /// </summary>
    /// <param name="roomID">房间的唯一标识符</param>
    /// <returns>对应的房间数据，如果不存在则返回null</returns>
    public Room GetRoom(string roomID)
    {
        return dungeonBuilderRoomDictionary.GetValueOrDefault(roomID);
    }

    /// <summary>
    /// 清理当前地牢的所有数据和游戏对象
    /// 在重新生成地牢之前调用，确保没有残留的数据和对象
    /// </summary>
    private void ClearDungeon()
    {
        // 如果没有房间数据，直接返回
        if (dungeonBuilderRoomDictionary.Count <= 0) return;

        // 遍历所有房间，销毁已实例化的游戏对象
        foreach (KeyValuePair<string, Room> keyValuePair in dungeonBuilderRoomDictionary)
        {
            Room room = keyValuePair.Value;

            // 如果房间已经实例化，销毁其游戏对象
            if (room.instantiatedRoom != null)
            {
                Destroy(room.instantiatedRoom.gameObject);
            }
        }

        // 清空房间字典
        dungeonBuilderRoomDictionary.Clear();
    }

    /// <summary>
    /// 尝试根据给定的房间节点图构建随机地牢
    /// 使用广度优先搜索算法处理房间节点，从入口开始逐步构建整个地牢
    /// </summary>
    /// <param name="roomNodeGraph">用于构建地牢的房间节点图</param>
    /// <returns>是否成功构建地牢（无房间重叠且所有房间都已处理）</returns>
    private bool AttemptToBuildRandomDungeon(RoomNodeGraphSO roomNodeGraph)
    {
        // 创建开放房间节点队列，用于广度优先遍历
        Queue<RoomNodeSO> openRoomNodeQueue = new Queue<RoomNodeSO>();

        // 查找入口房间节点作为起始点
        RoomNodeSO entrance = roomNodeGraph.GetRoomNode(roomNodeTypeList.list.Find(type => type.isEntrance));

        // 如果没有找到入口节点，构建失败
        if (entrance == null)
        {
            Debug.LogError("没有入口节点");
            return false;
        }

        // 将入口节点加入队列开始处理
        openRoomNodeQueue.Enqueue(entrance);

        // 初始化房间重叠检测标志
        bool noRoomOverlaps = true;

        // 处理队列中的所有房间节点
        noRoomOverlaps = ProcessRoomsInOpenRoomNodeQueue(roomNodeGraph, openRoomNodeQueue, noRoomOverlaps);

        // 成功条件：队列为空（所有房间都已处理）且没有房间重叠
        return openRoomNodeQueue.Count == 0 && noRoomOverlaps;
    }

    /// <summary>
    /// 处理开放房间节点队列中的所有房间
    /// 使用广度优先搜索算法，逐个处理房间节点并尝试放置房间
    /// 
    /// 处理流程：
    /// 1. 从队列中取出房间节点
    /// 2. 将其子节点加入队列
    /// 3. 如果是入口房间，直接创建并定位
    /// 4. 如果是普通房间，尝试与父房间连接并检测重叠
    /// </summary>
    /// <param name="roomNodeGraph">房间节点图</param>
    /// <param name="openRoomNodeQueue">待处理的房间节点队列</param>
    /// <param name="noRoomOverlaps">当前是否没有房间重叠</param>
    /// <returns>处理完成后是否仍然没有房间重叠</returns>
    private bool ProcessRoomsInOpenRoomNodeQueue(RoomNodeGraphSO roomNodeGraph, Queue<RoomNodeSO> openRoomNodeQueue,
        bool noRoomOverlaps)
    {
        // 持续处理队列中的房间节点，直到队列为空或出现重叠
        while (openRoomNodeQueue.Count > 0 && noRoomOverlaps)
        {
            // 从队列中取出下一个要处理的房间节点
            RoomNodeSO roomNode = openRoomNodeQueue.Dequeue();

            // 将当前房间节点的所有子节点加入队列（广度优先遍历）
            foreach (var childRoomNode in roomNodeGraph.GetChildRoomNodes(roomNode))
            {
                openRoomNodeQueue.Enqueue(childRoomNode);
            }

            // 处理入口房间：直接创建并设置为已定位
            if (roomNode.roomNodeType.isEntrance)
            {
                // 为入口房间选择随机模板
                RoomTemplateSO roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);

                // 创建入口房间并标记为已定位（入口房间位置固定）
                Room room = new Room(roomTemplate, roomNode)
                {
                    isPositioned = true
                };

                // 将房间添加到地牢字典中
                dungeonBuilderRoomDictionary.Add(room.id, room);
            }
            // 处理普通房间：需要与父房间连接
            else
            {
                // 获取父房间（每个非入口房间都有父房间）
                Room parentRoom = dungeonBuilderRoomDictionary[roomNode.parentRoomNodeIDList[0]];

                // 尝试放置房间，确保没有重叠
                noRoomOverlaps = CanPlaceRoomWithNoOverlaps(roomNode, parentRoom);
            }
        }

        return noRoomOverlaps;
    }

    /// <summary>
    /// 房间能否在没有重叠的情况下被放置
    /// </summary>
    /// <param name="roomNode">要放置的节点</param>
    /// <param name="parentRoom">父节点数据</param>
    private bool CanPlaceRoomWithNoOverlaps(RoomNodeSO roomNode, Room parentRoom)
    {
        bool roomOverlaps = true;

        while (roomOverlaps)
        {
            //没有连接并且可用的父节点
            List<Doorway> unconnectedAvailableParentDoorways =
                GetUnconnectedAvailableDoorways(parentRoom.doorwayList).ToList();

            if (unconnectedAvailableParentDoorways.Count == 0)
            {
                return false;
            }

            Doorway doorwayParent =
                unconnectedAvailableParentDoorways[Random.Range(0, unconnectedAvailableParentDoorways.Count)];

            RoomTemplateSO roomTemplate = GetRandomRoomTemplateForRoomConsistentWithParent(roomNode, doorwayParent);

            Room room = new Room(roomTemplate, roomNode);

            if (PlaceTheRoom(parentRoom, doorwayParent, room))
            {
                roomOverlaps = false;
                room.isPositioned = true;

                dungeonBuilderRoomDictionary.Add(room.id, room);
            }
            else
            {
                roomOverlaps = true;
            }
        }

        return true;
    }

    /// <summary>
    /// 在父房间的指定门道旁放置新房间
    /// 这是地牢生成中最核心的房间定位算法，负责精确计算房间位置
    /// 
    /// 算法步骤：
    /// 1. 找到新房间中与父房间门道方向相反的门道
    /// 2. 计算父房间门道在世界坐标中的位置
    /// 3. 根据门道方向计算位置调整偏移
    /// 4. 计算新房间的边界位置
    /// 5. 检测是否与现有房间重叠
    /// 6. 如果无重叠，标记门道为已连接；否则标记为不可用
    /// </summary>
    /// <param name="parentRoom">父房间数据</param>
    /// <param name="doorwayParent">父房间中要连接的门道</param>
    /// <param name="room">要放置的新房间</param>
    /// <returns>房间是否成功放置（无重叠）</returns>
    private bool PlaceTheRoom(Room parentRoom, Doorway doorwayParent, Room room)
    {
        // 在新房间中找到与父房间门道方向相反的门道
        Doorway doorway = GetOppositeDoorway(doorwayParent, room.doorwayList);

        // 如果找不到匹配的门道，标记父门道为不可用
        if (doorway == null)
        {
            doorwayParent.isUnavailable = true;
            return false;
        }

        // 计算父房间门道在世界坐标系中的实际位置
        Vector2Int parentDoorWayPosition =
            parentRoom.lowerBounds + doorwayParent.position - parentRoom.templateLowerBounds;

        // 根据门道方向计算位置调整偏移
        // 这确保两个门道能够正确对接
        Vector2Int adjustment = Vector2Int.zero;
        adjustment = doorway.orientation.GetReverseOrientation().NormalizePosition();

        // 计算新房间的边界位置
        // 基于父门道位置、调整偏移和新房间门道的相对位置
        room.lowerBounds = parentDoorWayPosition + adjustment + room.templateLowerBounds - doorway.position;
        room.upperBounds = room.lowerBounds + room.templateUpperBounds - room.templateLowerBounds;

        // 检测新房间是否与现有房间重叠
        Room overlappingRoom = CheckForRoomOverlap(room);

        // 如果没有重叠，成功放置房间
        if (overlappingRoom == null)
        {
            // 标记两个门道为已连接和不可用
            doorwayParent.isConnected = true;
            doorwayParent.isUnavailable = true;

            doorway.isConnected = true;
            doorway.isUnavailable = true;

            return true;
        }
        // 如果有重叠，放置失败
        else
        {
            doorwayParent.isUnavailable = true;
            return false;
        }
    }

    /// <summary>
    /// 获取与指定门道方向相反的门道
    /// 用于在房间连接时找到匹配的门道对
    /// 
    /// 例如：如果父门道朝向北方，则需要找到朝向南方的门道来连接
    /// </summary>
    /// <param name="doorwayParent">父房间的门道</param>
    /// <param name="roomDoorwayList">要搜索的房间门道列表</param>
    /// <returns>方向相反的门道，如果找不到则返回null</returns>
    private Doorway GetOppositeDoorway(Doorway doorwayParent, List<Doorway> roomDoorwayList)
    {
        return roomDoorwayList.Find(doorway => doorway.orientation.IsReverseOrientation(doorwayParent.orientation)
        );
    }

    /// <summary>
    /// 检测指定房间是否与现有房间重叠
    /// 遍历所有已定位的房间，检查边界是否有交集
    /// 
    /// 重叠检测规则：
    /// - 跳过自身房间的检测
    /// - 只检测已定位的房间
    /// - 使用房间的边界矩形进行碰撞检测
    /// </summary>
    /// <param name="roomToTest">要检测的房间</param>
    /// <returns>与之重叠的房间，如果没有重叠则返回null</returns>
    private Room CheckForRoomOverlap(Room roomToTest)
    {
        foreach (var item in dungeonBuilderRoomDictionary)
        {
            Room room = item.Value;

            // 跳过自身房间或未定位的房间
            if (room.id == roomToTest.id || !room.isPositioned) continue;

            // 检测边界重叠
            if (room.IsOverlapping(roomToTest))
            {
                return room;
            }
        }

        return null;
    }

    /// <summary>
    /// 获取与父房间门道配置一致的随机房间模板
    /// 确保子房间具有与父房间相匹配的门道方向，以便能够正确连接
    /// 
    /// 匹配规则：
    /// - 如果是走廊类型，根据父门道方向选择南北向或东西向走廊
    /// - 如果是普通房间，直接根据房间节点类型选择模板
    /// 
    /// 这确保了房间之间能够通过门道正确连接
    /// </summary>
    /// <param name="roomNode">房间节点，定义房间类型</param>
    /// <param name="doorwayParent">父房间的门道，用于确定连接方向</param>
    /// <returns>匹配的房间模板，如果找不到则返回null</returns>
    private RoomTemplateSO GetRandomRoomTemplateForRoomConsistentWithParent(RoomNodeSO roomNode, Doorway doorwayParent)
    {
        RoomTemplateSO roomTemplate = null;
        if (roomNode.roomNodeType.isCorridor)
        {
            switch (doorwayParent.orientation)
            {
                case Orientation.north:
                case Orientation.south:
                    roomTemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(type => type.isCorridorNS));
                    break;
                case Orientation.east:
                case Orientation.west:
                    roomTemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(type => type.isCorridorEW));
                    break;
            }
        }
        else
        {
            roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);
        }

        if (roomTemplate == null)
        {
            Debug.LogWarning($"GetRandomRoomTemplateForRoomConsistentWithParent中获取到的roomTemplate为空");
        }

        return roomTemplate;
    }

    /// <summary>
    /// 获取房间中所有未连接且可用的门道列表
    /// 用于确定房间还有哪些门道可以用来连接其他房间
    /// 
    /// 过滤条件：
    /// - 门道未连接（isConnected = false）
    /// - 门道可用（isUnavailable = false）
    /// </summary>
    /// <param name="roomDoorwayList">房间的门道列表</param>
    /// <returns>可用于连接的门道列表</returns>
    private IEnumerable<Doorway> GetUnconnectedAvailableDoorways(List<Doorway> roomDoorwayList)
    {
        foreach (Doorway doorway in roomDoorwayList)
        {
            if (!doorway.isConnected && !doorway.isUnavailable)
                yield return doorway;
        }
    }

    /// <summary>
    /// 实例化所有房间的游戏对象
    /// 在地牢生成完成后，将抽象的房间数据转换为实际的游戏对象
    /// 
    /// 实例化过程：
    /// 1. 遍历所有房间数据
    /// 2. 计算房间在世界坐标中的位置
    /// 3. 实例化房间预制体
    /// 4. 初始化InstantiatedRoom组件
    /// 5. 建立房间数据与游戏对象的双向引用
    /// </summary>
    private void InstantiateRoomGameobjects()
    {
        foreach (KeyValuePair<string, Room> keyvaluepair in dungeonBuilderRoomDictionary)
        {
            Room room = keyvaluepair.Value;

            // 计算房间在世界坐标中的实际位置
            // 基于房间边界和模板边界的差值
            Vector3 roomPosition = new Vector3(room.lowerBounds.x - room.templateLowerBounds.x,
                room.lowerBounds.y - room.templateLowerBounds.y, 0f);

            // 实例化房间预制体
            GameObject roomGameobject = Instantiate(room.prefab, roomPosition, Quaternion.identity, transform);

            // 获取InstantiatedRoom组件
            InstantiatedRoom instantiatedRoom = roomGameobject.GetComponentInChildren<InstantiatedRoom>();

            // 建立房间数据与实例化房间的关联
            instantiatedRoom.room = room;
            instantiatedRoom.Initialise(roomGameobject);
            room.instantiatedRoom = instantiatedRoom;
        }
    }
}