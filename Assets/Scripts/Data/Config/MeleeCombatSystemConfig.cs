using UnityEngine;
using RPGCharacterAnims.Lookups;

/// <summary>
/// Configuration for the melee combat system parameters and settings
/// </summary>
[CreateAssetMenu(fileName = "MeleeCombatSystemConfig", menuName = "Combat/Melee System Config")]
public class MeleeCombatSystemConfig : ScriptableObject
{
    #region Combat Settings
    [Header("Base Combat Parameters")]
    [Tooltip("Base damage for all attacks before modifiers")]
    public float baseAttackDamage = 10f;
    
    [Tooltip("Base attack speed multiplier")]
    public float baseAttackSpeed = 1f;
    
    [Range(0f, 1f)]
    [Tooltip("Chance to land a critical hit")]
    public float criticalHitChance = 0.1f;
    
    [Tooltip("Damage multiplier for critical hits")]
    public float criticalHitMultiplier = 2f;
    
    [Range(0f, 1f)]
    [Tooltip("Base chance to block attacks")]
    public float blockChance = 0.2f;
    
    [Range(0f, 1f)]
    [Tooltip("Percentage of damage reduced when blocking")]
    public float blockDamageReduction = 0.5f;
    
    [Tooltip("Base toughness stat affecting damage reduction")]
    public float toughness = 10f;
    #endregion

    #region Stamina System
    [Header("Stamina Parameters")]
    [Tooltip("Maximum stamina points")]
    public float maxStamina = 100f;
    
    [Tooltip("Rate at which stamina regenerates per second")]
    public float staminaRegenRate = 5f;
    
    [Tooltip("Base stamina cost for attacks")]
    public float baseAttackStaminaCost = 20f;
    #endregion

    #region Combo System
    [Header("Combo Parameters")]
    [Tooltip("Maximum number of hits in a combo chain")]
    [Range(1, 10)]
    public int maxComboChain = 3;
    
    [Tooltip("Time window to continue a combo (in seconds)")]
    public float comboTimeWindow = 1.5f;
    #endregion
}