using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor window for selecting and managing weapons for character setup
/// </summary>
public class WeaponSelectionWindow : EditorWindow
{
    private List<WeaponDataSO> allWeapons = new List<WeaponDataSO>();
    private List<WeaponDataSO> filteredWeapons = new List<WeaponDataSO>();
    private List<WeaponDataSO> selectedWeapons = new List<WeaponDataSO>();
    private string searchString = "";
    private Vector2 scrollPosition;
    private Vector2 weaponListScrollPosition;
    private Vector2 selectedWeaponsScrollPosition;
    private CharacterSetupWindow parentWindow;

    /// <summary>
    /// Shows the weapon selection window with a reference to the parent window
    /// </summary>
    public static void ShowWindow(CharacterSetupWindow parent)
    {
        WeaponSelectionWindow window = GetWindow<WeaponSelectionWindow>("Weapon Selection");
        window.parentWindow = parent;
        window.LoadWeapons();
    }

    /// <summary>
    /// Loads all weapon configurations from the specified path
    /// </summary>
    private void LoadWeapons()
    {
        allWeapons.Clear();
        string[] guids = AssetDatabase.FindAssets("", new[] { CharacterSetupWindow.WeaponCfgsPath });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            WeaponDataSO weaponDataSO = AssetDatabase.LoadAssetAtPath<WeaponDataSO>(path);
            allWeapons.Add(weaponDataSO);
        }
        filteredWeapons = new List<WeaponDataSO>(allWeapons);
    }

    /// <summary>
    /// Draws the editor window GUI
    /// </summary>
    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        searchString = EditorGUILayout.TextField(searchString, GUILayout.Width(position.width - 110));
        if (GUILayout.Button("Search", GUILayout.Width(100)))
        {
            FilterWeapons();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear Search"))
        {
            if (EditorUtility.DisplayDialog("Confirm Clear Search", "Are you sure you want to clear the search and show all weapons?", "Yes", "No"))
            {
                ClearSearch();
            }
        }
        if (GUILayout.Button("Clear All Selected Weapons"))
        {
            if (EditorUtility.DisplayDialog("Confirm Clear All Selected Weapons", "Are you sure you want to clear all selected weapons?", "Yes", "No"))
            {
                ClearAllSelectedWeapons();
            }
        }
        GUILayout.EndHorizontal();

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        // Available Weapons Section
        EditorGUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("Available Weapons:", EditorStyles.boldLabel);
        weaponListScrollPosition = GUILayout.BeginScrollView(weaponListScrollPosition, GUILayout.Height(position.height / 2));
        foreach (var weapon in filteredWeapons)
        {
            bool isSelected = selectedWeapons.Contains(weapon);
            bool newIsSelected = EditorGUILayout.ToggleLeft(weapon.name, isSelected);
            if (newIsSelected != isSelected)
            {
                if (newIsSelected)
                    selectedWeapons.Add(weapon);
                else
                    selectedWeapons.Remove(weapon);
            }
        }
        GUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        // Selected Weapons Section
        EditorGUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("Selected Weapons:", EditorStyles.boldLabel);
        selectedWeaponsScrollPosition = GUILayout.BeginScrollView(selectedWeaponsScrollPosition, GUILayout.Height(position.height / 4));
        foreach (var weapon in selectedWeapons)
        {
            GUILayout.Label(weapon.name);
        }
        GUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        GUILayout.EndScrollView();

        if (GUILayout.Button("Confirm Selected Weapons"))
        {
            ConfirmSelection();
        }
    }

    /// <summary>
    /// Filters weapons based on the search string
    /// </summary>
    private void FilterWeapons()
    {
        if (string.IsNullOrEmpty(searchString))
        {
            filteredWeapons = new List<WeaponDataSO>(allWeapons);
        }
        else
        {
            filteredWeapons = allWeapons.Where(w => w.name.ToLower().Contains(searchString.ToLower())).ToList();
        }
        Repaint();
    }

    /// <summary>
    /// Clears the search and shows all weapons
    /// </summary>
    private void ClearSearch()
    {
        searchString = "";
        FilterWeapons();
    }

    /// <summary>
    /// Clears all selected weapons
    /// </summary>
    private void ClearAllSelectedWeapons()
    {
        selectedWeapons.Clear();
        Repaint();
    }

    /// <summary>
    /// Confirms the weapon selection and updates the parent window
    /// </summary>
    private void ConfirmSelection()
    {
        parentWindow.AddSelectedWeapons(selectedWeapons);
        Close();
    }
}