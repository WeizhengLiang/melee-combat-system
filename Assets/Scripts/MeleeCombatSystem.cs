using UnityEngine;
using RPGCharacterAnims;
using RPGCharacterAnims.Lookups;
using RPGCharacterAnims.Actions;
using System.Collections.Generic;
using System.Linq;

public class MeleeCombatSystem : MonoBehaviour
{
    public MeleeCombatSystemConfig combatConfig;
    private RPGCharacterController characterController;
    private RPGCharacterWeaponController weaponController;
    private AttackHandler attackHandler;
    private WeaponManager weaponManager;

    private bool isInImpactPhase = false;

    private int currentAttackId = 0;
    private Dictionary<int, HashSet<IDamageable>> hitTargets = new Dictionary<int, HashSet<IDamageable>>();

    private void Start()
    {
        characterController = GetComponent<RPGCharacterController>();
        weaponController = GetComponent<RPGCharacterWeaponController>();
        weaponManager = GetComponent<WeaponManager>();
        attackHandler = characterController.GetHandler(HandlerTypes.Attack) as AttackHandler;

        if (attackHandler == null)
        {
            Debug.LogError("AttackHandler not found in RPGCharacterController");
            return;
        }

        // 订阅 AttackHandler 的事件
        attackHandler.OnImpactPhaseStart += StartImpactPhase;
        attackHandler.OnImpactPhaseEnd += EndImpactPhase;
        attackHandler.OnAttackActionEnd += EndAttack;

        // 订阅武器变化事件（假设 RPGCharacterController 有这样的事件）
        // characterController.OnWeaponChanged += OnWeaponChanged;
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

    public void PerformBlock()
    {
        characterController.StartAction("Block");
    }
    
    public void PerformDodge()
    {
        characterController.StartAction("Dodge");
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
        Debug.Log($"EndAttack called, currentAttackId: {currentAttackId}, hitTargets count: {hitTargets.Count}");
        hitTargets.Remove(currentAttackId);
        Debug.Log($"After removal, hitTargets count: {hitTargets.Count}");
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

        Debug.Log($"Current weapon: {currentWeapon}, Attack points: {attackPoints.Count}, Attack radius: {attackRadius}");
        foreach (var attackPoint in attackPoints)
        {
            Debug.Log($"Attack point: {attackPoint.name}, Attack point position: {attackPoint.position}, Attack radius: {attackRadius}");
            Collider[] hitColliders = Physics.OverlapSphere(attackPoint.position, attackRadius);
            foreach (var hitCollider in hitColliders)
            {
                Debug.Log($"Detected collider: {hitCollider.name}");
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
        Debug.Log($"ProcessHit called for {target}, weapon: {weapon}");

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

    private void OnWeaponChanged(Weapon newWeapon)
    {
        // 根据新武器更新 MeleeCombatSystem 的状态
        // 例如，更新攻击点、攻击半径等
        UpdateWeaponProperties(newWeapon);
    }

    private void UpdateWeaponProperties(Weapon weapon)
    {
        // 更新与武器相关的属性
        // 例如：
        // attackRadius = weaponManager.GetAttackRadius(weapon);
        // attackPoints = weaponManager.GetAttackPoints(weapon);
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

        // 取消订阅武器变化事件
        // if (characterController != null)
        // {
        //     characterController.OnWeaponChanged -= OnWeaponChanged;
        // }
    }
}
