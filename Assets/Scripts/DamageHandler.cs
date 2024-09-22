using System;
using RPGCharacterAnims;
using RPGCharacterAnims.Lookups;
using UnityEngine;
using Random = UnityEngine.Random;

public class DamageHandler : MonoBehaviour, IDamageable
{
    private RPGCharacterController characterController;
    private AttackHandler attackHandler;

    private void Awake()
    {
        characterController = GetComponent<RPGCharacterController>();
    }

    private void Start()
    {
        attackHandler = characterController.GetHandler(HandlerTypes.Attack) as AttackHandler;
    }

    public void ReceiveHit(Vector3 attackerPosition, float damage, float attackerToughness)
    {
        bool wasInterrupted = attackHandler.CurrentAttackPhase != AttackHandler.AttackPhase.None && 
                              attackHandler.TryInterruptAttack(attackerToughness);

        if (wasInterrupted || attackHandler.CurrentAttackPhase == AttackHandler.AttackPhase.None)
        {
            characterController.GetHit(Random.Range(1, 3));
            ApplyDamage(damage);

            // 如果需要击退效果，可以取消下面的注释
            // Vector3 knockbackDirection = (transform.position - attackerPosition).normalized;
            // knockbackDirection.y = 0;
            // transform.position += knockbackDirection * 0.5f;
        }
        else
        {
            // 如果攻击没有被打断，仍然承受一定比例的伤害
            ApplyDamage(damage * 0.5f);
        }
    }

    private void ApplyDamage(float damage)
    {
        // 这里应用实际的伤害逻辑，例如减少生命值
        // characterInstance.Health -= damage;
        Debug.Log($"Received {damage} damage");
    }
}