using System.Collections.Generic;
using RPGCharacterAnims.Lookups;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "RPG/Weapon Data SO")]
public class WeaponDataSO : ScriptableObject
{
    public Weapon weaponType;
    public GameObject prefab;
    public float attackRadius = 0.5f;
}

