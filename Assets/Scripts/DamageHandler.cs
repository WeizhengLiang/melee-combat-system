using RPGCharacterAnims;
using UnityEngine;

public class DamageHandler : MonoBehaviour, IDamageable
{
    private RPGCharacterController characterController;

    private void Awake()
    {
        characterController = GetComponent<RPGCharacterController>();
    }

    public void ReceiveHit(Vector3 attackerPosition)
    {
        if (characterController != null)
        {
            characterController.GetHit(Random.Range(1, 3));
        }

        // 简单的击退效果
        Vector3 knockbackDirection = (transform.position - attackerPosition).normalized;
        knockbackDirection.y = 0;
        transform.position += knockbackDirection * 0.5f;
    }
}