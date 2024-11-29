using UnityEngine;

/// <summary>
/// Defines the different levels of attack strength and their corresponding values
/// </summary>
public enum AttackLevel
{
    [InspectorName("Light Attack")]
    Light = 0,
    
    [InspectorName("Medium Attack")]
    Medium = 1,
    
    [InspectorName("Heavy Attack")]
    Heavy = 2,
    
    [InspectorName("Special Attack")]
    Special = 3
}