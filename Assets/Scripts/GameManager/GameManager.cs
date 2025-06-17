using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏管理器 - 负责整个游戏的核心逻辑控制
/// 使用单例模式确保全局唯一性，实现IAutoInitializable接口支持编辑器自动初始化
/// 
/// 主要职责：
/// 1. 管理游戏状态的转换
/// 2. 控制地牢关卡的加载和生成
/// 3. 协调各个游戏系统的运行
/// 4. 处理游戏的整体流程控制
/// </summary>
[DisallowMultipleComponent]
public class GameManager : SingletonMonobehaviour<GameManager>, IAutoInitializable
{
    /// <summary>
    /// 地牢关卡配置列表 - 包含所有可用的地牢关卡数据
    /// 每个DungeonLevelSO包含该关卡的房间模板、节点图等配置信息
    /// </summary>
    [SerializeField] private List<DungeonLevelSO> dungeonLevelList;

    /// <summary>
    /// 当前地牢关卡在列表中的索引
    /// 用于确定当前要加载的地牢关卡
    /// </summary>
    [SerializeField] private int currentDungeonLevelListIndex = 0;

    /// <summary>
    /// 当前游戏状态 - 控制游戏的整体流程
    /// 在Inspector中隐藏，但可以在运行时查看和修改
    /// </summary>
    [HideInInspector] public GameState gameState;

    /// <summary>
    /// Unity生命周期 - 游戏开始时的初始化
    /// 设置初始游戏状态，开始游戏流程
    /// </summary>
    private void Start()
    {
        // 设置游戏初始状态为已开始
        gameState = GameState.gameStarted;
    }

    /// <summary>
    /// Unity生命周期 - 每帧更新游戏逻辑
    /// 持续监控和处理游戏状态的变化
    /// </summary>
    private void Update()
    {
        // 处理当前游戏状态
        HandleGameState();
    }

    /// <summary>
    /// 游戏状态处理器 - 根据当前游戏状态执行相应的逻辑
    /// 使用状态机模式管理游戏的不同阶段
    /// 
    /// 当前支持的状态：
    /// - gameStarted: 游戏刚开始，需要加载第一个地牢关卡
    /// - playingLevel: 正在游玩关卡
    /// </summary>
    private void HandleGameState()
    {
        switch (gameState)
        {
            case GameState.gameStarted:
                // 游戏开始时，加载并生成第一个地牢关卡
                PlayDungeonLevel(currentDungeonLevelListIndex);

                // 转换到游玩关卡状态
                gameState = GameState.playingLevel;
                break;
                
            // 可以在这里添加更多游戏状态的处理逻辑
            // case GameState.levelComplete:
            // case GameState.gameOver:
            // etc.
        }
    }

    /// <summary>
    /// 开始游玩指定的地牢关卡
    /// 调用地牢构建器生成地牢，并处理生成结果
    /// </summary>
    /// <param name="dungeonLevelListIndex">要加载的地牢关卡在列表中的索引</param>
    private void PlayDungeonLevel(int dungeonLevelListIndex)
    {
        // 调用地牢构建器生成指定关卡的地牢
        bool dungeonBuildSuccess = DungeonBuilder.Instance.GenerateDungeon(dungeonLevelList[dungeonLevelListIndex]);
        
        // 如果地牢生成失败，记录错误日志
        if (!dungeonBuildSuccess)
        {
            Debug.LogError("无法构造地牢");
            // 这里可以添加失败处理逻辑，比如重试或显示错误界面
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// 编辑器初始化方法 - 实现IAutoInitializable接口
    /// 在Unity编辑器中自动查找和加载地牢关卡资源
    /// 这个方法只在编辑器环境中编译和执行
    /// 
    /// 功能：
    /// 1. 自动搜索项目中的所有DungeonLevelSO资源
    /// 2. 将找到的资源自动分配给dungeonLevelList
    /// 3. 提供错误信息以便调试
    /// </summary>
    /// <param name="errorMessage">如果初始化失败，返回错误信息</param>
    /// <returns>初始化是否成功</returns>
    public bool EditorInitialize(out string errorMessage)
    {
        // 初始化错误信息
        errorMessage = "";
        
        // 使用AssetFinder查找项目中所有的DungeonLevelSO资源
        var foundAsset = AssetFinder.FindAssets<DungeonLevelSO>();
        
        // 如果没有找到任何地牢关卡资源，返回失败
        if (foundAsset == null)
        {
            errorMessage = AssetErrorMsg.NotFound<DungeonLevelSO>();
            return false;
        }

        // 将找到的资源分配给地牢关卡列表
        dungeonLevelList = foundAsset;
        return true;
    }

#endif
}