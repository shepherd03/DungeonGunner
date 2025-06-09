using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Room_", menuName = "Scriptable Objects/Dungeon/Room")]
public class RoomTemplateSO : ScriptableObject
{
    [HideInInspector] public string guid;

    [Space(10)]
    [Header("房间预制体")]
    public GameObject prefab;

    [HideInInspector] public GameObject previousPrefab; // this is used to regenerate the guid if the so is copied and the prefab is changed

    [Space(10)]
    [Header("配置")]

    public RoomNodeTypeSO roomNodeType;

    [Header("左下角边界")]
    public Vector2Int lowerBounds;

    [Header("右上角边界")]
    public Vector2Int upperBounds;

    [Header("门")]
    [Tooltip("每一个门对应一项")]
    [SerializeField] private List<Doorway> doorwayList;

    #region Tooltip

    [Tooltip("Each possible spawn position (used for enemies and chests) for the room in tilemap coordinates should be added to this array")]

    #endregion Tooltip

    public Vector2Int[] spawnPositionArray;

    public List<Doorway> GetDoorwayList => doorwayList;

    #region 验证器与工具

#if UNITY_EDITOR

    private void OnValidate()
    {
        if (guid == "" || previousPrefab != prefab)
        {
            guid = GUID.Generate().ToString();
            previousPrefab = prefab;
            EditorUtility.SetDirty(this);
        }

        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(doorwayList), doorwayList);

        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(spawnPositionArray), spawnPositionArray);
    }

    public void GenerateBounds()
    {
        if (prefab == null)
        {
            Debug.LogWarning("请先填入正确的Prefab");
            return;
        }
        
        Tilemap[] tilemaps = prefab.GetComponentsInChildren<Tilemap>();
        Vector2Int tempLowerBounds = Vector2Int.zero;
        Vector2Int tempUpperBounds = Vector2Int.zero;
        foreach (var tilemap in tilemaps)
        {
            tempLowerBounds = new Vector2Int(Math.Min(tempLowerBounds.x,tilemap.cellBounds.xMin), Math.Min(tempLowerBounds.y, tilemap.cellBounds.yMin));
            tempUpperBounds = new Vector2Int(Math.Max(tempUpperBounds.x,tilemap.cellBounds.xMax), Math.Max(tempUpperBounds.y, tilemap.cellBounds.yMax));
        }
        lowerBounds = tempLowerBounds;
        //由于是Vector2Int算是左下角的，因此要减去1
        upperBounds = tempUpperBounds - Vector2Int.one;
    }

#endif

    #endregion Validation
}