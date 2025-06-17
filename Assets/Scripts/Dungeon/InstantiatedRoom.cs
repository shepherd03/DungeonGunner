using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(BoxCollider2D))]
public class InstantiatedRoom : MonoBehaviour
{
    [HideInInspector] public Room room;
    [HideInInspector] public Grid grid;
    public DungeonTileMapGroup tilemapGroup;
    [HideInInspector] public Bounds roomColliderBounds;

    private BoxCollider2D collider;
    private void Awake()
    {
        collider = GetComponent<BoxCollider2D>();
        roomColliderBounds = collider.bounds;
    }

    private void BlockOffUnusedDoorways()
    {
        room.doorwayList.ForEach(doorway =>
        {
            if (doorway.isConnected) return;
            
            BlockDoorwayOnTilemapLayer(tilemapGroup.collisionTilemap,doorway);
            BlockDoorwayOnTilemapLayer(tilemapGroup.minimapTilemap,doorway);
            BlockDoorwayOnTilemapLayer(tilemapGroup.groundTilemap,doorway);
            BlockDoorwayOnTilemapLayer(tilemapGroup.decorate1Tilemap,doorway);
            BlockDoorwayOnTilemapLayer(tilemapGroup.decorate2Tilemap,doorway);
            BlockDoorwayOnTilemapLayer(tilemapGroup.frontTilemap,doorway);
        });
    }

    private void BlockDoorwayOnTilemapLayer(Tilemap tilemap, Doorway doorway)
    {
        switch (doorway.orientation)
        {
            case Orientation.north:
            case Orientation.south:
                BlockDoorwayOnHorizontally(tilemap, doorway);
                break;
            case Orientation.east:
                case Orientation.west:
                BlockDoorwayVertically(tilemap, doorway);
                break;
        }
    }
    
    private void BlockDoorwayOnHorizontally(Tilemap tilemap, Doorway doorway)
    {
        Vector2Int startPos = doorway.doorwayStartCopyPosition;

        for (int xPos = 0; xPos < doorway.doorwayCopyTileWidth; xPos++)
        {
            for (int yPos = 0; yPos < doorway.doorwayCopyTileHeight; yPos++)
            {
                Matrix4x4 transformMatrix =
                    tilemap.GetTransformMatrix(new Vector3Int(startPos.x + xPos, startPos.y - yPos));

                tilemap.SetTile(new Vector3Int(startPos.x + 1 + xPos, startPos.y - yPos),
                    tilemap.GetTile(new Vector3Int(startPos.x + xPos, startPos.y - yPos)));

                tilemap.SetTransformMatrix(new Vector3Int(startPos.x + 1 + xPos, startPos.y - yPos), transformMatrix);
            }
        }
    }

    private void BlockDoorwayVertically(Tilemap tilemap, Doorway doorway)
    {
        Vector2Int startPos = doorway.doorwayStartCopyPosition;

        for (int yPos = 0; yPos < doorway.doorwayCopyTileHeight; yPos++)
        {
            for (int xPos = 0; xPos < doorway.doorwayCopyTileWidth; xPos++)
            {
                Matrix4x4 transformMatrix =
                    tilemap.GetTransformMatrix(new Vector3Int(startPos.x + xPos, startPos.y - yPos));

                tilemap.SetTile(new Vector3Int(startPos.x + xPos, startPos.y - 1 - yPos),
                    tilemap.GetTile(new Vector3Int(startPos.x + xPos, startPos.y - yPos)));

                tilemap.SetTransformMatrix(new Vector3Int(startPos.x + xPos, startPos.y - 1 - yPos), transformMatrix);
            }
        }
    }



    /// <summary>
    /// 在给定物体下获取组件进行初始化
    /// </summary>
    public void Initialise(GameObject roomGameObject)
    {
        grid = roomGameObject.GetComponentInChildren<Grid>();
        tilemapGroup = new DungeonTileMapGroup(grid);
        BlockOffUnusedDoorways();
        tilemapGroup.DisableCollisionTilemapRender();
    }
}