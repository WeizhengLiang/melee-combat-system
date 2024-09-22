using UnityEngine;

public interface IDamageable
{
    void ReceiveHit(Vector3 attackerPosition, float damage, float toughness);
}