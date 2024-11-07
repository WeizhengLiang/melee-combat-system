using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using RPGCharacterAnims;
using RPGCharacterAnims.Lookups;

public class CharacterSetupWindow : EditorWindow
{
    private enum CharacterType
    {
        BuildPlayer,
        BuildNPC
    }

    private enum SetupMode
    {
        Default,
        Custom
    }

    private CharacterType selectedCharacterType;
    private SetupMode selectedSetupMode;
    private GameObject characterPrefab;
    // private MeleeCombatSystemConfig combatConfig; 隐藏数值设计
    private RPGCharacterWeaponController weaponControllerSettings;
    // private bool isCombatConfigEditing = false; 隐藏数值设计
    private List<WeaponDataSO> availableWeaponsSO = new ();

    private bool useDamageHandler = true;
    private bool useWeaponManager = true;
    private bool useMeleeCombatInput = true;
    private bool useMeleeCombatSystem = true;
    private bool useRPGCharacterWeaponController = true;
    private bool setAsMainCameraTarget = false;

    private const string CharacterPrefabPath = "Assets/ExplosiveLLC/RPG Character Mecanim Animation Pack FREE/Prefabs/Character/RPG-Character.prefab";
    private const string CharacterNPCPrefabPath = "Assets/ExplosiveLLC/RPG Character Mecanim Animation Pack FREE/Prefabs/Character/RPG-Character-NPC.prefab";
    private const string CharacterCombatCfgsPath = "Assets/Resources/CombatCfgs";
    public const string WeaponCfgsPath = "Assets/Resources/WeaponCfgs";

    private GameObject selectedPrefab;
    private GameObject playerTemplatePrefab;
    private GameObject npcTemplatePrefab;
    private const string PrefabsFolderPath = "Assets/ExplosiveLLC/RPG Character Mecanim Animation Pack FREE/Prefabs/Character";
    private const string PlayerTemplatePath = "Assets/ExplosiveLLC/RPG Character Mecanim Animation Pack FREE/Prefabs/CharacterTemplate/RPG-Character_Template.prefab";
    private const string NPCTemplatePath = "Assets/ExplosiveLLC/RPG Character Mecanim Animation Pack FREE/Prefabs/CharacterTemplate/RPG-Character-NPC_Template.prefab";

    [MenuItem("Tools/Character Setup")]
    public static void ShowWindow()
    {
        GetWindow<CharacterSetupWindow>("Character Setup");
    }

    private Vector2 scrollPosition;
    private GameObject templateCharacterInstance;

    private void OnGUI()
    {
        GUILayout.Label("Character Setup", EditorStyles.boldLabel);

        selectedCharacterType = (CharacterType)EditorGUILayout.EnumPopup("Character Type", selectedCharacterType);
        selectedSetupMode = (SetupMode)EditorGUILayout.EnumPopup("Setup Mode", selectedSetupMode);

        selectedPrefab = (GameObject)EditorGUILayout.ObjectField("Selected Prefab", selectedPrefab, typeof(GameObject), false);
        if (selectedPrefab != null && !AssetDatabase.GetAssetPath(selectedPrefab).StartsWith(PrefabsFolderPath))
        {
            EditorUtility.DisplayDialog("Invalid Prefab", "Please select a prefab from the designated prefab folder.", "OK");
            selectedPrefab = null;
        }

        GameObject templatePrefab = selectedCharacterType == CharacterType.BuildPlayer
            ? AssetDatabase.LoadAssetAtPath<GameObject>(PlayerTemplatePath)
            : AssetDatabase.LoadAssetAtPath<GameObject>(NPCTemplatePath);

        if (templatePrefab != characterPrefab)
        {
            characterPrefab = templatePrefab;
            if (templateCharacterInstance != null)
            {
                DestroyImmediate(templateCharacterInstance);
            }
            templateCharacterInstance = PrefabUtility.InstantiatePrefab(characterPrefab) as GameObject;
            templateCharacterInstance.name = "Preview " + selectedCharacterType.ToString();
        }

        if (templateCharacterInstance != null)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Character Instance", templateCharacterInstance, typeof(GameObject), true);
            EditorGUI.EndDisabledGroup();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            if (selectedSetupMode == SetupMode.Default)
            {
                GUI.enabled = false;
            }

            GUILayout.Space(10);
            GUILayout.Label("Component Toggles", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            if (selectedCharacterType == CharacterType.BuildPlayer)
            {
                DrawComponentToggle<MeleeCombatInput>("Melee Combat Input", ref useMeleeCombatInput);
                DrawComponentToggle<MeleeCombatSystem>("Melee Combat System", ref useMeleeCombatSystem);
                DrawComponentToggle<WeaponManager>("Weapon Manager", ref useWeaponManager);
                DrawComponentToggle<DamageHandler>("Damage Handler", ref useDamageHandler);

                ApplyComponentIfNeeded<MeleeCombatInput>(templateCharacterInstance, useMeleeCombatInput);
                ApplyComponentIfNeeded<MeleeCombatSystem>(templateCharacterInstance, useMeleeCombatSystem);
                ApplyComponentIfNeeded<WeaponManager>(templateCharacterInstance, useWeaponManager);
                ApplyComponentIfNeeded<DamageHandler>(templateCharacterInstance, useDamageHandler);
            }
            else
            {
                DrawComponentToggle<MeleeCombatSystem>("Melee Combat System", ref useMeleeCombatSystem);
                DrawComponentToggle<WeaponManager>("Weapon Manager", ref useWeaponManager);
                DrawComponentToggle<DamageHandler>("Damage Handler", ref useDamageHandler);
                DrawComponentToggle<RPGCharacterWeaponController>("RPG Character Weapon Controller", ref useRPGCharacterWeaponController);

                ApplyComponentIfNeeded<MeleeCombatSystem>(templateCharacterInstance, useMeleeCombatSystem);
                ApplyComponentIfNeeded<WeaponManager>(templateCharacterInstance, useWeaponManager);
                ApplyComponentIfNeeded<DamageHandler>(templateCharacterInstance, useDamageHandler);
                ApplyComponentIfNeeded<RPGCharacterWeaponController>(templateCharacterInstance, useRPGCharacterWeaponController);
            }
            EditorGUILayout.EndVertical();

            if (selectedSetupMode == SetupMode.Default)
            {
                GUI.enabled = true;
            }

            GUILayout.Space(10);
            GUILayout.Label("Weapon Manager Settings", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            if (useWeaponManager)
            {
                DrawWeaponManagerSettings();
            }
            EditorGUILayout.EndVertical();

            // 隐藏数值设计
            // GUILayout.Space(10);
            // GUILayout.Label("Melee Combat System Settings", EditorStyles.boldLabel);
            // EditorGUILayout.BeginVertical(GUI.skin.box);
            // if (useMeleeCombatSystem)
            // {
            //     DrawMeleeCombatSystemSettings();
            // }
            // EditorGUILayout.EndVertical();

            if (selectedCharacterType == CharacterType.BuildNPC && useRPGCharacterWeaponController)
            {
                GUILayout.Space(10);
                GUILayout.Label("RPG Character Weapon Controller Settings", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                DrawRPGCharacterWeaponControllerSettings();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.BeginHorizontal();

            if (selectedCharacterType == CharacterType.BuildPlayer)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                setAsMainCameraTarget = EditorGUILayout.ToggleLeft("Set this character to main camera", setAsMainCameraTarget);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.BeginVertical(GUI.skin.box);
            if (GUILayout.Button("Clear All Setup", GUILayout.Width(100)))
            {
                ClearAllSetup();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Generate Character"))
            {
                GenerateCharacter();
            }
        }
    }

    private void DrawComponentToggle<T>(string label, ref bool useComponent) where T : Component
    {
        bool hasComponent = templateCharacterInstance.GetComponent<T>() != null;
        EditorGUI.BeginChangeCheck();
        useComponent = EditorGUILayout.Toggle(label, useComponent);
        if (EditorGUI.EndChangeCheck())
        {
            if (useComponent && !hasComponent)
            {
                templateCharacterInstance.AddComponent<T>();
            }
            else if (!useComponent && hasComponent)
            {
                DestroyImmediate(templateCharacterInstance.GetComponent<T>());
            }
        }
    }

    private void DrawWeaponManagerSettings()
    {
        WeaponManager weaponManager = templateCharacterInstance.GetComponent<WeaponManager>();
        if (weaponManager != null)
        {
            EditorGUILayout.LabelField("Unarmed Attack Points Config", EditorStyles.boldLabel);
            weaponManager.unarmedConfig = (UnarmedAttackPointsConfig)EditorGUILayout.ObjectField(
                "Unarmed Config", weaponManager.unarmedConfig, typeof(UnarmedAttackPointsConfig), false);
            
            EditorGUILayout.LabelField("Available Weapons", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name", GUILayout.Width(200));
            EditorGUILayout.LabelField("Type", GUILayout.Width(200));
            EditorGUILayout.LabelField("", GUILayout.Width(100)); // 占位符，用于对齐删除按钮
            EditorGUILayout.EndHorizontal();

            // 显示 Unarmed 武器（不可删除）
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Unarmed", GUILayout.Width(200));
            EditorGUILayout.LabelField("Unarmed", GUILayout.Width(200));
            GUI.enabled = false;
            GUILayout.Button("Remove", GUILayout.Width(100));
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            // 显示其他武器
            for (int i = 0; i < availableWeaponsSO.Count; i++)
            {
                if (availableWeaponsSO[i].weaponType == Weapon.Unarmed) continue;

                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(availableWeaponsSO[i].name, GUILayout.Width(200));
                EditorGUILayout.LabelField(availableWeaponsSO[i].weaponType.ToString(), GUILayout.Width(200));
                if (GUILayout.Button("Remove", GUILayout.Width(100)))
                {
                    availableWeaponsSO.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add Weapon"))
            {
                ShowWeaponSelectionMenu();
            }

            if (GUILayout.Button("Remove All Weapons (Except Unarmed)"))
            {
                if (EditorUtility.DisplayDialog("Confirm Remove All Weapons", "Are you sure you want to remove all weapons except Unarmed?", "Yes", "No"))
                {
                    availableWeaponsSO.RemoveAll(w => w.weaponType != Weapon.Unarmed);
                }
            }
        }
    }

    private void ShowWeaponSelectionMenu()
    {
        WeaponSelectionWindow.ShowWindow(this);
    }

    public void AddSelectedWeapons(List<WeaponDataSO> selectedWeapons)
    {
        foreach (var weaponDataSO in selectedWeapons)
        {
            if(availableWeaponsSO.Any(w => w.name == weaponDataSO.name))
            {
                Debug.Log($"Weapon already exists! - {weaponDataSO.name}");
                continue;
            }

            availableWeaponsSO.Add(weaponDataSO);
        }
    }

    
// 隐藏数值设计
    // private void DrawMeleeCombatSystemSettings()
    // {
    //     MeleeCombatSystem meleeCombatSystem = templateCharacterInstance.GetComponent<MeleeCombatSystem>();
    //     if (meleeCombatSystem != null)
    //     {
    //         EditorGUILayout.LabelField("Melee Combat System Config", EditorStyles.boldLabel);

    //         // 获取所有的 Combat Config
    //         string[] guids = AssetDatabase.FindAssets("t:MeleeCombatSystemConfig", new[] { CharacterCombatCfgsPath });
    //         List<string> configNames = new List<string> { "Please select character combat config" };
    //         List<MeleeCombatSystemConfig> configs = new List<MeleeCombatSystemConfig>();

    //         foreach (string guid in guids)
    //         {
    //             string path = AssetDatabase.GUIDToAssetPath(guid);
    //             MeleeCombatSystemConfig config = AssetDatabase.LoadAssetAtPath<MeleeCombatSystemConfig>(path);
    //             configs.Add(config);
    //             configNames.Add(config.name);
    //         }

    //         int selectedIndex = combatConfig != null ? configs.IndexOf(combatConfig) + 1 : 0;
    //         int newSelectedIndex = EditorGUILayout.Popup("Combat Config", selectedIndex, configNames.ToArray());

    //         if (newSelectedIndex != selectedIndex)
    //         {
    //             if (newSelectedIndex == 0)
    //             {
    //                 combatConfig = null;
    //             }
    //             else
    //             {
    //                 combatConfig = configs[newSelectedIndex - 1];
    //             }
    //             GUI.changed = true;
    //         }

    //         if (!isCombatConfigEditing)
    //         {
    //             if (GUILayout.Button("Create New Config"))
    //             {
    //                 string path = EditorUtility.SaveFilePanelInProject("Save Combat Config", "MeleeCombatSystemConfig", "asset", "Please enter a file name to save the config to", CharacterCombatCfgsPath);
    //                 if (!string.IsNullOrEmpty(path))
    //                 {
    //                     combatConfig = CreateInstance<MeleeCombatSystemConfig>();
    //                     AssetDatabase.CreateAsset(combatConfig, path);
    //                     AssetDatabase.SaveAssets();
    //                     GUI.changed = true;
    //                 }
    //             }
    //         }

    //         EditorGUI.BeginDisabledGroup(true);
    //         EditorGUILayout.ObjectField("Combat Config", combatConfig, typeof(MeleeCombatSystemConfig), false);
    //         EditorGUI.EndDisabledGroup();

    //         if (!isCombatConfigEditing && selectedIndex != 0)
    //         {
    //             if (GUILayout.Button("Edit Config"))
    //             {
    //                 isCombatConfigEditing = true;
    //             }
    //         }

    //         if (isCombatConfigEditing)
    //         {
    //             if (GUILayout.Button("Save Config"))
    //             {
    //                 EditorUtility.SetDirty(combatConfig);
    //                 AssetDatabase.SaveAssets();
    //                 isCombatConfigEditing = false;
    //                 GUI.changed = true;
    //             }
                
    //             EditorGUI.BeginDisabledGroup(false);
    //             combatConfig.baseAttackDamage = EditorGUILayout.FloatField("Base Attack Damage", combatConfig.baseAttackDamage);
    //             combatConfig.baseAttackSpeed = EditorGUILayout.FloatField("Base Attack Speed", combatConfig.baseAttackSpeed);
    //             combatConfig.criticalHitChance = EditorGUILayout.FloatField("Critical Hit Chance", combatConfig.criticalHitChance);
    //             combatConfig.criticalHitMultiplier = EditorGUILayout.FloatField("Critical Hit Multiplier", combatConfig.criticalHitMultiplier);
    //             combatConfig.blockChance = EditorGUILayout.FloatField("Block Chance", combatConfig.blockChance);
    //             combatConfig.blockDamageReduction = EditorGUILayout.FloatField("Block Damage Reduction", combatConfig.blockDamageReduction);
    //             combatConfig.toughness = EditorGUILayout.FloatField("Toughness", combatConfig.toughness);
    //             EditorGUI.EndDisabledGroup();
    //         }
    //         else
    //         {
    //             if (selectedIndex != 0)
    //             {
    //                 EditorGUI.BeginDisabledGroup(true);
    //                 combatConfig.baseAttackDamage = EditorGUILayout.FloatField("Base Attack Damage", combatConfig.baseAttackDamage);
    //                 combatConfig.baseAttackSpeed = EditorGUILayout.FloatField("Base Attack Speed", combatConfig.baseAttackSpeed);
    //                 combatConfig.criticalHitChance = EditorGUILayout.FloatField("Critical Hit Chance", combatConfig.criticalHitChance);
    //                 combatConfig.criticalHitMultiplier = EditorGUILayout.FloatField("Critical Hit Multiplier", combatConfig.criticalHitMultiplier);
    //                 combatConfig.blockChance = EditorGUILayout.FloatField("Block Chance", combatConfig.blockChance);
    //                 combatConfig.blockDamageReduction = EditorGUILayout.FloatField("Block Damage Reduction", combatConfig.blockDamageReduction);
    //                 combatConfig.toughness = EditorGUILayout.FloatField("Toughness", combatConfig.toughness);
    //                 EditorGUI.EndDisabledGroup();
    //             }
                
    //         }
    //     }
    // }

    private void DrawRPGCharacterWeaponControllerSettings()
    {
        RPGCharacterWeaponController weaponController = templateCharacterInstance.GetComponent<RPGCharacterWeaponController>();
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
        if (templateCharacterInstance == null)
        {
            Debug.LogError("Character instance is not set!");
            return;
        }
        
        string warningMessage = "The following component/infos are missing:\n";

        if (selectedSetupMode == SetupMode.Default)
        {
            // 隐藏数值设计
            // if (combatConfig == null)
            // {
            //     warningMessage += "- Combat Config\n";
            // }
            if (availableWeaponsSO.Count == 0)
            {
                warningMessage += "- Weapons\n";
            }
            if (selectedCharacterType == CharacterType.BuildPlayer && !setAsMainCameraTarget)
            {
                warningMessage += "- Main Camera Target\n";
            }
        }
        else if (selectedSetupMode == SetupMode.Custom)
        {
            // components missing
            if (!useMeleeCombatInput)
            {
                warningMessage += "- Melee Combat Input\n";
            }
            if (!useMeleeCombatSystem)
            {
                warningMessage += "- Melee Combat System\n";
            }
            if (!useWeaponManager)
            {
                warningMessage += "- Weapon Manager\n";
            }
            if (!useDamageHandler)
            {
                warningMessage += "- Damage Handler\n";
            }
            if (selectedCharacterType == CharacterType.BuildNPC && !useRPGCharacterWeaponController)
            {
                warningMessage += "- RPG Character Weapon Controller\n";
            }
            
            // component information missing
            // 隐藏数值设计
            // if (combatConfig == null)
            // {
            //     warningMessage += "- Combat Config\n";
            // }
            if (availableWeaponsSO.Count == 0)
            {
                warningMessage += "- Weapons\n";
            }
            if (selectedCharacterType == CharacterType.BuildPlayer && !setAsMainCameraTarget)
            {
                warningMessage += "- Main Camera Target\n";
            }
        }

        if (warningMessage != "The following component/infos are missing:\n")
        {
            if (!EditorUtility.DisplayDialog("Missing Components", warningMessage + "Do you want to proceed?", "Yes", "No"))
            {
                return;
            }
        }

        GameObject finalCharacter = PrefabUtility.InstantiatePrefab(selectedPrefab) as GameObject;
        finalCharacter.name = "Generated " + selectedCharacterType.ToString();

        CopyComponentsToTarget(templateCharacterInstance, finalCharacter);

        if (useWeaponManager)
        {
            WeaponManager weaponManager = finalCharacter.GetComponent<WeaponManager>();
            if (weaponManager != null)
            {
                if (weaponManager.unarmedConfig != null)
                {
                    foreach (var attackPoint in weaponManager.unarmedConfig.attackPoints)
                    {
                        Transform bone = Utility.FindDeepChild(finalCharacter.transform, attackPoint.boneName);
                        if (bone != null)
                        {
                            // create attackpoint instances
                            // GameObject attackPointObj = new GameObject(attackPoint.name);
                            // attackPointObj.transform.SetParent(bone);
                            // attackPointObj.transform.localPosition = attackPoint.localPosition;
                            // attackPointObj.tag = "AttackPoint";
                            
                            // add unarmed attackpoint data to WeaponManager
                            WeaponManager.WeaponData weaponData = new WeaponManager.WeaponData();
                            weaponData.weaponType = Weapon.Unarmed;
                            weaponData.name = Weapon.Unarmed.ToString();
                            weaponData.weaponInstance = null;
                            weaponManager.availableWeapons.Add(weaponData);
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("Character's unarmedConfig is missing, Setup unarmed AttackPoints failed");
                    return;
                }
                
                foreach (var temWeaponDataSO in availableWeaponsSO)
                {
                    Transform handTransform = Utility.FindDeepChild(finalCharacter.transform, "B_R_Hand");
                    if (handTransform != null)
                    {
                        WeaponManager.WeaponData weaponData = new WeaponManager.WeaponData();
                        weaponData.weaponType = temWeaponDataSO.weaponType;
                        weaponData.name = temWeaponDataSO.name;
                        weaponData.weaponInstance = Instantiate(temWeaponDataSO.prefab, handTransform);
                        weaponManager.availableWeapons.Add(weaponData);
                    }
                    else
                    {
                        Debug.LogError("Hand transform not found!");
                    }
                }
            }
        }

        // 隐藏数值设计
        // if (useMeleeCombatSystem)
        // {
        //     MeleeCombatSystem meleeCombatSystem = finalCharacter.GetComponent<MeleeCombatSystem>();
        //     if (meleeCombatSystem != null)
        //     {
        //         meleeCombatSystem.combatConfig = combatConfig;
        //     }
        // }

        if (useRPGCharacterWeaponController)
        {
            RPGCharacterWeaponController weaponController = finalCharacter.GetComponent<RPGCharacterWeaponController>();
            if (weaponController != null && weaponControllerSettings != null)
            {
                EditorUtility.CopySerialized(weaponControllerSettings, weaponController);
            }
        }

        if (selectedCharacterType == CharacterType.BuildPlayer && setAsMainCameraTarget)
        {
            CameraController cameraController = FindObjectOfType<CameraController>();
            if (cameraController != null)
            {
                cameraController.cameraTarget = finalCharacter;
            }
            else
            {
                Debug.LogError("CameraController not found in the scene!");
            }
        }

        Undo.RegisterCreatedObjectUndo(finalCharacter, "Generate Character");

        // 重新创建预览实例
        DestroyImmediate(templateCharacterInstance);
        templateCharacterInstance = PrefabUtility.InstantiatePrefab(characterPrefab) as GameObject;
        templateCharacterInstance.name = "Preview " + selectedCharacterType.ToString();
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

    private void ClearAllSetup()
    {
        if (EditorUtility.DisplayDialog("Clear All Setup", "Are you sure you want to clear all setup and start fresh?", "Yes", "No"))
        {
            selectedCharacterType = CharacterType.BuildPlayer;
            selectedSetupMode = SetupMode.Default;
            // combatConfig = null; 隐藏数值设计
            weaponControllerSettings = null;
            //isCombatConfigEditing = false; 隐藏数值设计
            availableWeaponsSO.Clear();

            useDamageHandler = true;
            useWeaponManager = true;
            useMeleeCombatInput = true;
            useMeleeCombatSystem = true;
            useRPGCharacterWeaponController = true;
            setAsMainCameraTarget = false;

            // Recreate the preview instance
            DestroyImmediate(templateCharacterInstance);
            characterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CharacterPrefabPath);
            templateCharacterInstance = PrefabUtility.InstantiatePrefab(characterPrefab) as GameObject;
            templateCharacterInstance.name = "Preview " + selectedCharacterType.ToString();

            // Reset components
            ApplyComponentIfNeeded<MeleeCombatInput>(templateCharacterInstance, useMeleeCombatInput);
            ApplyComponentIfNeeded<MeleeCombatSystem>(templateCharacterInstance, useMeleeCombatSystem);
            ApplyComponentIfNeeded<WeaponManager>(templateCharacterInstance, useWeaponManager);
            ApplyComponentIfNeeded<DamageHandler>(templateCharacterInstance, useDamageHandler);
            ApplyComponentIfNeeded<RPGCharacterWeaponController>(templateCharacterInstance, useRPGCharacterWeaponController);
        }
    }
    
    private void OnDestroy()
    {
        DestroyPreviewInstance();
    }
    
    private void OnEnable()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        playerTemplatePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerTemplatePath);
        npcTemplatePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(NPCTemplatePath);
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
        if (templateCharacterInstance != null)
        {
            DestroyImmediate(templateCharacterInstance);
            templateCharacterInstance = null;
        }
    }

    public static void CopyComponentsToTarget(GameObject source, GameObject target, bool copyTransform = false)
    {
        Component[] sourceComponents = source.GetComponents<Component>();
        foreach (Component sourceComponent in sourceComponents)
        {
            if (!copyTransform && sourceComponent is Transform)
                continue;

            System.Type componentType = sourceComponent.GetType();
            Component targetComponent = target.GetComponent(componentType);

            if (targetComponent == null)
                targetComponent = target.AddComponent(componentType);

            EditorUtility.CopySerialized(sourceComponent, targetComponent);
        }
    }
}