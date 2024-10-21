using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class CharacterModelManager : EditorWindow
{
    private const string ModelsFolderPath = "Assets/ExplosiveLLC/RPG Character Mecanim Animation Pack FREE/Models/Characters";
    private const string PrefabsFolderPath = "Assets/ExplosiveLLC/RPG Character Mecanim Animation Pack FREE/Prefabs/Character";
    private const string ConfigsFolderPath = "Assets/Resources/UnarmedAttackPointCfgs";

    private Vector2 previewScrollPosition;
    private Dictionary<GameObject, bool> prefabToggles = new Dictionary<GameObject, bool>();
    private Dictionary<GameObject, UnarmedAttackPointsConfig> tempConfigs = new Dictionary<GameObject, UnarmedAttackPointsConfig>();
    private GameObject importedPrefab;
    private Vector2 scrollPosition;

    [MenuItem("Tools/Character Model Manager")]
    public static void ShowWindow()
    {
        GetWindow<CharacterModelManager>("Character Model Manager");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Import New Model"))
        {
            ImportNewModel();
        }

        importedPrefab = (GameObject)EditorGUILayout.ObjectField("Imported Prefab", importedPrefab, typeof(GameObject), false);

        if (GUILayout.Button("Set Attack Points"))
        {
            AttackPointSetupWindow.ShowWindow();
        }

        DisplayAttackPointsTable();

        GUILayout.Space(10);

        DisplayConfigGenerationSection();

        if (GUILayout.Button("Generate Unarmed Config"))
        {
            GenerateUnarmedConfig();
        }
    }

    private void ImportNewModel()
    {
        string modelPath = EditorUtility.OpenFilePanel("Select Character Model", "", "fbx");
        if (string.IsNullOrEmpty(modelPath)) return;

        string fileName = Path.GetFileNameWithoutExtension(modelPath);
        string destPath = Path.Combine(ModelsFolderPath, fileName + ".fbx");

        if (!Directory.Exists(ModelsFolderPath))
        {
            Directory.CreateDirectory(ModelsFolderPath);
        }

        File.Copy(modelPath, destPath, true);
        AssetDatabase.Refresh();

        GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(destPath);
        if (modelPrefab != null)
        {
            string prefabPath = Path.Combine(PrefabsFolderPath, fileName + ".prefab");
            importedPrefab = PrefabUtility.SaveAsPrefabAsset(modelPrefab, prefabPath);
        }

        Debug.Log($"Model imported: {destPath}");
    }

    private void DisplayAttackPointsTable()
    {
        if (importedPrefab == null) return;

        EditorGUILayout.LabelField("Attack Points", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Name", GUILayout.Width(200));
        EditorGUILayout.LabelField("Bone Name", GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        Transform[] allChildren = importedPrefab.GetComponentsInChildren<Transform>();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        foreach (Transform child in allChildren)
        {
            if (child.CompareTag("AttackPoint"))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(child.name, GUILayout.Width(200));
                EditorGUILayout.LabelField(child.parent.name, GUILayout.Width(200));
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }

    private void DisplayConfigGenerationSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Character Prefabs", EditorStyles.boldLabel, GUILayout.Width(position.width / 2));
        EditorGUILayout.LabelField("Config Generation Preview", EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        
        // 左侧：预制体列表
        EditorGUILayout.BeginVertical(GUILayout.Width(position.width / 2));
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { PrefabsFolderPath });
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (!prefabToggles.ContainsKey(prefab))
            {
                prefabToggles[prefab] = false;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            prefabToggles[prefab] = EditorGUILayout.Toggle(prefabToggles[prefab], GUILayout.Width(20));
            EditorGUILayout.LabelField(prefab.name, GUILayout.Width(position.width / 2 - 30));
            if (EditorGUI.EndChangeCheck())
            {
                if (prefabToggles[prefab] && !tempConfigs.ContainsKey(prefab))
                {
                    tempConfigs[prefab] = CreateUnarmedConfigForPrefab(prefab);
                }
                Repaint();
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        // 右侧：配置预览
        EditorGUILayout.BeginVertical(GUILayout.Width(position.width / 2));
        previewScrollPosition = EditorGUILayout.BeginScrollView(previewScrollPosition);
        
        bool anyPrefabSelected = false;
        foreach (var kvp in prefabToggles.Where(kvp => kvp.Value))
        {
            anyPrefabSelected = true;
            EditorGUILayout.LabelField(kvp.Key.name, EditorStyles.boldLabel);
            DisplayConfigPreview(tempConfigs[kvp.Key]);
            EditorGUILayout.Space(10);
        }

        if (!anyPrefabSelected)
        {
            EditorGUILayout.LabelField("Select prefabs to preview", EditorStyles.centeredGreyMiniLabel);
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("(Un)Select All"))
        {
            bool allSelected = prefabToggles.Values.All(v => v);
            foreach (var key in prefabToggles.Keys.ToList())
            {
                prefabToggles[key] = !allSelected;
                if (prefabToggles[key] && !tempConfigs.ContainsKey(key))
                {
                    tempConfigs[key] = CreateUnarmedConfigForPrefab(key);
                }
            }
            Repaint();
        }

        EditorGUILayout.EndVertical();
    }

    private void DisplayConfigPreview(UnarmedAttackPointsConfig config)
    {
        EditorGUI.indentLevel++;
        
        SerializedObject serializedObject = new SerializedObject(config);
        SerializedProperty attackPointsProp = serializedObject.FindProperty("attackPoints");

        EditorGUILayout.PropertyField(attackPointsProp, true);
        
        serializedObject.ApplyModifiedProperties();

        EditorGUI.indentLevel--;
    }

    private UnarmedAttackPointsConfig CreateUnarmedConfigForPrefab(GameObject prefab)
    {
        UnarmedAttackPointsConfig config = ScriptableObject.CreateInstance<UnarmedAttackPointsConfig>();
        config.attackPoints.Clear();

        Transform[] allChildren = prefab.GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child.CompareTag("AttackPoint"))
            {
                config.attackPoints.Add(new UnarmedAttackPointsConfig.UnarmedAttackPoint
                {
                    name = child.name,
                    boneName = child.parent.name,
                    localPosition = child.localPosition,
                    parent = child.parent
                });
            }
        }

        return config;
    }

    private void GenerateUnarmedConfig()
    {
        foreach (var kvp in prefabToggles.Where(kvp => kvp.Value))
        {
            GameObject prefab = kvp.Key;
            UnarmedAttackPointsConfig config = tempConfigs[prefab];

            string configName = GetUniqueConfigName(prefab.name + "_UnarmedConfig");
            if (!string.IsNullOrEmpty(configName))
            {
                string configPath = Path.Combine(ConfigsFolderPath, configName + ".asset");
                AssetDatabase.CreateAsset(config, configPath);
                Debug.Log($"Unarmed config '{configName}' generated for prefab '{prefab.name}'.");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private string GetUniqueConfigName(string defaultName)
    {
        string configName = defaultName;

        string fullPath = EditorUtility.SaveFilePanel("Save Unarmed Config", ConfigsFolderPath, configName, "asset");
        if (string.IsNullOrEmpty(fullPath))
        {
            EditorUtility.DisplayDialog("Path Empty", "Path is empty, please select a path", "OK");
            return null; // User cancelled the operation
        }

        configName = Path.GetFileNameWithoutExtension(fullPath);
        string assetPath = "Assets" + fullPath.Substring(Application.dataPath.Length);

        if (AssetDatabase.LoadAssetAtPath<UnarmedAttackPointsConfig>(assetPath) != null)
        {
            if (!EditorUtility.DisplayDialog("Config Already Exists", 
                "A config with this name already exists. Do you want to overwrite it?", 
                "Yes", "No"))
            {
                return GetUniqueConfigName(configName); // Recursively call the method to get a new name
            }
        }

        return configName;
    }

    private string GetBonePath(Transform bone)
    {
        string path = bone.name;
        while (bone.parent != null && bone.parent != bone.root)
        {
            bone = bone.parent;
            path = bone.name + "/" + path;
        }
        return path;
    }
}