using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnarmedAttackPointsConfig", menuName = "RPG/Unarmed Attack Points Config")]
public class UnarmedAttackPointsConfig : ScriptableObject
{
    [System.Serializable]
    public class UnarmedAttackPoint
    {
        public string name;
        public string boneName;
        public Vector3 localPosition;
        public Transform self;
    }

    public List<UnarmedAttackPoint> attackPoints = new List<UnarmedAttackPoint>();
}