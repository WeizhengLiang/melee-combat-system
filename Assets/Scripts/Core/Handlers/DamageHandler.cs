using System;
using RPGCharacterAnims;
using RPGCharacterAnims.Actions;
using RPGCharacterAnims.Lookups;
using UnityEngine;
using Random = UnityEngine.Random;

public class DamageHandler : MonoBehaviour, IDamageable
{
    private RPGCharacterController characterController;
    private RPGCharacterMovementController movementController;
    private AttackHandler attackHandler;

    private void Awake()
    {
        characterController = GetComponent<RPGCharacterController>();
        movementController = GetComponent<RPGCharacterMovementController>();
    }

    private void Start()
    {
        attackHandler = characterController.GetHandler(HandlerTypes.Attack) as AttackHandler;
    }

    public void ReceiveHit(Vector3 hitPosition, AttackLevel attackerLevel)
    {
        if (attackHandler == null) return;

        // 只在非攻击状态或攻击被打断时处理击退
        if (attackHandler.CurrentAttackPhase == AttackHandler.AttackPhase.None || 
            attackHandler.IsAttackInterrupted)
        {
            ProcessHitReaction(hitPosition, attackerLevel);
        }
    }

    private void ProcessHitReaction(Vector3 hitPosition, AttackLevel attackerLevel)
    {
        // 计算击退方向（从攻击点指向被击打者）
        Vector3 hitDirection = (transform.position - hitPosition).normalized;
        float force = GetHitForce(attackerLevel);
        float variableForce = force * 0.2f;

        // Heavy攻击造成击倒，其他造成击退
        if (attackerLevel == AttackLevel.Heavy)
        {
            if (characterController.HandlerExists(HandlerTypes.Knockdown))
            {
                characterController.StartAction(HandlerTypes.Knockdown, 
                    new HitContext((int)KnockdownType.Knockdown1, hitDirection, force, variableForce));
            }
        }
        else
        {
            if (characterController.HandlerExists(HandlerTypes.Knockback))
            {
                // Light攻击使用Knockback1，Medium攻击使用Knockback2
                KnockbackType knockbackType = attackerLevel == AttackLevel.Medium ? 
                    KnockbackType.Knockback2 : KnockbackType.Knockback1;

                characterController.StartAction(HandlerTypes.Knockback, 
                    new HitContext((int)knockbackType, hitDirection, force, variableForce));
            }
        }
    }

    private float GetHitForce(AttackLevel attackerLevel)
    {
        return attackerLevel switch
        {
            AttackLevel.Light => 2f,
            AttackLevel.Medium => 3f,
            AttackLevel.Heavy => 4f,
            _ => 2f
        };
    }
}
