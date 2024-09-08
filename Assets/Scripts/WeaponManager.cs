using UnityEngine;
using System.Collections.Generic;
using RPGCharacterAnims.Lookups;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance { get; private set; }

    [System.Serializable]
    public class WeaponPrefabPair
    {
        public Weapon weaponType;
        public GameObject prefab;
    }

    public List<WeaponPrefabPair> weaponPrefabs = new List<WeaponPrefabPair>();

    private Dictionary<Weapon, GameObject> weaponPrefabDict = new Dictionary<Weapon, GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeWeaponPrefabDictionary();
    }

    private void InitializeWeaponPrefabDictionary()
    {
        foreach (var pair in weaponPrefabs)
        {
            if (!weaponPrefabDict.ContainsKey(pair.weaponType))
            {
                weaponPrefabDict[pair.weaponType] = pair.prefab;
            }
            else
            {
                Debug.LogWarning($"Duplicate weapon type found: {pair.weaponType}. Ignoring duplicate.");
            }
        }
    }

    public GameObject GetWeaponPrefab(Weapon weaponType)
    {
        if (weaponPrefabDict.TryGetValue(weaponType, out GameObject prefab))
        {
            return prefab;
        }
        Debug.LogWarning($"Weapon prefab for {weaponType} not found.");
        return null;
    }

    public Dictionary<Weapon, GameObject> GetAllWeaponPrefabs()
    {
        return new Dictionary<Weapon, GameObject>(weaponPrefabDict);
    }

    public bool IsWeaponAvailable(Weapon weaponType)
    {
        return weaponPrefabDict.ContainsKey(weaponType);
    }
}
