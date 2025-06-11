using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class GameManager : SingletonMonobehaviour<GameManager>, IAutoInitializable
{
    [SerializeField] private List<DungeonLevelSO> dungeonLevelList;

    [SerializeField] private int currentDungeonLevelListIndex = 0;

    [HideInInspector] public GameState gameState;

    private void Start()
    {
        gameState = GameState.gameStarted;
    }

    private void Update()
    {
        HandleGameState();
    }

    private void HandleGameState()
    {
        switch (gameState)
        {
            case GameState.gameStarted:
                PlayDungeonLevel(currentDungeonLevelListIndex);

                gameState = GameState.playingLevel;
                break;
        }
    }

    private void PlayDungeonLevel(int dungeonLevelListIndex)
    {
        bool dungeonBuildSuccess = DungeonBuilder.Instance.GenerateDungeon(dungeonLevelList[dungeonLevelListIndex]);
        if (!dungeonBuildSuccess)
        {
            Debug.LogError("无法构造地牢");
        }
    }

#if UNITY_EDITOR
    public bool EditorInitialize(out string errorMessage)
    {
        errorMessage = "";
        var foundAsset = AssetFinder.FindAssets<DungeonLevelSO>();
        if (foundAsset == null)
        {
            errorMessage = AssetErrorMsg.NotFound<DungeonLevelSO>();
            return false;
        }

        dungeonLevelList = foundAsset;
        return true;
    }

#endif
}