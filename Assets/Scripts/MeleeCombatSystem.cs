using UnityEngine;
using RPGCharacterAnims;
using RPGCharacterAnims.Lookups;
using RPGCharacterAnims.Actions;
using System.Collections.Generic;

public class MeleeCombatSystem : MonoBehaviour
{
    private RPGCharacterController characterController;
    private RPGCharacterWeaponController weaponController;
    private AttackHandler attackHandler;
    private WeaponManager weaponManager;
    private CharacterInstance characterInstance;

    private bool isInImpactPhase = false;

    private int currentAttackId = 0;
    private Dictionary<int, HashSet<IDamageable>> hitTargets = new Dictionary<int, HashSet<IDamageable>>();

    private void Start()
    {
        characterController = GetComponent<RPGCharacterController>();
        weaponController = GetComponent<RPGCharacterWeaponController>();
        weaponManager = GetComponent<WeaponManager>();
        attackHandler = characterController.GetHandler(HandlerTypes.Attack) as AttackHandler;
        characterInstance = GetComponent<CharacterInstance>();

        if (attackHandler == null)
        {
            Debug.LogError("AttackHandler not found in RPGCharacterController");
            return;
        }

        // 订阅 AttackHandler 的事件
        attackHandler.OnImpactPhaseStart += StartImpactPhase;
        attackHandler.OnImpactPhaseEnd += EndImpactPhase;
        attackHandler.OnAttackActionEnd += EndAttack;
    }

    private void Update()
    {
        if (isInImpactPhase)
        {
            DetectHit(attackHandler.CurrentAttackSide);
        }
    }

    public void PerformAttack(int attackNumber, Side attackSide)
    {
        if (characterController.CanStartAction(HandlerTypes.Attack))
        {
            characterController.StartAction(HandlerTypes.Attack, new AttackContext(HandlerTypes.Attack, attackSide, attackNumber));
        }
    }

    private void StartImpactPhase()
    {
        currentAttackId++;
        isInImpactPhase = true;
    }

    private void EndImpactPhase()
    {
        isInImpactPhase = false;
    }

    private void EndAttack()
    {
        hitTargets.Remove(currentAttackId);
    }

    private void DetectHit(Side attackSide)
    {
        if(!hitTargets.ContainsKey(currentAttackId))
        {
            hitTargets[currentAttackId] = new HashSet<IDamageable>();
        }

        Weapon currentWeapon = (attackSide == Side.Left) ? characterController.leftWeapon : characterController.rightWeapon;
        List<Transform> attackPoints = weaponManager.GetAttackPoints(currentWeapon);
        float attackRadius = weaponManager.GetAttackRadius(currentWeapon);

        foreach (var attackPoint in attackPoints)
        {
            Collider[] hitColliders = Physics.OverlapSphere(attackPoint.position, attackRadius);
            foreach (var hitCollider in hitColliders)
            {
                IDamageable damageable = hitCollider.GetComponent<IDamageable>();
                if (damageable != null && hitCollider.gameObject != gameObject && !hitTargets[currentAttackId].Contains(damageable))
                {
                    hitTargets[currentAttackId].Add(damageable);
                    ProcessHit(damageable, attackPoint.position);
                }
            }
        }
    }

    private void ProcessHit(IDamageable target, Vector3 hitPosition)
    {
        // 检查目标是否处于防御状态
        bool isTargetDefending = (target as MonoBehaviour)?.GetComponent<DefenseHandler>()?.IsDefending ?? false;

        if (isTargetDefending)
        {
            // 处理击中防御的逻辑，例如播放防御音效或特效
            Debug.Log("Hit defended!");
        }
        else
        {
            // 应用伤害
            target.ReceiveHit(hitPosition, characterInstance.Toughness);
        }
    }

    public void EquipWeapon(Weapon weapon)
    {
        weaponManager.EquipWeapon(weapon);
        weaponController.UnsheathWeapon(weapon);
    }

    public void UnequipWeapon()
    {
        Weapon currentWeapon = characterController.rightWeapon;
        weaponManager.UnequipWeapon();
        weaponController.SheathWeapon(currentWeapon, Weapon.Unarmed);
    }

    private void OnDestroy()
    {
        // 取消订阅事件
        if (attackHandler != null)
        {
            attackHandler.OnImpactPhaseStart -= StartImpactPhase;
            attackHandler.OnImpactPhaseEnd -= EndImpactPhase;
            attackHandler.OnAttackActionEnd -= EndAttack;
        }
    }
}