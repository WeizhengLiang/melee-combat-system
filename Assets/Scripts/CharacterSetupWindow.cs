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
    private GameObject characterInstance;

    private void OnGUI()
    {
        GUILayout.Label("Character Setup", EditorStyles.boldLabel);

        selectedCharacterType = (CharacterType)EditorGUILayout.EnumPopup("Character Type", selectedCharacterType);
        selectedSetupMode = (SetupMode)EditorGUILayout.EnumPopup("Setup Mode", selectedSetupMode);

        GameObject newPrefab = selectedCharacterType == CharacterType.Character
            ? AssetDatabase.LoadAssetAtPath<GameObject>(CharacterPrefabPath)
            : AssetDatabase.LoadAssetAtPath<GameObject>(CharacterNPCPrefabPath);

        if (newPrefab != characterPrefab)
        {
            characterPrefab = newPrefab;
            if (characterInstance != null)
            {
                DestroyImmediate(characterInstance);
            }
            characterInstance = PrefabUtility.InstantiatePrefab(characterPrefab) as GameObject;
            characterInstance.name = "Preview " + selectedCharacterType.ToString();
        }

        if (characterInstance != null)
        {
            EditorGUILayout.ObjectField("Character Instance", characterInstance, typeof(GameObject), true);
            // 继续绘制其他UI元素...
        
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

                ApplyComponentIfNeeded<MeleeCombatInput>(characterInstance, useMeleeCombatInput);
                ApplyComponentIfNeeded<MeleeCombatSystem>(characterInstance, useMeleeCombatSystem);
                ApplyComponentIfNeeded<WeaponManager>(characterInstance, useWeaponManager);
                ApplyComponentIfNeeded<DamageHandler>(characterInstance, useDamageHandler);
            }
            else
            {
                DrawComponentToggle<MeleeCombatSystem>("Melee Combat System", ref useMeleeCombatSystem);
                DrawComponentToggle<WeaponManager>("Weapon Manager", ref useWeaponManager);
                DrawComponentToggle<DamageHandler>("Damage Handler", ref useDamageHandler);
                DrawComponentToggle<RPGCharacterWeaponController>("RPG Character Weapon Controller", ref useRPGCharacterWeaponController);

                ApplyComponentIfNeeded<MeleeCombatSystem>(characterInstance, useMeleeCombatSystem);
                ApplyComponentIfNeeded<WeaponManager>(characterInstance, useWeaponManager);
                ApplyComponentIfNeeded<DamageHandler>(characterInstance, useDamageHandler);
                ApplyComponentIfNeeded<RPGCharacterWeaponController>(characterInstance, useRPGCharacterWeaponController);
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
        bool hasComponent = characterInstance.GetComponent<T>() != null;
        EditorGUI.BeginChangeCheck();
        useComponent = EditorGUILayout.Toggle(label, useComponent);
        if (EditorGUI.EndChangeCheck())
        {
            if (useComponent && !hasComponent)
            {
                characterInstance.AddComponent<T>();
            }
            else if (!useComponent && hasComponent)
            {
                DestroyImmediate(characterInstance.GetComponent<T>());
            }
        }
    }

    private void DrawWeaponManagerSettings()
    {
        WeaponManager weaponManager = characterInstance.GetComponent<WeaponManager>();
        if (weaponManager != null)
        {
            EditorGUILayout.LabelField("Available Weapons", EditorStyles.boldLabel);
            
            for (int i = 0; i < availableWeapons.Count; i++)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                availableWeapons[i].weaponType = (Weapon)EditorGUILayout.EnumPopup("Weapon Type", availableWeapons[i].weaponType);
                availableWeapons[i].prefab = EditorGUILayout.ObjectField("Weapon Prefab", availableWeapons[i].prefab, typeof(GameObject), false) as GameObject;
                availableWeapons[i].attackRadius = EditorGUILayout.FloatField("Attack Radius", availableWeapons[i].attackRadius);

                EditorGUILayout.HelpBox("Please setup Attack Points with 'AttackPoint' tag in weapon prefab", MessageType.Info);

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
        MeleeCombatSystem meleeCombatSystem = characterInstance.GetComponent<MeleeCombatSystem>();
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
        RPGCharacterWeaponController weaponController = characterInstance.GetComponent<RPGCharacterWeaponController>();
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
        if (characterInstance == null)
        {
            Debug.LogError("Character instance is not set!");
            return;
        }

        // 创建一个新的游戏对象作为最终生成的角色
        GameObject finalCharacter = Instantiate(characterInstance);
        finalCharacter.name = "Generated " + selectedCharacterType.ToString();

        // 应用所有设置...
        if (useWeaponManager)
        {
            WeaponManager weaponManager = finalCharacter.GetComponent<WeaponManager>();
            if (weaponManager != null)
            {
                weaponManager.availableWeapons = new List<WeaponManager.WeaponData>(availableWeapons);
            }
        }

        if (useMeleeCombatSystem)
        {
            MeleeCombatSystem meleeCombatSystem = finalCharacter.GetComponent<MeleeCombatSystem>();
            if (meleeCombatSystem != null)
            {
                meleeCombatSystem.combatConfig = combatConfig;
            }
        }

        if (useRPGCharacterWeaponController)
        {
            RPGCharacterWeaponController weaponController = finalCharacter.GetComponent<RPGCharacterWeaponController>();
            if (weaponController != null && weaponControllerSettings != null)
            {
                EditorUtility.CopySerialized(weaponControllerSettings, weaponController);
            }
        }

        Undo.RegisterCreatedObjectUndo(finalCharacter, "Generate Character");

        // 重新创建预览实例
        DestroyImmediate(characterInstance);
        characterInstance = PrefabUtility.InstantiatePrefab(characterPrefab) as GameObject;
        characterInstance.name = "Preview " + selectedCharacterType.ToString();
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
    
    private void OnDestroy()
    {
        DestroyPreviewInstance();
    }
    
    private void OnEnable()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            DestroyPreviewInstance();
        }
    }

    private void DestroyPreviewInstance()
    {
        if (characterInstance != null)
        {
            DestroyImmediate(characterInstance);
            characterInstance = null;
        }
    }
}