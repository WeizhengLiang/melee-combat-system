using UnityEngine;
using RPGCharacterAnims;
using RPGCharacterAnims.Lookups;
using RPGCharacterAnims.Actions;
using System.Collections.Generic;

public class MeleeCombatSystem : MonoBehaviour
{
    public MeleeCombatSystemConfig combatConfig;
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
            attackHandler.ResetInterruptFlag();
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
                    ProcessHit(damageable, attackPoint.position, characterController.rightWeapon);
                }
            }
        }
    }

    private void ProcessHit(IDamageable target, Vector3 hitPosition, Weapon weapon)
    {
        bool isTargetDefending = (target as MonoBehaviour)?.GetComponent<DefenseHandler>()?.IsDefending ?? false;

        if (isTargetDefending)
        {
            if (Random.value < combatConfig.blockChance)
            {
                Debug.Log("Hit blocked!");
                return;
            }
        }

        // float damage = CalculateDamage(weapon);
        float damage = 0;
        target.ReceiveHit(hitPosition, damage, combatConfig.toughness);
    }

    // private float CalculateDamage(Weapon weapon)
    // {
    //     MeleeCombatSystemConfig.WeaponData weaponData = combatConfig.GetWeaponData(weapon);
    //     float baseDamage = combatConfig.baseAttackDamage * weaponData.damageMultiplier;
    //
    //     if (Random.value < combatConfig.criticalHitChance)
    //     {
    //         baseDamage *= combatConfig.criticalHitMultiplier;
    //         Debug.Log("Critical hit!");
    //     }
    //
    //     return baseDamage;
    // }

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