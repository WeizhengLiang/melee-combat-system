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
    public CombatAnimationConfig animationConfig;


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

        if (animationConfig != null)
        {
            AnimationData.Initialize(animationConfig);
        }
        else
        {
            Debug.LogError("Combat Animation Config is not assigned in MeleeCombatSystem");
        }

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

    public void PerformAttack(int attackNumber, AttackLevel level)
    {
        if (characterController.CanStartAction(HandlerTypes.Attack))
        {
            var weaponController = GetComponent<RPGCharacterWeaponController>();
            Side currentSide = weaponController != null ? weaponManager.GetCurrentWeaponSide() : Side.None;

            characterController.StartAction(HandlerTypes.Attack, 
                new AttackContext(HandlerTypes.Attack, currentSide, attackNumber, level));
            attackHandler.StartAttack(level);
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

        foreach (var attackPoint in attackPoints)
        {
            Collider[] hitColliders = Physics.OverlapSphere(attackPoint.position, attackRadius);
            foreach (var hitCollider in hitColliders)
            {
                IDamageable damageable = hitCollider.GetComponent<IDamageable>();
                if (damageable != null && hitCollider.gameObject != gameObject && !hitTargets[currentAttackId].Contains(damageable))
                {
                    hitTargets[currentAttackId].Add(damageable);
                    ProcessHit(damageable, attackPoint.position, currentWeapon);
                }
            }
        }
    }

    private void ProcessHit(IDamageable target, Vector3 hitPosition, Weapon weapon)
    {
        MeleeCombatSystem targetSystem = (target as MonoBehaviour)?.GetComponent<MeleeCombatSystem>();
        
        if (targetSystem != null && targetSystem.isInImpactPhase)
        {
            // 检查攻击等级和时间
            if (attackHandler.CurrentAttackLevel < targetSystem.attackHandler.CurrentAttackLevel ||
                (attackHandler.CurrentAttackLevel == targetSystem.attackHandler.CurrentAttackLevel &&
                 attackHandler.AttackStartTime > targetSystem.attackHandler.AttackStartTime))
            {
                // 我方攻击被打断
                attackHandler.TryInterruptAttack(targetSystem.attackHandler.CurrentAttackLevel);
                return;
            }
            
            // 尝试打断对方攻击
            targetSystem.attackHandler.TryInterruptAttack(attackHandler.CurrentAttackLevel);
        }

        bool isTargetDefending = (target as MonoBehaviour)?.GetComponent<DefenseHandler>()?.IsDefending ?? false;
        if (isTargetDefending)
        {
            if (Random.value < combatConfig.blockChance)
            {
                Debug.Log("Hit blocked!");
                return;
            }
        }

        // ApplyKnockback(target as MonoBehaviour);
        (target as DamageHandler)?.ReceiveHit(hitPosition, attackHandler.CurrentAttackLevel);
    }

    private void ApplyKnockback(MonoBehaviour target)
    {
        if (target?.GetComponent<Rigidbody>() is Rigidbody rb)
        {
            var knockbackType = attackHandler.CurrentAttackLevel switch
            {
                AttackLevel.Light => KnockbackType.Knockback1,
                AttackLevel.Heavy => KnockbackType.Knockback2,
                _ => KnockbackType.Knockback1
            };

            Vector3 direction = AnimationData.HitDirection(knockbackType);
            // float force = combatConfig.GetKnockbackForce(attackHandler.CurrentAttackLevel);
            // rb.AddForce(direction * force, ForceMode.Impulse);
        }
    }

    private void OnDestroy()
    {
        if (attackHandler != null)
        {
            attackHandler.OnImpactPhaseStart -= StartImpactPhase;
            attackHandler.OnImpactPhaseEnd -= EndImpactPhase;
            attackHandler.OnAttackActionEnd -= EndAttack;
        }
        hitTargets.Clear();
    }
}
