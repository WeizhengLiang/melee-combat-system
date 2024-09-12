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
        if (characterController.CanStartAction(HandlerTypes.Attack) && attackHandler.CanStartAction(characterController))
        {
            characterController.StartAction(HandlerTypes.Attack, new AttackContext(HandlerTypes.Attack, attackSide, attackNumber));
        }
    }

    private void StartImpactPhase()
    {
        isInImpactPhase = true;
    }

    private void EndImpactPhase()
    {
        isInImpactPhase = false;
    }

    private void DetectHit(Side attackSide)
    {
        Weapon currentWeapon = (attackSide == Side.Left) ? characterController.leftWeapon : characterController.rightWeapon;
        List<Transform> attackPoints = weaponManager.GetAttackPoints(currentWeapon);
        float attackRadius = weaponManager.GetAttackRadius(currentWeapon);

        foreach (var attackPoint in attackPoints)
        {
            Collider[] hitColliders = Physics.OverlapSphere(attackPoint.position, attackRadius);
            foreach (var hitCollider in hitColliders)
            {
                IDamageable damageable = hitCollider.GetComponent<IDamageable>();
                if (damageable != null && hitCollider.gameObject != gameObject)
                {
                    damageable.ReceiveHit(attackPoint.position, characterInstance.Toughness);
                }
            }
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
        }
    }
}