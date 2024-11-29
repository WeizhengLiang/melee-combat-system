using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Configuration for unarmed combat attack points and their positioning
/// </summary>
[CreateAssetMenu(fileName = "UnarmedAttackPointsConfig", menuName = "Combat/Unarmed Attack Points")]
public class UnarmedAttackPointsConfig : ScriptableObject
{
    #region Data Structures
    [System.Serializable]
    public class AttackPointConfig
    {
        [Header("Point Configuration")]
        [Tooltip("Unique identifier for this attack point")]
        public string name;

        [Tooltip("Name of the bone this point should be attached to")]
        public string boneName;

        [Tooltip("Local position offset from the bone")]
        public Vector3 localPosition;
    }
    #endregion

    #region Fields
    [Header("Attack Points")]
    [Tooltip("List of attack points for unarmed combat")]
    public List<AttackPointConfig> attackPoints = new();
    #endregion

    #region Validation
    private void OnValidate()
    {
        ValidateAttackPoints();
    }

    private void ValidateAttackPoints()
    {
        foreach (var point in attackPoints)
        {
            if (string.IsNullOrEmpty(point.name))
            {
                Debug.LogWarning($"[UnarmedAttackPointsConfig] Attack point name cannot be empty");
            }
            if (string.IsNullOrEmpty(point.boneName))
            {
                Debug.LogWarning($"[UnarmedAttackPointsConfig] Bone name cannot be empty for attack point: {point.name}");
            }
        }
    }
    #endregion
}