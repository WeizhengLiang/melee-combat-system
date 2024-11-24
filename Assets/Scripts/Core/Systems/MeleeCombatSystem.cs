using UnityEngine;
using RPGCharacterAnims;
using RPGCharacterAnims.Lookups;
using RPGCharacterAnims.Actions;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class MeleeCombatSystem : MonoBehaviour
{
    // public MeleeCombatSystemConfig combatConfig;
    private RPGCharacterController characterController;
    private RPGCharacterWeaponController weaponController;
    private MCS_Attack mcsAttack;
    private WeaponManager weaponManager;
    public CombatAnimationConfig animationConfig;


    private bool isInImpactPhase = false;
    private int currentAttackId = 0;
    private Dictionary<int, HashSet<IDamageable>> hitTargets = new Dictionary<int, HashSet<IDamageable>>();
    private bool isBlocking = false;
    public bool IsBlocking => isBlocking;

    private bool isDodging = false;

    private void Start()
    {
        characterController = GetComponent<RPGCharacterController>();
        weaponController = GetComponent<RPGCharacterWeaponController>();
        weaponManager = GetComponent<WeaponManager>();
        mcsAttack = characterController.GetHandler(HandlerTypes.Attack) as MCS_Attack;

        if (mcsAttack == null)
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

        mcsAttack.OnImpactPhaseStart += StartImpactPhase;
        mcsAttack.OnImpactPhaseEnd += EndImpactPhase;
        mcsAttack.OnAttackActionEnd += EndMcsAttack;
    }

    private void Update()
    {
        if (isInImpactPhase)
        {
            DetectHit(mcsAttack.CurrentAttackSide);
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
            mcsAttack.StartAttack(level);
        }
    }

    public void PerformBlock()
    {
        if (!characterController.CanStartAction(HandlerTypes.Block)) return;
        
        isBlocking = true;
        characterController.StartAction(HandlerTypes.Block);
    }

    public void EndBlock()
    {
        if (isBlocking)
        {
            isBlocking = false;
            characterController.EndAction(HandlerTypes.Block);
        }
    }

    public void PerformDodge()
    {
        if (!characterController.CanStartAction(HandlerTypes.Dodge) || isDodging) return;

        isDodging = true;
        characterController.Dodge(DodgeType.Backward);
        StartCoroutine(EndDodgeCoroutine());
    }

    private IEnumerator EndDodgeCoroutine()
    {
        yield return new WaitForSeconds(0.55f);
        isDodging = false;
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

    private void EndMcsAttack()
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
        
        if (targetSystem != null)
        {
            // 如果目标正在闪避，攻击无效
            if (targetSystem.isDodging) return;

            // 如果目标正在格挡
            if (targetSystem.IsBlocking)
            {
                // 攻击被格挡，打断攻击
                mcsAttack.TryInterruptAttack(AttackLevel.Heavy);
                return;
            }

            if (targetSystem.isInImpactPhase)
            {
                // 检查攻击等级和时间
                if (mcsAttack.CurrentAttackLevel < targetSystem.mcsAttack.CurrentAttackLevel ||
                    (mcsAttack.CurrentAttackLevel == targetSystem.mcsAttack.CurrentAttackLevel &&
                     mcsAttack.AttackStartTime > targetSystem.mcsAttack.AttackStartTime))
                {
                    // 我方攻击被打断
                    mcsAttack.TryInterruptAttack(targetSystem.mcsAttack.CurrentAttackLevel);
                    return;
                }
                
                // 尝试打断对方攻击
                targetSystem.mcsAttack.TryInterruptAttack(mcsAttack.CurrentAttackLevel);
            }
        }

        bool isTargetDefending = (target as MonoBehaviour)?.GetComponent<DefenseHandler>()?.IsDefending ?? false;
        if (isTargetDefending)
        {
            // if (Random.value < combatConfig.blockChance)
            // {
                Debug.Log("Hit blocked!");
                return;
            // }
        }

        // ApplyKnockback(target as MonoBehaviour);
        (target as DamageHandler)?.ReceiveHit(hitPosition, mcsAttack.CurrentAttackLevel);
    }

    private void ApplyKnockback(MonoBehaviour target)
    {
        if (target?.GetComponent<Rigidbody>() is Rigidbody rb)
        {
            var knockbackType = mcsAttack.CurrentAttackLevel switch
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
        if (mcsAttack != null)
        {
            mcsAttack.OnImpactPhaseStart -= StartImpactPhase;
            mcsAttack.OnImpactPhaseEnd -= EndImpactPhase;
            mcsAttack.OnAttackActionEnd -= EndMcsAttack;
        }
        hitTargets.Clear();
    }
}
