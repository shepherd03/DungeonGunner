using System.Collections.Generic;
using UnityEngine;

public class GameResources : MonoBehaviour, IAutoInitializable
{
    private static GameResources instance;

    public static GameResources Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<GameResources>("GameResources");
            }

            return instance;
        }
    }

    public RoomNodeTypeListSO roomNodeTypeList;

    public Material dimmedMaterial;

#if UNITY_EDITOR

    public bool EditorInitialize(out string errorMessage)
    {
        errorMessage = "";
        List<RoomNodeTypeListSO> foundRoomNodeTypeList = AssetFinder.FindAssets<RoomNodeTypeListSO>();
        if (foundRoomNodeTypeList == null)
        {
            errorMessage = AssetErrorMsg.NotFound<RoomNodeTypeListSO>();
            return false;
        }

        if (foundRoomNodeTypeList.Count > 1)
        {
            errorMessage = AssetErrorMsg.NotSingle<RoomNodeTypeListSO>();
            return false;
        }
        
        roomNodeTypeList = foundRoomNodeTypeList[0];
        return true;
    }

#endif
}