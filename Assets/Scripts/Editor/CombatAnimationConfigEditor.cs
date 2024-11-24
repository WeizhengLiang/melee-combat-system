using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using RPGCharacterAnims;
using RPGCharacterAnims.Lookups;

[CustomEditor(typeof(CombatAnimationConfig))]
public class CombatAnimationConfigEditor : Editor
{
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
        
        // 双手剑攻击配置
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            showTwoHandSwordAttacks = EditorGUILayout.Foldout(showTwoHandSwordAttacks, "Two-Hand Sword Attacks", true);
            if (showTwoHandSwordAttacks)
            {
                DrawAttackGroup(AttackAnimationType.TwoHandSword_Light1, AttackAnimationType.TwoHandSword_Heavy1);
            }
        }

        // 空手攻击配置
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

    private void InitializeDefaultAttacks(CombatAnimationConfig config)
    {
        if (config.attackAnimations == null)
            config.attackAnimations = new List<AttackAnimationData>();
        
        config.attackAnimations.Clear();
        
        // 只添加双手剑的基础攻击动画配置
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

    private AttackLevel GetDefaultAttackLevel(AttackAnimationType type)
    {
        if (type.ToString().Contains("Light")) return AttackLevel.Light;
        if (type.ToString().Contains("Medium")) return AttackLevel.Medium;
        if (type.ToString().Contains("Heavy")) return AttackLevel.Heavy;
        return AttackLevel.Light;
    }

    private KnockbackType GetDefaultKnockbackType(AttackAnimationType type)
    {
        if (type.ToString().Contains("Heavy") || type.ToString().Contains("Special"))
            return KnockbackType.Knockback2;
        return KnockbackType.Knockback1;
    }

    private int GetDefaultLegacyNumber(AttackAnimationType type)
    {
        // 这里需要根据实际的动画编号来设置
        return (int)type + 1;
    }
}
