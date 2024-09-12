using RPGCharacterAnims;
using RPGCharacterAnims.Lookups;
using UnityEngine;

public class DamageHandler : MonoBehaviour, IDamageable
{
    private RPGCharacterController characterController;
    private AttackHandler attackHandler;

    private void Awake()
    {
        characterController = GetComponent<RPGCharacterController>();
        attackHandler = characterController.GetHandler(HandlerTypes.Attack) as AttackHandler;
    }

    public void ReceiveHit(Vector3 attackerPosition, float attackerToughness)
    {
        bool wasInterrupted = false;

        if (attackHandler != null)
        {
            wasInterrupted = attackHandler.TryInterruptAttack(attackerToughness);
        }

        if (wasInterrupted || attackHandler == null)
        {
            // 如果攻击被打断或者角色没有在攻击中，执行普通的受击逻辑
            characterController.GetHit(Random.Range(1, 3));

            // 简单的击退效果
            // Vector3 knockbackDirection = (transform.position - attackerPosition).normalized;
            // knockbackDirection.y = 0;
            // transform.position += knockbackDirection * 0.5f;
        }
        else
        {
            characterController.GetHit(Random.Range(1, 3));
        }
    }
}