using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Room
{
    public string id;
    public string templateID;
    public GameObject prefab;
    public RoomNodeTypeSO roomNodeType;
    public Vector2Int lowerBounds;
    public Vector2Int upperBounds;
    public Vector2Int templateLowerBounds;
    public Vector2Int templateUpperBounds;
    public Vector2Int[] spawnPositionArray;
    public List<string> childRoomIDList;
    public string parentRoomID;
    public List<Doorway> doorwayList;
    public bool isPositioned = false;
    public InstantiatedRoom instantiatedRoom;
    public bool isLit = false;
    public bool isClearedOfEneymies = false;
    public bool isPreviouslyVisied = false;

    public Room()
    {
        childRoomIDList = new List<string>();
        doorwayList = new List<Doorway>();
    }

    public Room(RoomTemplateSO roomTemplate, RoomNodeSO roomNode)
    {
        Room room = new Room();
        room.templateID = roomTemplate.guid;
        room.id = roomNode.id;
        room.prefab = roomTemplate.prefab;
        room.roomNodeType = roomTemplate.roomNodeType;
        room.lowerBounds = roomTemplate.lowerBounds;
        room.upperBounds = roomTemplate.upperBounds;
        room.spawnPositionArray = roomTemplate.spawnPositionArray;
        room.templateLowerBounds = roomTemplate.lowerBounds;
        room.templateUpperBounds = roomTemplate.upperBounds;

        room.childRoomIDList = new List<string>(room.childRoomIDList);
        room.doorwayList = room.doorwayList.Select(doorway => doorway.DeepCopy()).ToList();

        if (roomNode.parentRoomNodeIDList.Count == 0)
        {
            room.parentRoomID = "";
            room.isPreviouslyVisied = true;
        }
        else
        {
            room.parentRoomID = roomNode.parentRoomNodeIDList[0];
        }
    }

    /// <summary>
    /// 是否和另一个房间有重合点
    /// </summary>
    public bool IsOverlapping(Room otherRoom)
    {
        bool isOverlappingX = MathUtilities.IsOverlappingInterval(lowerBounds.x, upperBounds.x, otherRoom.lowerBounds.x,
            otherRoom.upperBounds.x);

        bool isOverlappingY = MathUtilities.IsOverlappingInterval(lowerBounds.y, upperBounds.y, otherRoom.lowerBounds.y,
            otherRoom.upperBounds.y);
        
        return isOverlappingX && isOverlappingY;
    }
}