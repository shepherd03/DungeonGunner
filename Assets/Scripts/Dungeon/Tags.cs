using UnityEngine;

/// <summary>
/// 游戏标签常量类，包含Unity项目中使用的所有标签定义
/// </summary>
public static class Tags
{
    // 基础标签
    public const string Untagged = "Untagged";
    public const string Respawn = "Respawn";
    public const string Finish = "Finish";
    public const string EditorOnly = "EditorOnly";
    
    // 主要游戏对象标签
    public const string MainCamera = "MainCamera";
    public const string Player = "Player";
    public const string GameController = "GameController";
    
    // 网格和瓦片地图相关标签
    public const string Grid = "grid";
    public const string CollisionTilemap = "collisionTilemap";
    public const string MinimapTilemap = "minimapTilemap";
    public const string GroundTilemap = "groundTilemap";
    public const string Decoration1Tilemap = "decoration1Tilemap";
    public const string Decoration2Tilemap = "decoration2Tilemap";
    public const string FrontTilemap = "frontTilemap";
    public const string RoomTilemap = "roomTilemap";
    
    // 小地图和玩家相关标签
    public const string MiniMapPlayer = "miniMapPlayer";
    public const string PlayerWeapon = "playerWeapon";

    /// <summary>
    /// 获取所有定义的标签数组
    /// </summary>
    public static readonly string[] AllTags = 
    {
        Untagged, Respawn, Finish, EditorOnly,
        MainCamera, Player, GameController,
        Grid, CollisionTilemap, MinimapTilemap, GroundTilemap,
        Decoration1Tilemap, Decoration2Tilemap, FrontTilemap, RoomTilemap,
        MiniMapPlayer, PlayerWeapon
    };

    /// <summary>
    /// 检查GameObject是否具有指定标签
    /// </summary>
    public static bool HasTag(this GameObject gameObject, string tag)
    {
        return gameObject.CompareTag(tag);
    }

    /// <summary>
    /// 为GameObject设置标签（确保标签已存在）
    /// </summary>
    public static void SetTag(this GameObject gameObject, string tag)
    {
        if (System.Array.Exists(AllTags, t => t == tag))
        {
            gameObject.tag = tag;
        }
        else
        {
            Debug.LogWarning($"标签 '{tag}' 未在GameTags中定义，使用Untagged代替");
            gameObject.tag = Untagged;
        }
    }
}