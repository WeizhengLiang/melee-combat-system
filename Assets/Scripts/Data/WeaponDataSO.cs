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

    [Header("Combat Properties")]
    [Tooltip("Radius of attack detection")]
    [Range(0.1f, 2.0f)]
    public float attackRadius = 0.5f;

    [Tooltip("Base damage multiplier for this weapon")]
    [Range(0.1f, 3.0f)]
    public float damageMultiplier = 1.0f;

    [Tooltip("Attack speed modifier")]
    [Range(0.5f, 2.0f)]
    public float attackSpeedModifier = 1.0f;
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

