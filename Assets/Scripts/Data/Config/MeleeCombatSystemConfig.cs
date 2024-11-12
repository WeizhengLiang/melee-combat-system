using UnityEngine;
using RPGCharacterAnims.Lookups;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MeleeCombatSystemConfig", menuName = "RPG/Melee Combat System Config", order = 1)]
public class MeleeCombatSystemConfig : ScriptableObject
{
    // [System.Serializable]
    // public class WeaponData
    // {
    //     public Weapon weaponType;
    //     public GameObject prefab;
    //     public WeaponAttackPoints attackPoints;
    //     public float attackRadius = 0.5f;
    //     public float damageMultiplier = 1f;
    //     public float attackSpeedMultiplier = 1f;
    //     public float staminaCost = 10f;
    // }
    //
    // [Header("Weapon Data")]
    // public List<WeaponData> weaponDataList = new List<WeaponData>();

    [Header("General Combat Settings")]
    public float baseAttackDamage = 10f;
    public float baseAttackSpeed = 1f;
    public float criticalHitChance = 0.1f;
    public float criticalHitMultiplier = 2f;
    public float blockChance = 0.2f;
    public float blockDamageReduction = 0.5f;
    public float toughness = 10f;

    [Header("Stamina System")]
    public float maxStamina = 100f;
    public float staminaRegenRate = 5f;
    public float baseAttackStaminaCost = 20f;

    [Header("Combo System")]
    public int maxComboChain = 3;
    public float comboTimeWindow = 1.5f;

    // public WeaponData GetWeaponData(Weapon weaponType)
    // {
    //     return weaponDataList.Find(w => w.weaponType == weaponType);
    // }
    //
    // public List<Weapon> GetAvailableWeapons()
    // {
    //     return weaponDataList.ConvertAll(w => w.weaponType);
    // }
}