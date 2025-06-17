using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏设置类 - 包含所有游戏相关的配置常量
/// 使用静态类确保全局访问，所有设置都是编译时常量以提高性能
/// 
/// 这个类集中管理了游戏中的各种配置参数，包括：
/// - 地牢生成相关的限制和尝试次数
/// - 房间布局的约束条件
/// - 性能优化相关的参数
/// </summary>
public static class Settings
{
    #region 地牢构建设置

    /// <summary>
    /// 单个房间节点图的最大重建尝试次数
    /// 当地牢生成过程中出现房间重叠或其他问题时，系统会重新尝试构建
    /// 这个值控制了对同一个节点图的最大重试次数，防止无限循环
    /// 值越大生成成功率越高，但可能影响性能
    /// </summary>
    public const int maxDungeonRebuildAttemptsForGraph = 1000;
    
    /// <summary>
    /// 地牢构建的最大尝试次数
    /// 当一个房间节点图无法成功生成地牢时，系统会尝试其他的节点图
    /// 这个值控制了总的尝试次数，包括尝试不同的节点图
    /// 这是最外层的重试机制，确保游戏不会因为地牢生成失败而卡死
    /// </summary>
    public const int maxDungeonBuildAttempts = 10;

    #endregion
    
    #region 房间设置
    
    /// <summary>
    /// 每个房间最大子走廊数量
    /// 限制从单个房间延伸出的走廊数量，控制地牢的复杂度和分支程度
    /// 较小的值会产生更线性的地牢布局，较大的值会产生更复杂的分支结构
    /// 这个参数直接影响地牢的探索难度和视觉复杂度
    /// </summary>
    public const int maxChildCorridors = 3;

    #endregion
}
