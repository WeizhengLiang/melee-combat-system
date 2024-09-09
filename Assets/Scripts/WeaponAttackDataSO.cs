using UnityEngine;
using RPGCharacterAnims.Lookups;
using System.Collections.Generic;

//[CreateAssetMenu(fileName = "WeaponAttackData", menuName = "RPG/Weapon Attack Data", order = 1)]
public class WeaponAttackDataSO : MonoBehaviour
{
    [System.Serializable]
    public class AttackPoint
    {
        public Transform atkPtTransform;
    }

    [System.Serializable]
    public class WeaponAttackData
    {
        public Weapon weaponType;
        public float attackRadius;
        public AttackPoint[] attackPoints;
    }

    public List<WeaponAttackData> weaponAttackDataList = new List<WeaponAttackData>();

    public float GetAttackRadius(Weapon weapon)
    {
        WeaponAttackData data = weaponAttackDataList.Find(w => w.weaponType == weapon);
        return data != null ? data.attackRadius : 0.5f;
    }

    public AttackPoint[] GetAttackPoints(Weapon weapon)
    {
        WeaponAttackData data = weaponAttackDataList.Find(w => w.weaponType == weapon);
        return data != null ? data.attackPoints : new AttackPoint[0];
    }
}
