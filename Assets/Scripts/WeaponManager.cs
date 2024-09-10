using UnityEngine;
using System.Collections.Generic;
using RPGCharacterAnims;
using RPGCharacterAnims.Extensions;
using RPGCharacterAnims.Lookups;

public class WeaponManager : MonoBehaviour
{
    [System.Serializable]
    public class WeaponData
    {
        public Weapon weaponType;
        public GameObject prefab;
        public List<Transform> attackPoints;
        public float attackRadius = 0.5f;
    }

    public List<WeaponData> availableWeapons = new List<WeaponData>();
    private Dictionary<Weapon, WeaponData> weaponDataDict = new Dictionary<Weapon, WeaponData>();

    private RPGCharacterController characterController;

    private void Awake()
    {
        characterController = GetComponent<RPGCharacterController>();
        InitializeWeaponData();
    }

    private void InitializeWeaponData()
    {
        foreach (var weaponData in availableWeapons)
        {
            if (!weaponDataDict.ContainsKey(weaponData.weaponType))
            {
                weaponDataDict[weaponData.weaponType] = weaponData;
            }
            else
            {
                Debug.LogWarning($"Duplicate weapon type found: {weaponData.weaponType}. Ignoring duplicate.");
            }
        }
    }

    public GameObject GetWeaponPrefab(Weapon weaponType)
    {
        if (weaponDataDict.TryGetValue(weaponType, out WeaponData weaponData))
        {
            return weaponData.prefab;
        }
        Debug.LogWarning($"Weapon prefab for {weaponType} not found.");
        return null;
    }

    public List<Transform> GetAttackPoints(Weapon weaponType)
    {
        if (weaponDataDict.TryGetValue(weaponType, out WeaponData weaponData))
        {
            return weaponData.attackPoints;
        }
        Debug.LogWarning($"Attack points for {weaponType} not found.");
        return new List<Transform>();
    }

    public float GetAttackRadius(Weapon weaponType)
    {
        if (weaponDataDict.TryGetValue(weaponType, out WeaponData weaponData))
        {
            return weaponData.attackRadius;
        }
        Debug.LogWarning($"Attack radius for {weaponType} not found. Using default value.");
        return 0.5f;
    }

    public bool IsWeaponAvailable(Weapon weaponType)
    {
        return weaponDataDict.ContainsKey(weaponType);
    }

    public void EquipWeapon(Weapon weaponType)
    {
        if (IsWeaponAvailable(weaponType))
        {
            characterController.rightWeapon = weaponType;
            if (weaponType.Is2HandedWeapon())
            {
                characterController.leftWeapon = weaponType;
            }
            else
            {
                characterController.leftWeapon = Weapon.Unarmed;
            }
        }
        else
        {
            Debug.LogWarning($"Attempted to equip unavailable weapon: {weaponType}");
        }
    }

    public void UnequipWeapon()
    {
        characterController.rightWeapon = Weapon.Unarmed;
        characterController.leftWeapon = Weapon.Unarmed;
    }
}
