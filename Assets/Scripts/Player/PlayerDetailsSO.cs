using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "PlayerDetails_", menuName = "Scriptable Objects/Player/Player Details")]
public class PlayerDetailsSO : ScriptableObject
{
    /// <summary>
    /// 游戏角色名字
    /// </summary>
    public string characterName;
    public GameObject playerPrefab;
    public RuntimeAnimatorController runtimeAnimatorController;
    public int playerHealth;
    public Sprite playerIcon;
    public Sprite playerHandSprite;

    #region 验证器

    #if UNITY_EDITOR

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this,nameof(characterName),characterName);
        HelperUtilities.ValidateCheckNullValue(this,nameof(playerPrefab),playerPrefab);
        HelperUtilities.ValidateCheckPositiveValue(this,nameof(playerHealth),playerHealth,false);
        HelperUtilities.ValidateCheckNullValue(this,nameof(playerIcon),playerIcon);
        HelperUtilities.ValidateCheckNullValue(this,nameof(playerHandSprite),playerHandSprite);
        HelperUtilities.ValidateCheckNullValue(this,nameof(runtimeAnimatorController),runtimeAnimatorController);
    }
    
    #endif

    #endregion
}
