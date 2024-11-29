using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using RPGCharacterAnims;
using RPGCharacterAnims.Lookups;

/// <summary>
/// Custom editor for configuring combat animations and their properties
/// </summary>
[CustomEditor(typeof(CombatAnimationConfig))]
public class CombatAnimationConfigEditor : Editor
{
    // Foldout states for attack categories
    private bool showTwoHandSwordAttacks = true;
    private bool showUnarmedAttacks = true;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var config = (CombatAnimationConfig)target;

        if (GUILayout.Button("Initialize Default Attacks"))
        {
            InitializeDefaultAttacks(config);
        }

        EditorGUILayout.Space(10);
        
        // Two-Hand Sword attack configurations
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            showTwoHandSwordAttacks = EditorGUILayout.Foldout(showTwoHandSwordAttacks, "Two-Hand Sword Attacks", true);
            if (showTwoHandSwordAttacks)
            {
                DrawAttackGroup(AttackAnimationType.TwoHandSword_Light1, AttackAnimationType.TwoHandSword_Heavy1);
            }
        }

        // Unarmed attack configurations
        EditorGUILayout.Space(5);
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            showUnarmedAttacks = EditorGUILayout.Foldout(showUnarmedAttacks, "Unarmed Attacks", true);
            if (showUnarmedAttacks)
            {
                DrawAttackGroup(AttackAnimationType.Unarmed_Light1, AttackAnimationType.Unarmed_Heavy1);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// Draws the attack configuration group for a specific range of attack types
    /// </summary>
    private void DrawAttackGroup(AttackAnimationType start, AttackAnimationType end)
    {
        var attackAnimations = serializedObject.FindProperty("attackAnimations");
        
        for (int i = 0; i < attackAnimations.arraySize; i++)
        {
            var element = attackAnimations.GetArrayElementAtIndex(i);
            var animationType = (AttackAnimationType)element.FindPropertyRelative("animationType").enumValueIndex;
            
            if (animationType >= start && animationType <= end)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(element.FindPropertyRelative("animationType"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("attackLevel"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("knockbackType"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("duration"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("legacyAnimationNumber"));
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }
        }
    }

    /// <summary>
    /// Initializes the default attack configurations for all attack types
    /// </summary>
    private void InitializeDefaultAttacks(CombatAnimationConfig config)
    {
        if (config.attackAnimations == null)
            config.attackAnimations = new List<AttackAnimationData>();
        
        config.attackAnimations.Clear();
        
        foreach (AttackAnimationType type in System.Enum.GetValues(typeof(AttackAnimationType)))
        {
            float duration = AnimationData.AttackDuration(type);
            var data = new AttackAnimationData
            {
                animationType = type,
                attackLevel = GetDefaultAttackLevel(type),
                knockbackType = GetDefaultKnockbackType(type),
                duration = duration > 0 ? duration : 1.0f,
                legacyAnimationNumber = GetDefaultLegacyNumber(type),
            };
            
            config.attackAnimations.Add(data);
        }
        
        EditorUtility.SetDirty(config);
    }

    /// <summary>
    /// Determines the default attack level based on the attack type name
    /// </summary>
    private AttackLevel GetDefaultAttackLevel(AttackAnimationType type)
    {
        if (type.ToString().Contains("Light")) return AttackLevel.Light;
        if (type.ToString().Contains("Medium")) return AttackLevel.Medium;
        if (type.ToString().Contains("Heavy")) return AttackLevel.Heavy;
        return AttackLevel.Light;
    }

    /// <summary>
    /// Determines the default knockback type based on the attack type
    /// </summary>
    private KnockbackType GetDefaultKnockbackType(AttackAnimationType type)
    {
        if (type.ToString().Contains("Heavy") || type.ToString().Contains("Special"))
            return KnockbackType.Knockback2;
        return KnockbackType.Knockback1;
    }

    /// <summary>
    /// Gets the default legacy animation number for the attack type
    /// </summary>
    private int GetDefaultLegacyNumber(AttackAnimationType type)
    {
        return (int)type + 1;
    }
}
