using UnityEngine;
using RPGCharacterAnims.Lookups;

/// <summary>
/// Defines the data structure for attack animations and their properties
/// </summary>
[System.Serializable]
public class AttackAnimationData
{
    #region Animation Properties
    [Header("Animation Type")]
    [Tooltip("The type of attack animation")]
    public AttackAnimationType animationType;

    [Tooltip("The strength level of this attack")]
    public AttackLevel attackLevel;

    [Header("Combat Properties")]
    [Tooltip("Type of knockback effect")]
    public KnockbackType knockbackType;

    [Tooltip("Duration of the animation in seconds")]
    [Range(0.1f, 5.0f)]
    public float duration;

    [Header("Legacy Support")]
    [Tooltip("Animation number for legacy system compatibility")]
    public int legacyAnimationNumber;
    #endregion

    #region Constructors
    public AttackAnimationData()
    {
        // Default values
        duration = 1.0f;
        attackLevel = AttackLevel.Light;
        knockbackType = KnockbackType.Knockback1;
    }

    public AttackAnimationData(AttackAnimationType type, AttackLevel level)
    {
        animationType = type;
        attackLevel = level;
        duration = 1.0f;
        knockbackType = KnockbackType.Knockback1;
    }
    #endregion
}