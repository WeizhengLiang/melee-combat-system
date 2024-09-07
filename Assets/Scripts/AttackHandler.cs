using System.Collections;
using RPGCharacterAnims;
using RPGCharacterAnims.Lookups;
using UnityEngine;

public class AttackHandler : MonoBehaviour, IAttacker
{
    private RPGCharacterController characterController;

    private void Awake()
    {
        characterController = GetComponent<RPGCharacterController>();
    }

    public void PerformAttack()
    {
        // 触发攻击动画
        characterController.Attack(1, Side.Right, characterController.leftWeapon, characterController.rightWeapon, 0.5f);
        StartCoroutine(DetectHit());
    }

    public UnityEngine.Vector3 GetAttackPosition()
    {
        return transform.position;
    }

    private IEnumerator DetectHit()
    {
        yield return new WaitForSeconds(0.2f);

        Collider[] hitColliders = Physics.OverlapSphere(transform.position + transform.forward * 1.5f, 1f);
        foreach (var hitCollider in hitColliders)
        {
            IDamageable damageable = hitCollider.GetComponent<IDamageable>();
            if (damageable != null && hitCollider.gameObject != this.gameObject)
            {
                damageable.ReceiveHit(GetAttackPosition());
            }
        }
    }
}