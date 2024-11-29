using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;

/// <summary>
/// Editor window for setting up attack points on character prefabs
/// </summary>
public class AttackPointSetupWindow : EditorWindow
{
    private GameObject selectedPrefab;
    private Vector2 scrollPosition;
    private List<Transform> attackPoints = new List<Transform>();
    private bool isPrefabOpend = false;
    private Transform currentEditingAttackPoint;

    [MenuItem("Tools/Attack Point Setup")]
    public static void ShowWindow()
    {
        GetWindow<AttackPointSetupWindow>("Attack Point Setup");
    }

    private void OnGUI()
    {
        selectedPrefab = EditorGUILayout.ObjectField("Character Prefab", selectedPrefab, typeof(GameObject), false) as GameObject;

        if (selectedPrefab == null) return;

        if (GUILayout.Button("Refresh Attack Points"))
        {
            RefreshAttackPoints();
        }

        EditorGUILayout.LabelField("Attack Points List", EditorStyles.boldLabel);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        for (int i = 0; i < attackPoints.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            attackPoints[i].name = EditorGUILayout.TextField(attackPoints[i].name);
            attackPoints[i].localPosition = EditorGUILayout.Vector3Field("", attackPoints[i].localPosition);

            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            bool isPrefabOpen = prefabStage != null && prefabStage.assetPath == AssetDatabase.GetAssetPath(selectedPrefab);

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Open Character Prefab"))
        {
            OpenPrefab(selectedPrefab);
            isPrefabOpend = true;
        }

        if (isPrefabOpend)
        {
            if (GUILayout.Button("Add Attack Point"))
            {
                AddAttackPoint();
            }
        }
    }

    /// <summary>
    /// Refreshes the list of attack points from the selected prefab
    /// </summary>
    private void RefreshAttackPoints()
    {
        attackPoints.Clear();
        Transform[] allChildren = selectedPrefab.GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child.CompareTag("AttackPoint"))
            {
                attackPoints.Add(child);
            }
        }
    }

    /// <summary>
    /// Adds a new attack point to the selected object in the prefab
    /// </summary>
    private void AddAttackPoint()
    {
        var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage == null)
        {
            Debug.LogWarning("Please open prefab stage first");
            return;
        }

        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject == null || !prefabStage.IsPartOfPrefabContents(selectedObject))
        {
            Debug.LogWarning("Please select a valid object in the prefab");
            return;
        }

        GameObject newPoint = new GameObject("AttackPoint_" + attackPoints.Count);
        newPoint.transform.SetParent(selectedObject.transform);
        newPoint.transform.localPosition = Vector3.zero;
        newPoint.tag = "AttackPoint";
        attackPoints.Add(newPoint.transform);
        Selection.activeGameObject = newPoint;

        EditorSceneManager.MarkSceneDirty(prefabStage.scene);
        RefreshAttackPoints();
        Repaint();
    }

    /// <summary>
    /// Saves changes made to the prefab
    /// </summary>
    public void SaveChange()
    {
        var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage != null && prefabStage.scene.isDirty)
        {
            PrefabUtility.SaveAsPrefabAsset(prefabStage.prefabContentsRoot, prefabStage.assetPath);
            AssetDatabase.Refresh();
        }

        Debug.Log("Changes saved successfully.");
        RefreshAttackPoints();
        Repaint();
    }
    
    /// <summary>
    /// Opens the selected prefab in the prefab editor
    /// </summary>
    private static void OpenPrefab(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning("Please select a game object");
            return;
        }

        string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefab);

        if (IsPreviewSceneOpened(prefabPath))
        {
            Debug.LogWarning("Prefab is already open in prefab stage");
            return;
        }

        if (!string.IsNullOrEmpty(prefabPath))
        {
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefabAsset != null)
            {
                AssetDatabase.OpenAsset(prefabAsset);
            }
            else
            {
                Debug.LogError("Cannot open prefab");
            }
        }
        else
        {
            Debug.LogWarning("Not a prefab instance");
        }
    }

    /// <summary>
    /// Checks if the prefab is already opened in the prefab stage
    /// </summary>
    private static bool IsPreviewSceneOpened(string prefabPath)
    {
        var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        return prefabStage != null && prefabStage.assetPath == prefabPath;
    }
}