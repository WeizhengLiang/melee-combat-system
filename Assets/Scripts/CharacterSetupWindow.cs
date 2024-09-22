using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using RPGCharacterAnims;
using RPGCharacterAnims.Lookups;

public class CharacterSetupWindow : EditorWindow
{
    private enum CharacterType
    {
        Character,
        CharacterNPC
    }

    private enum SetupMode
    {
        Default,
        Custom
    }

    private CharacterType selectedCharacterType;
    private SetupMode selectedSetupMode;
    private GameObject characterPrefab;
    private MeleeCombatSystemConfig combatConfig;
    private RPGCharacterWeaponController weaponControllerSettings;
    private List<WeaponManager.WeaponData> availableWeapons = new List<WeaponManager.WeaponData>();

    private bool useDamageHandler = true;
    private bool useWeaponManager = true;
    private bool useMeleeCombatInput = true;
    private bool useMeleeCombatSystem = true;
    private bool useRPGCharacterWeaponController = true;

    private const string CharacterPrefabPath = "Assets/ExplosiveLLC/RPG Character Mecanim Animation Pack FREE/Prefabs/Character/RPG-Character.prefab";
    private const string CharacterNPCPrefabPath = "Assets/ExplosiveLLC/RPG Character Mecanim Animation Pack FREE/Prefabs/Character/RPG-Character-NPC.prefab";

    [MenuItem("Tools/Character Setup")]
    public static void ShowWindow()
    {
        GetWindow<CharacterSetupWindow>("Character Setup");
    }

    private Vector2 scrollPosition;

    private void OnGUI()
    {
        GUILayout.Label("Character Setup", EditorStyles.boldLabel);

        selectedCharacterType = (CharacterType)EditorGUILayout.EnumPopup("Character Type", selectedCharacterType);
        selectedSetupMode = (SetupMode)EditorGUILayout.EnumPopup("Setup Mode", selectedSetupMode);

        if (selectedCharacterType == CharacterType.Character)
        {
            characterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CharacterPrefabPath);
        }
        else
        {
            characterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CharacterNPCPrefabPath);
        }

        if (characterPrefab != null)
        {
            EditorGUILayout.ObjectField("Character Prefab", characterPrefab, typeof(GameObject), false);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            if (selectedSetupMode == SetupMode.Default)
            {
                GUI.enabled = false;
            }

            if (selectedCharacterType == CharacterType.Character)
            {
                DrawComponentToggle<MeleeCombatInput>("Melee Combat Input", ref useMeleeCombatInput);
                DrawComponentToggle<MeleeCombatSystem>("Melee Combat System", ref useMeleeCombatSystem);
                DrawComponentToggle<WeaponManager>("Weapon Manager", ref useWeaponManager);
                DrawComponentToggle<DamageHandler>("Damage Handler", ref useDamageHandler);
            }
            else
            {
                DrawComponentToggle<MeleeCombatSystem>("Melee Combat System", ref useMeleeCombatSystem);
                DrawComponentToggle<WeaponManager>("Weapon Manager", ref useWeaponManager);
                DrawComponentToggle<DamageHandler>("Damage Handler", ref useDamageHandler);
                DrawComponentToggle<RPGCharacterWeaponController>("RPG Character Weapon Controller", ref useRPGCharacterWeaponController);
            }

            if (selectedSetupMode == SetupMode.Default)
            {
                GUI.enabled = true;
            }

            if (useWeaponManager)
            {
                DrawWeaponManagerSettings();
            }

            if (useMeleeCombatSystem)
            {
                DrawMeleeCombatSystemSettings();
            }

            if (selectedCharacterType == CharacterType.CharacterNPC && useRPGCharacterWeaponController)
            {
                DrawRPGCharacterWeaponControllerSettings();
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Generate Character"))
            {
                GenerateCharacter();
            }
        }
    }

    private void DrawComponentToggle<T>(string label, ref bool useComponent) where T : Component
    {
        bool hasComponent = characterPrefab.GetComponent<T>() != null;
        useComponent = EditorGUILayout.Toggle(label, useComponent);

        if (useComponent != hasComponent)
        {
            if (useComponent)
            {
                characterPrefab.AddComponent<T>();
            }
            else
            {
                DestroyImmediate(characterPrefab.GetComponent<T>());
            }
        }
    }

    private void DrawWeaponManagerSettings()
    {
        WeaponManager weaponManager = characterPrefab.GetComponent<WeaponManager>();
        if (weaponManager != null)
        {
            EditorGUILayout.LabelField("Available Weapons", EditorStyles.boldLabel);
            
            for (int i = 0; i < availableWeapons.Count; i++)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                availableWeapons[i].weaponType = (Weapon)EditorGUILayout.EnumPopup("Weapon Type", availableWeapons[i].weaponType);
                availableWeapons[i].prefab = EditorGUILayout.ObjectField("Weapon Prefab", availableWeapons[i].prefab, typeof(GameObject), false) as GameObject;
                availableWeapons[i].attackRadius = EditorGUILayout.FloatField("Attack Radius", availableWeapons[i].attackRadius);

                EditorGUILayout.LabelField("Attack Points", EditorStyles.boldLabel);
                if (availableWeapons[i].attackPoints == null)
                {
                    availableWeapons[i].attackPoints = new List<Transform>();
                }

                for (int j = 0; j < availableWeapons[i].attackPoints.Count; j++)
                {
                    EditorGUILayout.BeginHorizontal();
                    availableWeapons[i].attackPoints[j] = EditorGUILayout.ObjectField($"Point {j + 1}", availableWeapons[i].attackPoints[j], typeof(Transform), true) as Transform;
                    if (GUILayout.Button("Remove Point", GUILayout.Width(100)))
                    {
                        availableWeapons[i].attackPoints.RemoveAt(j);
                        j--;
                    }
                    EditorGUILayout.EndHorizontal();
                }

                if (GUILayout.Button("Add Attack Point"))
                {
                    availableWeapons[i].attackPoints.Add(null);
                }

                if (GUILayout.Button("Remove Weapon"))
                {
                    availableWeapons.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add Weapon"))
            {
                availableWeapons.Add(new WeaponManager.WeaponData());
            }
        }
    }

    private void DrawMeleeCombatSystemSettings()
    {
        MeleeCombatSystem meleeCombatSystem = characterPrefab.GetComponent<MeleeCombatSystem>();
        if (meleeCombatSystem != null)
        {
            EditorGUILayout.LabelField("Melee Combat System Config", EditorStyles.boldLabel);
            combatConfig = EditorGUILayout.ObjectField("Combat Config", combatConfig, typeof(MeleeCombatSystemConfig), false) as MeleeCombatSystemConfig;

            if (combatConfig == null && GUILayout.Button("Create New Config"))
            {
                combatConfig = CreateInstance<MeleeCombatSystemConfig>();
                AssetDatabase.CreateAsset(combatConfig, "Assets/Resources/CombatCfgs/MeleeCombatSystemConfig.asset");
                AssetDatabase.SaveAssets();
            }

            if (combatConfig != null)
            {
                combatConfig.baseAttackDamage = EditorGUILayout.FloatField("Base Attack Damage", combatConfig.baseAttackDamage);
                combatConfig.baseAttackSpeed = EditorGUILayout.FloatField("Base Attack Speed", combatConfig.baseAttackSpeed);
                combatConfig.criticalHitChance = EditorGUILayout.FloatField("Critical Hit Chance", combatConfig.criticalHitChance);
                combatConfig.criticalHitMultiplier = EditorGUILayout.FloatField("Critical Hit Multiplier", combatConfig.criticalHitMultiplier);
                combatConfig.blockChance = EditorGUILayout.FloatField("Block Chance", combatConfig.blockChance);
                combatConfig.blockDamageReduction = EditorGUILayout.FloatField("Block Damage Reduction", combatConfig.blockDamageReduction);
                combatConfig.toughness = EditorGUILayout.FloatField("Toughness", combatConfig.toughness);
            }
        }
    }

    private void DrawRPGCharacterWeaponControllerSettings()
    {
        RPGCharacterWeaponController weaponController = characterPrefab.GetComponent<RPGCharacterWeaponController>();
        if (weaponController != null)
        {
            EditorGUILayout.LabelField("RPG Character Weapon Controller Settings", EditorStyles.boldLabel);
            
            SerializedObject serializedObject = new SerializedObject(weaponController);
            SerializedProperty property = serializedObject.GetIterator();
            bool enterChildren = true;
            while (property.NextVisible(enterChildren))
            {
                if (property.name == "m_Script") continue;
                EditorGUILayout.PropertyField(property, true);
                enterChildren = false;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }

    private void GenerateCharacter()
    {
        if (characterPrefab == null)
        {
            Debug.LogError("Character prefab is not set!");
            return;
        }

        GameObject character = PrefabUtility.InstantiatePrefab(characterPrefab) as GameObject;
        character.name = "Generated " + selectedCharacterType.ToString();

        if (selectedCharacterType == CharacterType.Character)
        {
            ApplyComponentIfNeeded<MeleeCombatInput>(character, useMeleeCombatInput);
            ApplyComponentIfNeeded<MeleeCombatSystem>(character, useMeleeCombatSystem);
            ApplyComponentIfNeeded<WeaponManager>(character, useWeaponManager);
            ApplyComponentIfNeeded<DamageHandler>(character, useDamageHandler);
        }
        else
        {
            ApplyComponentIfNeeded<MeleeCombatSystem>(character, useMeleeCombatSystem);
            ApplyComponentIfNeeded<WeaponManager>(character, useWeaponManager);
            ApplyComponentIfNeeded<DamageHandler>(character, useDamageHandler);
            ApplyComponentIfNeeded<RPGCharacterWeaponController>(character, useRPGCharacterWeaponController);
        }

        if (useWeaponManager)
        {
            WeaponManager weaponManager = character.GetComponent<WeaponManager>();
            if (weaponManager != null)
            {
                weaponManager.availableWeapons = new List<WeaponManager.WeaponData>(availableWeapons);
            }
        }

        if (useMeleeCombatSystem)
        {
            MeleeCombatSystem meleeCombatSystem = character.GetComponent<MeleeCombatSystem>();
            if (meleeCombatSystem != null)
            {
                meleeCombatSystem.combatConfig = combatConfig;
            }
        }

        if (useRPGCharacterWeaponController)
        {
            RPGCharacterWeaponController weaponController = character.GetComponent<RPGCharacterWeaponController>();
            if (weaponController != null && weaponControllerSettings != null)
            {
                EditorUtility.CopySerialized(weaponControllerSettings, weaponController);
            }
        }

        Undo.RegisterCreatedObjectUndo(character, "Generate Character");
    }

    private void ApplyComponentIfNeeded<T>(GameObject target, bool shouldApply) where T : Component
    {
        if (shouldApply)
        {
            if (target.GetComponent<T>() == null)
            {
                target.AddComponent<T>();
            }
        }
        else
        {
            T component = target.GetComponent<T>();
            if (component != null)
            {
                DestroyImmediate(component);
            }
        }
    }
}