using UnityEngine;

/// <summary>
/// Defines all available attack animation types for different weapons and attack levels
/// </summary>
public enum AttackAnimationType
{
    #region Two-Handed Sword Attacks
    [InspectorName("Two-Hand Sword - Light Attack 1")]
    TwoHandSword_Light1,
    
    [InspectorName("Two-Hand Sword - Light Attack 2")]
    TwoHandSword_Light2,
    
    [InspectorName("Two-Hand Sword - Medium Attack 1")]
    TwoHandSword_Medium1,
    
    [InspectorName("Two-Hand Sword - Medium Attack 2")]
    TwoHandSword_Medium2,
    
    [InspectorName("Two-Hand Sword - Heavy Attack")]
    TwoHandSword_Heavy1,
    #endregion

    #region Unarmed Attacks
    [InspectorName("Unarmed - Light Attack 1")]
    Unarmed_Light1,
    
    [InspectorName("Unarmed - Light Attack 2")]
    Unarmed_Light2,
    
    [InspectorName("Unarmed - Medium Attack 1")]
    Unarmed_Medium1,
    
    [InspectorName("Unarmed - Medium Attack 2")]
    Unarmed_Medium2,
    
    [InspectorName("Unarmed - Heavy Attack")]
    Unarmed_Heavy1
    #endregion
}