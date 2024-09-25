using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using RPGCharacterAnims;

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
    private bool isCombatConfigEditing = false;
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
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Character Instance", characterInstance, typeof(GameObject), true);
            EditorGUI.EndDisabledGroup();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            if (selectedSetupMode == SetupMode.Default)
            {
                GUI.enabled = false;
            }

            GUILayout.Space(10);
            GUILayout.Label("Component Toggles", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(GUI.skin.box);
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

            GUILayout.Space(10);
            GUILayout.Label("Melee Combat System Settings", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            if (useMeleeCombatSystem)
            {
                DrawMeleeCombatSystemSettings();
            }
            EditorGUILayout.EndVertical();

            if (selectedCharacterType == CharacterType.CharacterNPC && useRPGCharacterWeaponController)
            {
                GUILayout.Space(10);
                GUILayout.Label("RPG Character Weapon Controller Settings", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                DrawRPGCharacterWeaponControllerSettings();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.BeginHorizontal();

            if (selectedCharacterType == CharacterType.Character)
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
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name", GUILayout.Width(200));
            EditorGUILayout.LabelField("Type", GUILayout.Width(200));
            EditorGUILayout.LabelField("", GUILayout.Width(100)); // 占位符，用于对齐删除按钮
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < availableWeaponsSO.Count; i++)
            {
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

            if (GUILayout.Button("Remove All Weapons"))
            {
                if (EditorUtility.DisplayDialog("Confirm Remove All Weapons", "Are you sure you want to remove all weapons?", "Yes", "No"))
                {
                    availableWeaponsSO.Clear();
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

    private Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
            {
                return child;
            }
            else
            {
                Transform found = FindDeepChild(child, childName);
                if (found != null)
                {
                    return found;
                }
            }
        }
        return null;
    }

    private void DrawMeleeCombatSystemSettings()
    {
        MeleeCombatSystem meleeCombatSystem = characterInstance.GetComponent<MeleeCombatSystem>();
        if (meleeCombatSystem != null)
        {
            EditorGUILayout.LabelField("Melee Combat System Config", EditorStyles.boldLabel);

            // 获取所有的 Combat Config
            string[] guids = AssetDatabase.FindAssets("t:MeleeCombatSystemConfig", new[] { CharacterCombatCfgsPath });
            List<string> configNames = new List<string> { "Please select character combat config" };
            List<MeleeCombatSystemConfig> configs = new List<MeleeCombatSystemConfig>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MeleeCombatSystemConfig config = AssetDatabase.LoadAssetAtPath<MeleeCombatSystemConfig>(path);
                configs.Add(config);
                configNames.Add(config.name);
            }

            int selectedIndex = combatConfig != null ? configs.IndexOf(combatConfig) + 1 : 0;
            int newSelectedIndex = EditorGUILayout.Popup("Combat Config", selectedIndex, configNames.ToArray());

            if (newSelectedIndex != selectedIndex)
            {
                if (newSelectedIndex == 0)
                {
                    combatConfig = null;
                }
                else
                {
                    combatConfig = configs[newSelectedIndex - 1];
                }
                GUI.changed = true;
            }

            if (!isCombatConfigEditing)
            {
                if (GUILayout.Button("Create New Config"))
                {
                    string path = EditorUtility.SaveFilePanelInProject("Save Combat Config", "MeleeCombatSystemConfig", "asset", "Please enter a file name to save the config to", CharacterCombatCfgsPath);
                    if (!string.IsNullOrEmpty(path))
                    {
                        combatConfig = CreateInstance<MeleeCombatSystemConfig>();
                        AssetDatabase.CreateAsset(combatConfig, path);
                        AssetDatabase.SaveAssets();
                        GUI.changed = true;
                    }
                }
            }

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Combat Config", combatConfig, typeof(MeleeCombatSystemConfig), false);
            EditorGUI.EndDisabledGroup();

            if (!isCombatConfigEditing && selectedIndex != 0)
            {
                if (GUILayout.Button("Edit Config"))
                {
                    isCombatConfigEditing = true;
                }
            }

            if (isCombatConfigEditing)
            {
                if (GUILayout.Button("Save Config"))
                {
                    EditorUtility.SetDirty(combatConfig);
                    AssetDatabase.SaveAssets();
                    isCombatConfigEditing = false;
                    GUI.changed = true;
                }
                
                EditorGUI.BeginDisabledGroup(false);
                combatConfig.baseAttackDamage = EditorGUILayout.FloatField("Base Attack Damage", combatConfig.baseAttackDamage);
                combatConfig.baseAttackSpeed = EditorGUILayout.FloatField("Base Attack Speed", combatConfig.baseAttackSpeed);
                combatConfig.criticalHitChance = EditorGUILayout.FloatField("Critical Hit Chance", combatConfig.criticalHitChance);
                combatConfig.criticalHitMultiplier = EditorGUILayout.FloatField("Critical Hit Multiplier", combatConfig.criticalHitMultiplier);
                combatConfig.blockChance = EditorGUILayout.FloatField("Block Chance", combatConfig.blockChance);
                combatConfig.blockDamageReduction = EditorGUILayout.FloatField("Block Damage Reduction", combatConfig.blockDamageReduction);
                combatConfig.toughness = EditorGUILayout.FloatField("Toughness", combatConfig.toughness);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                if (selectedIndex != 0)
                {
                    EditorGUI.BeginDisabledGroup(true);
                    combatConfig.baseAttackDamage = EditorGUILayout.FloatField("Base Attack Damage", combatConfig.baseAttackDamage);
                    combatConfig.baseAttackSpeed = EditorGUILayout.FloatField("Base Attack Speed", combatConfig.baseAttackSpeed);
                    combatConfig.criticalHitChance = EditorGUILayout.FloatField("Critical Hit Chance", combatConfig.criticalHitChance);
                    combatConfig.criticalHitMultiplier = EditorGUILayout.FloatField("Critical Hit Multiplier", combatConfig.criticalHitMultiplier);
                    combatConfig.blockChance = EditorGUILayout.FloatField("Block Chance", combatConfig.blockChance);
                    combatConfig.blockDamageReduction = EditorGUILayout.FloatField("Block Damage Reduction", combatConfig.blockDamageReduction);
                    combatConfig.toughness = EditorGUILayout.FloatField("Toughness", combatConfig.toughness);
                    EditorGUI.EndDisabledGroup();
                }
                
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
        
        string warningMessage = "The following component/infos are missing:\n";

        if (selectedSetupMode == SetupMode.Default)
        {
            if (combatConfig == null)
            {
                warningMessage += "- Combat Config\n";
            }
            if (availableWeaponsSO.Count == 0)
            {
                warningMessage += "- Weapons\n";
            }
            if (selectedCharacterType == CharacterType.Character && !setAsMainCameraTarget)
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
            if (selectedCharacterType == CharacterType.CharacterNPC && !useRPGCharacterWeaponController)
            {
                warningMessage += "- RPG Character Weapon Controller\n";
            }
            
            // component information missing
            if (combatConfig == null)
            {
                warningMessage += "- Combat Config\n";
            }
            if (availableWeaponsSO.Count == 0)
            {
                warningMessage += "- Weapons\n";
            }
            if (selectedCharacterType == CharacterType.Character && !setAsMainCameraTarget)
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

        // 创建一个新的游戏对象作为最终生成的角色
        GameObject finalCharacter = Instantiate(characterInstance);
        finalCharacter.name = "Generated " + selectedCharacterType.ToString();

        // 应用所有设置...
        if (useWeaponManager)
        {
            WeaponManager weaponManager = finalCharacter.GetComponent<WeaponManager>();
            if (weaponManager != null)
            {
                foreach (var temWeaponDataSO in availableWeaponsSO)
                {
                    Transform handTransform = FindDeepChild(finalCharacter.transform, "B_R_Hand");
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

        if (selectedCharacterType == CharacterType.Character && setAsMainCameraTarget)
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

    private void ClearAllSetup()
    {
        if (EditorUtility.DisplayDialog("Clear All Setup", "Are you sure you want to clear all setup and start fresh?", "Yes", "No"))
        {
            selectedCharacterType = CharacterType.Character;
            selectedSetupMode = SetupMode.Default;
            combatConfig = null;
            weaponControllerSettings = null;
            isCombatConfigEditing = false;
            availableWeaponsSO.Clear();

            useDamageHandler = true;
            useWeaponManager = true;
            useMeleeCombatInput = true;
            useMeleeCombatSystem = true;
            useRPGCharacterWeaponController = true;
            setAsMainCameraTarget = false;

            // Recreate the preview instance
            DestroyImmediate(characterInstance);
            characterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CharacterPrefabPath);
            characterInstance = PrefabUtility.InstantiatePrefab(characterPrefab) as GameObject;
            characterInstance.name = "Preview " + selectedCharacterType.ToString();

            // Reset components
            ApplyComponentIfNeeded<MeleeCombatInput>(characterInstance, useMeleeCombatInput);
            ApplyComponentIfNeeded<MeleeCombatSystem>(characterInstance, useMeleeCombatSystem);
            ApplyComponentIfNeeded<WeaponManager>(characterInstance, useWeaponManager);
            ApplyComponentIfNeeded<DamageHandler>(characterInstance, useDamageHandler);
            ApplyComponentIfNeeded<RPGCharacterWeaponController>(characterInstance, useRPGCharacterWeaponController);
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