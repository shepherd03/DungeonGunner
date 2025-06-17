using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class DungeonTileMapGroup
{
    [HideInInspector]public Tilemap groundTilemap;
    [HideInInspector]public Tilemap decorate1Tilemap;
    [HideInInspector]public Tilemap decorate2Tilemap;
    [HideInInspector]public Tilemap frontTilemap;
    [HideInInspector]public Tilemap collisionTilemap;
    [HideInInspector]public Tilemap minimapTilemap;

    public DungeonTileMapGroup(Tilemap groundTilemap, Tilemap decorate1Tilemap, Tilemap decorate2Tilemap, Tilemap frontTilemap, Tilemap collisionTilemap, Tilemap minimapTilemap)
    {
        this.groundTilemap = groundTilemap;
        this.decorate1Tilemap = decorate1Tilemap;
        this.decorate2Tilemap = decorate2Tilemap;
        this.frontTilemap = frontTilemap;
        this.collisionTilemap = collisionTilemap;
        this.minimapTilemap = minimapTilemap;
    }

    public DungeonTileMapGroup(Grid grid)
    {
        Tilemap[] tilemaps = grid.GetComponentsInChildren<Tilemap>();
        foreach (Tilemap tilemap in tilemaps)
        {
            if (tilemap.gameObject.HasTag(Tags.GroundTilemap))
            {
                groundTilemap = tilemap;
            }
            else if (tilemap.gameObject.HasTag(Tags.Decoration1Tilemap))
            {
                decorate1Tilemap = tilemap;
            }
            else if (tilemap.gameObject.HasTag(Tags.Decoration2Tilemap))
            {
                decorate2Tilemap = tilemap;
            }
            else if (tilemap.gameObject.HasTag(Tags.FrontTilemap))
            {
                frontTilemap = tilemap;
            }
            else if (tilemap.gameObject.HasTag(Tags.CollisionTilemap))
            {
                collisionTilemap = tilemap;
            }
            else if (tilemap.gameObject.HasTag(Tags.MinimapTilemap))
            {
                minimapTilemap = tilemap;
            }
        }
    }

    public void DisableCollisionTilemapRender()
    {
        collisionTilemap.gameObject.GetComponent<TilemapRenderer>().enabled = false;
    }
}