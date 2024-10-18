using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;

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

            // EditorGUI.BeginDisabledGroup(!isPrefabOpen);
            // if (GUILayout.Button("Remove"))
            // {
            //     if (isPrefabOpen)
            //     {
            //         DestroyImmediate(attackPoints[i].gameObject, true);
            //         attackPoints.RemoveAt(i);
            //         i--;
            //         
            //         // 标记预制体场景为 dirty
            //         EditorSceneManager.MarkSceneDirty(PrefabStageUtility.GetCurrentPrefabStage().scene);
            //         
            //         // 强制保存更改
            //         SaveChange();
            //         
            //         // 强制刷新 CharacterModelManager
            //         // CharacterModelManager.RefreshAllConfigs();
            //     }
            //     else
            //     {
            //         Debug.LogWarning("please open prefab to edit");
            //     }
            // }
            // EditorGUI.EndDisabledGroup();

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

        // if (GUILayout.Button("Save Changes"))
        // {
        //     SaveChange();
        // }
    }

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

    private void AddAttackPoint()
    {
        var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage == null)
        {
            Debug.LogWarning("please open prefab stage");
            return;
        }

        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject == null || !prefabStage.IsPartOfPrefabContents(selectedObject))
        {
            Debug.LogWarning("please select a prefab instance");
            return;
        }

        GameObject newPoint = new GameObject("AttackPoint_" + attackPoints.Count);
        newPoint.transform.SetParent(selectedObject.transform);
        newPoint.transform.localPosition = Vector3.zero;
        newPoint.tag = "AttackPoint";
        attackPoints.Add(newPoint.transform);
        Selection.activeGameObject = newPoint;

        // 手动将场景标记为dirty
        EditorSceneManager.MarkSceneDirty(prefabStage.scene);

        // 刷新AttackPoints列表
        RefreshAttackPoints();

        // 强制重绘窗口
        Repaint();
    }

    public void SaveChange()
    {
        var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage != null)
        {
            // 在预制体编辑模式下
            if (prefabStage.scene.isDirty)
            {
                // 保存预制体
                PrefabUtility.SaveAsPrefabAsset(prefabStage.prefabContentsRoot, prefabStage.assetPath);
                
                // 刷新资源数据库
                AssetDatabase.Refresh();
            }
        }

        Debug.Log("更改已成功保存。");
        
        // 刷新 AttackPoints 列表
        RefreshAttackPoints();
        
        // 强制重绘窗口
        Repaint();
    }
    
    private static void OpenPrefab(GameObject prefab)
    {
        GameObject selectedObject = prefab;
    
        if (selectedObject != null)
        {
            // 获取预制体资源路径
            string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(selectedObject);

            if(IsPreviewSceneOpened(prefabPath))
            {
                Debug.LogWarning("prefab is opened in prefab stage");
                return;
            }
        
            if (!string.IsNullOrEmpty(prefabPath))
            {
                // 加载预制体资源
                GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
                if (prefabAsset != null)
                {
                    // 打开预制体编辑器
                    AssetDatabase.OpenAsset(prefabAsset);
                }
                else
                {
                    Debug.LogError("cant open prefab");
                }
            }
            else
            {
                Debug.LogWarning("not prefab instance");
            }
        }
        else
        {
            Debug.LogWarning("please select a game object");
        }
    }

    private static bool IsPreviewSceneOpened(string prefabPath)
    {
        var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage != null)
        {
            return prefabStage.assetPath == prefabPath;
        }
        return false;
    }

}