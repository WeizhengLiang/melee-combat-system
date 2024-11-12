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

    public void ReceiveHit(Vector3 hitPosition, AttackLevel attackerLevel)
    {
        if (attackHandler == null)
        {
            Debug.LogError("AttackHandler is null in DamageHandler");
            return;
        }

        if (attackHandler.CurrentAttackPhase == AttackHandler.AttackPhase.None || 
            attackHandler.IsAttackInterrupted)
        {
            // 根据攻击等级选择击退类型
            KnockbackType knockbackType = attackerLevel switch
            {
                AttackLevel.Light => KnockbackType.Knockback1,
                AttackLevel.Medium => KnockbackType.Knockback1,
                AttackLevel.Heavy => KnockbackType.Knockback2,
                AttackLevel.Special => KnockbackType.Knockback2,
                _ => KnockbackType.Knockback1
            };

            // 使用原有的 Knockback 系统
            characterController.Knockback(knockbackType);
        }
    }
}
