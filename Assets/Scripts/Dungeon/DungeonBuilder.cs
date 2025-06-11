using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public class DungeonBuilder : SingletonMonobehaviour<DungeonBuilder>
{
    private static readonly int Alpha = Shader.PropertyToID("_Alpha");

    public Dictionary<string, Room> dungeonBuilderRoomDictionary = new Dictionary<string, Room>();
    private Dictionary<string, RoomTemplateSO> roomTemplateDictionary = new Dictionary<string, RoomTemplateSO>();
    private List<RoomTemplateSO> roomTemplateList = null;
    private RoomNodeTypeListSO roomNodeTypeList;
    private bool dungeonBuildSuccessful;

    private void Awake()
    {
        LoadRoomNodeTypeList();

        GameResources.Instance.dimmedMaterial.SetFloat(Alpha, 1f);
    }

    private void LoadRoomNodeTypeList()
    {
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    public bool GenerateDungeon(DungeonLevelSO currentDungeonLevel)
    {
        roomTemplateList = currentDungeonLevel.roomTemplateList;

        LoadRoomTemplatesInDictionary();

        dungeonBuildSuccessful = false;

        int dungeonBuildAttempts = 0;
        while (!dungeonBuildSuccessful && dungeonBuildAttempts < Settings.maxDungeonBuildAttempts)
        {
            dungeonBuildAttempts++;

            RoomNodeGraphSO roomNodeGraph = SelectRandomRoomNodeGraph(currentDungeonLevel.roomNodeGraphList);

            int dungeonRebuildAttemptsForNodeGraph = 0;
            dungeonBuildSuccessful = false;

            while (!dungeonBuildSuccessful &&
                   dungeonRebuildAttemptsForNodeGraph <= Settings.maxDungeonRebuildAttemptsForGraph)
            {
                ClearDungeon();

                dungeonRebuildAttemptsForNodeGraph++;

                dungeonBuildSuccessful = AttemptsToBuildRandomDungeon(roomNodeGraph);

                if (dungeonBuildSuccessful)
                {
                    InstantiateRoomGameobjects();
                }
            }
        }

        return dungeonBuildSuccessful;
    }

    public void LoadRoomTemplatesInDictionary()
    {
        roomTemplateDictionary.Clear();
        roomTemplateList.ForEach(roomTemplate =>
        {
            if (!roomTemplateDictionary.TryAdd(roomTemplate.guid, roomTemplate))
            {
                Debug.Log($"房间模板已经存在: {roomTemplate.guid}");
            }
        });
    }

    private RoomTemplateSO GetRandomRoomTemplate(RoomNodeTypeSO roomNodeType)
    {
        List<RoomTemplateSO> matchingRoomTemplates = new List<RoomTemplateSO>();

        roomTemplateList.ForEach(roomTemplate =>
        {
            if (roomNodeType == roomTemplate.roomNodeType)
            {
                matchingRoomTemplates.Add(roomTemplate);
            }
        });

        if (matchingRoomTemplates.Count == 0) return null;

        return matchingRoomTemplates[Random.Range(0, matchingRoomTemplates.Count)];
    }

    private RoomNodeGraphSO SelectRandomRoomNodeGraph(List<RoomNodeGraphSO> roomNodeGraphList)
    {
        return roomNodeGraphList[Random.Range(0, roomNodeGraphList.Count)];
    }

    public RoomTemplateSO GetRoomTemplate(string roomTemplateID)
    {
        return roomTemplateDictionary.GetValueOrDefault(roomTemplateID);
    }

    public Room GetRoom(string roomID)
    {
        return dungeonBuilderRoomDictionary.GetValueOrDefault(roomID);
    }

    private void ClearDungeon()
    {
        if (dungeonBuilderRoomDictionary.Count <= 0) return;

        foreach (KeyValuePair<string, Room> keyValuePair in dungeonBuilderRoomDictionary)
        {
            Room room = keyValuePair.Value;

            if (room.instantiatedRoom != null)
            {
                Destroy(room.instantiatedRoom.gameObject);
            }
        }

        dungeonBuilderRoomDictionary.Clear();
    }

    private bool AttemptsToBuildRandomDungeon(RoomNodeGraphSO roomNodeGraph)
    {
        Queue<RoomNodeSO> openRoomNodeQueue = new Queue<RoomNodeSO>();

        RoomNodeSO entrance = roomNodeGraph.GetRoomNode(roomNodeTypeList.list.Find(type => type.isEntrance));

        if (entrance == null)
        {
            Debug.LogError("没有入口节点");
            return false;
        }

        openRoomNodeQueue.Enqueue(entrance);

        bool noRoomOverlaps = true;
        noRoomOverlaps = ProcessRoomsInOpenRoomNodeQueue(roomNodeGraph, openRoomNodeQueue, noRoomOverlaps);

        return openRoomNodeQueue.Count == 0 && noRoomOverlaps;
    }

    private bool ProcessRoomsInOpenRoomNodeQueue(RoomNodeGraphSO roomNodeGraph, Queue<RoomNodeSO> openRoomNodeQueue,
        bool noRoomOverlaps)
    {
        while (openRoomNodeQueue.Count > 0 && noRoomOverlaps)
        {
            RoomNodeSO roomNode = openRoomNodeQueue.Dequeue();

            foreach (var childRoomNode in roomNodeGraph.GetChildRoomNodes(roomNode))
            {
                openRoomNodeQueue.Enqueue(childRoomNode);
            }

            if (roomNode.roomNodeType.isEntrance)
            {
                RoomTemplateSO roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);

                Room room = new Room(roomTemplate, roomNode)
                {
                    isPositioned = true
                };

                dungeonBuilderRoomDictionary.Add(room.id, room);
            }
            else
            {
                Room parentRoom = dungeonBuilderRoomDictionary[roomNode.parentRoomNodeIDList[0]];

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
    /// 在父房间的某个门道另一侧放置房间
    /// </summary>
    /// <returns>房间是否没有重叠</returns>
    private bool PlaceTheRoom(Room parentRoom, Doorway doorwayParent, Room room)
    {
        Doorway doorway = GetOppositeDoorway(doorwayParent, room.doorwayList);

        if (doorway == null)
        {
            doorwayParent.isUnavailable = true;
            return false;
        }


        Vector2Int parentDoorWayPosition =
            parentRoom.lowerBounds + doorwayParent.position - parentRoom.templateLowerBounds;

        Vector2Int adjustment = Vector2Int.zero;
        //基于尝试连接的房间门道位置去计算，调整一格的位置偏移
        adjustment = doorway.orientation.GetReverseOrientation().NormalizePosition();
        
        room.lowerBounds = parentDoorWayPosition + adjustment + room.templateLowerBounds - doorway.position;
        room.upperBounds = room.lowerBounds + room.templateUpperBounds - room.templateLowerBounds;

        Room overlappingRoom = CheckForRoomOverlap(room);

        if (overlappingRoom == null)
        {
            doorwayParent.isConnected = true;
            doorwayParent.isUnavailable = true;
            
            doorway.isConnected = true;
            doorway.isUnavailable = true;

            return true;
        }
        else
        {
            doorwayParent.isUnavailable = false;
            return false;
        }
    }

    private Doorway GetOppositeDoorway(Doorway doorwayParent, List<Doorway> roomDoorwayList)
    {
        return roomDoorwayList.Find(doorway => doorway.orientation.IsReverseOrientation(doorwayParent.orientation)
        );
    }
    
    private Room CheckForRoomOverlap(Room roomToTest)
    {
        foreach (var item in dungeonBuilderRoomDictionary)
        {
            Room room = item.Value;

            if (room.id == roomToTest.id && !room.isPositioned) continue;

            if (room.IsOverlapping(roomToTest))
            {
                return room;
            }
        }

        return null;
    }

    /// <summary>
    /// 获取一个随机房间模板数据，用于和父节点连接
    /// </summary>
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

        return roomTemplate;
    }

    /// <summary>
    /// 迭代获取一个可用且没有被连接的门道
    /// </summary>
    /// <param name="parentRoomDoorwayList">父节点门道列表</param>
    /// <returns></returns>
    private IEnumerable<Doorway> GetUnconnectedAvailableDoorways(List<Doorway> parentRoomDoorwayList)
    {
        foreach (var doorway in parentRoomDoorwayList)
        {
            //未连接并且可用
            if (!doorway.isConnected && !doorway.isUnavailable)
            {
                yield return doorway;
            }
        }
    }

    private void InstantiateRoomGameobjects()
    {
        
    }
}