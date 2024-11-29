using UnityEngine;
using RPGCharacterAnims.Lookups;

/// <summary>
/// ScriptableObject containing configuration data for weapons
/// </summary>
[CreateAssetMenu(fileName = "New Weapon Data", menuName = "Combat/Weapon Data")]
public class WeaponDataSO : ScriptableObject
{
    #region Weapon Configuration
    [Header("Basic Settings")]
    [Tooltip("Type of weapon this data represents")]
    public Weapon weaponType;

    [Tooltip("Prefab model for this weapon")]
    public GameObject prefab;

    #endregion

    #region Validation
    private void OnValidate()
    {
        ValidateWeaponData();
    }

    private void ValidateWeaponData()
    {
        if (prefab == null)
        {
            Debug.LogWarning($"[WeaponData] Weapon prefab is missing for {name}");
        }
    }
    #endregion
}

