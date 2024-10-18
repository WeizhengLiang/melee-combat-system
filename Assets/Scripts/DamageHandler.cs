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
        Debug.Log($"ReceiveHit called, damage: {damage}, attackerToughness: {attackerToughness}, CurrentAttackPhase: {attackHandler.CurrentAttackPhase}");
        
        bool wasInterrupted = attackHandler.CurrentAttackPhase != AttackHandler.AttackPhase.None && 
                              attackHandler.TryInterruptAttack(attackerToughness);
        
        Debug.Log($"Was attack interrupted: {wasInterrupted}");

        if (wasInterrupted || attackHandler.CurrentAttackPhase == AttackHandler.AttackPhase.None)
        {
            characterController.GetHit(Random.Range(1, 3));
            ApplyDamage(damage);
            Debug.Log($"Full damage applied: {damage}");
        }
        else
        {
            ApplyDamage(damage * 0.5f);
            Debug.Log($"Reduced damage applied: {damage * 0.5f}");
        }
    }

    private void ApplyDamage(float damage)
    {
        // 这里应用实际的伤害逻辑，例如减少生命值
        // characterInstance.Health -= damage;
        Debug.Log($"Received {damage} damage");
    }
}
