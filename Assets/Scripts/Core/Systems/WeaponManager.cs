using UnityEngine;
using System.Collections.Generic;
using RPGCharacterAnims;
using RPGCharacterAnims.Extensions;
using RPGCharacterAnims.Lookups;

/// <summary>
/// Manages weapon systems including:
/// - Weapon data and configurations
/// - Weapon attachment points
/// - Attack points for hit detection
/// - Weapon equipping and unequipping
/// </summary>
public class WeaponManager : MonoBehaviour
{
    #region Data Structures
    [System.Serializable]
    public class WeaponData
    {
        [Header("Weapon Configuration")]
        public string name;
        public Weapon weaponType;
        public GameObject weaponInstance;
        
        [Header("Attack Configuration")]
        public List<Transform> attackPoints;
        public Transform attachPoint;
        public float attackRadius = 0.5f;
    }
    #endregion

    #region Fields
    [Header("Weapon Setup")]
    [Tooltip("List of available weapons and their configurations")]
    public List<WeaponData> availableWeapons = new();

    [Header("Unarmed Configuration")]
    [Tooltip("Configuration for unarmed combat attack points")]
    public UnarmedAttackPointsConfig unarmedConfig;

    private Dictionary<Weapon, WeaponData> weaponDataDict = new();
    private RPGCharacterController characterController;

    // Public read-only access to weapon data
    public Dictionary<Weapon, WeaponData> WeaponDataDict => weaponDataDict;
    #endregion

    #region Initialization
    private void Awake()
    {
        InitializeComponents();
        InitializeWeaponData();
    }

    private void InitializeComponents()
    {
        characterController = GetComponent<RPGCharacterController>();
        if (characterController == null)
        {
            Debug.LogError("[WeaponManager] RPGCharacterController not found");
            enabled = false;
        }
    }

    private void InitializeWeaponData()
    {
        SetupWeaponsAttackPoints();
    }
    #endregion

    #region Weapon Setup
    private void SetupWeaponsAttackPoints()
    {
        foreach (var weaponData in availableWeapons)
        {
            if (weaponDataDict.ContainsKey(weaponData.weaponType))
            {
                Debug.LogWarning($"[WeaponManager] Duplicate weapon type found: {weaponData.weaponType}. Ignoring duplicate.");
                continue;
            }

            InitializeWeaponAttackPoints(weaponData);
            weaponDataDict[weaponData.weaponType] = weaponData;
        }
    }

    private void InitializeWeaponAttackPoints(WeaponData weaponData)
    {
        weaponData.attackPoints = new List<Transform>();
        
        if (weaponData.weaponType == Weapon.Unarmed)
        {
            SetupUnarmedAttackPoints(weaponData);
        }
        else
        {
            SetupWeaponAttackPoints(weaponData);
        }
    }

    private void SetupWeaponAttackPoints(WeaponData weaponData)
    {
        foreach (Transform child in weaponData.weaponInstance.transform)
        {
            if (child.CompareTag("AttackPoint"))
            {
                weaponData.attackPoints.Add(child);
            }
            else if (child.CompareTag("AttachPoint"))
            {
                weaponData.attachPoint = child;
            }
        }
    }

    private void SetupUnarmedAttackPoints(WeaponData weaponData)
    {
        if (unarmedConfig == null)
        {
            Debug.LogWarning("[WeaponManager] UnarmedConfig is missing, unarmed attack points setup failed");
            return;
        }

        foreach (var pointConfig in unarmedConfig.attackPoints)
        {
            CreateUnarmedAttackPoint(weaponData, pointConfig);
        }
    }

    private void CreateUnarmedAttackPoint(WeaponData weaponData, UnarmedAttackPointsConfig.AttackPointConfig pointConfig)
    {
        Transform bone = Utility.FindDeepChild(transform, pointConfig.boneName);
        if (bone == null)
        {
            Debug.LogWarning($"[WeaponManager] Bone {pointConfig.boneName} not found for unarmed attack point {pointConfig.name}");
            return;
        }

        Transform attackPoint = GetOrCreateAttackPoint(bone, pointConfig);
        weaponData.attackPoints.Add(attackPoint);
    }

    private Transform GetOrCreateAttackPoint(Transform bone, UnarmedAttackPointsConfig.AttackPointConfig config)
    {
        Transform existingPoint = bone.Find(config.name);
        if (existingPoint != null) return existingPoint;

        GameObject attackPointObj = new GameObject(config.name);
        attackPointObj.transform.SetParent(bone);
        attackPointObj.transform.localPosition = config.localPosition;
        attackPointObj.tag = "AttackPoint";
        
        return attackPointObj.transform;
    }
    #endregion

    #region Weapon Access
    /// <summary>
    /// Retrieves weapon prefab for the specified weapon type
    /// </summary>
    public GameObject GetWeaponPrefab(Weapon weaponType)
    {
        if (weaponDataDict.TryGetValue(weaponType, out WeaponData weaponData))
        {
            return weaponData.weaponInstance;
        }
        Debug.LogWarning($"[WeaponManager] Weapon prefab for {weaponType} not found");
        return null;
    }

    /// <summary>
    /// Gets attack points for the specified weapon type
    /// </summary>
    public List<Transform> GetAttackPoints(Weapon weaponType)
    {
        if (weaponDataDict.TryGetValue(weaponType, out WeaponData weaponData))
        {
            return weaponData.attackPoints;
        }
        Debug.LogWarning($"[WeaponManager] Attack points for {weaponType} not found");
        return new List<Transform>();
    }

    /// <summary>
    /// Gets attack radius for the specified weapon type
    /// </summary>
    public float GetAttackRadius(Weapon weaponType)
    {
        if (weaponDataDict.TryGetValue(weaponType, out WeaponData weaponData))
        {
            return weaponData.attackRadius;
        }
        Debug.LogWarning($"[WeaponManager] Attack radius for {weaponType} not found. Using default value");
        return 0.5f;
    }
    #endregion

    #region Weapon State Management
    /// <summary>
    /// Checks if the specified weapon type is available
    /// </summary>
    public bool IsWeaponAvailable(Weapon weaponType)
    {
        return weaponDataDict.ContainsKey(weaponType);
    }

    /// <summary>
    /// Equips the specified weapon type if available
    /// </summary>
    public void EquipWeapon(Weapon weaponType)
    {
        if (!IsWeaponAvailable(weaponType))
        {
            Debug.LogWarning($"[WeaponManager] Attempted to equip unavailable weapon: {weaponType}");
            return;
        }

        characterController.rightWeapon = weaponType;
        characterController.leftWeapon = weaponType.Is2HandedWeapon() ? weaponType : Weapon.Unarmed;
    }

    /// <summary>
    /// Unequips current weapons
    /// </summary>
    public void UnequipWeapon()
    {
        characterController.rightWeapon = Weapon.Unarmed;
        characterController.leftWeapon = Weapon.Unarmed;
    }

    /// <summary>
    /// Determines the current weapon side based on equipped weapons
    /// </summary>
    public Side GetCurrentWeaponSide()
    {
        if (characterController.rightWeapon.Is2HandedWeapon())
        {
            return Side.None;
        }
        
        if (characterController.rightWeapon != Weapon.Unarmed && 
            characterController.leftWeapon != Weapon.Unarmed)
        {
            return Side.Dual;
        }
        
        return Time.frameCount % 2 == 0 ? Side.Right : Side.Left;
    }
    #endregion
}
