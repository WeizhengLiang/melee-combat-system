using UnityEngine;

public interface IAttacker
{
    void PerformAttack();
    Vector3 GetAttackPosition();
}