using UnityEngine;
using RPGCharacterAnims;
using RPGCharacterAnims.Lookups;
using RPGCharacterAnims.Actions;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Core combat system that handles melee combat mechanics including:
/// - Attack execution and combo system
/// - Hit detection and processing
/// - Combat state management (blocking, dodging)
/// - Weapon-based combat interactions
/// </summary>
public class MeleeCombatSystem : MonoBehaviour
{
    #region Component References
    private RPGCharacterController characterController;
    private RPGCharacterWeaponController weaponController;
    private AttackHandler attackHandler;
    private WeaponManager weaponManager;
    #endregion

    #region Configuration
    [Header("Combat Configuration")]
    [Tooltip("Configuration for combat animations and timings")]
    public CombatAnimationConfig animationConfig;

    [Header("Debug Settings")]
    [Tooltip("Enable debug logging")]
    public bool debugMode;
    #endregion

    #region Combat State
    private bool isInImpactPhase;
    private int currentAttackId;
    private bool isBlocking;
    private bool isDodging;
    
    // Tracks which targets have been hit during the current attack
    private readonly Dictionary<int, HashSet<IDamageable>> hitTargets = new();
    
    public bool IsBlocking => isBlocking;
    #endregion

    #region Initialization
    private void Start()
    {
        if (!TryInitializeComponents()) return;
        SetupEventListeners();
        ValidateConfiguration();
    }

    private bool TryInitializeComponents()
    {
        characterController = GetComponent<RPGCharacterController>();
        weaponController = GetComponent<RPGCharacterWeaponController>();
        weaponManager = GetComponent<WeaponManager>();
        attackHandler = characterController.GetHandler(HandlerTypes.Attack) as AttackHandler;

        if (attackHandler == null)
        {
            Debug.LogError("[MeleeCombatSystem] Failed to initialize: AttackHandler component not found");
            enabled = false;
            return false;
        }

        return true;
    }

    private void SetupEventListeners()
    {
        attackHandler.OnImpactPhaseStart += StartImpactPhase;
        attackHandler.OnImpactPhaseEnd += EndImpactPhase;
        attackHandler.OnAttackActionEnd += EndAttackHandler;
    }

    private void ValidateConfiguration()
    {
        if (animationConfig == null)
        {
            Debug.LogError("[MeleeCombatSystem] Combat Animation Config is missing");
            enabled = false;
            return;
        }
        AnimationData.Initialize(animationConfig);
    }
    #endregion

    #region Combat Actions
    /// <summary>
    /// Executes an attack with the specified parameters if conditions are met
    /// </summary>
    public void PerformAttack(int attackNumber, AttackLevel level)
    {
        if (!CanPerformAttack()) return;

        Side attackSide = DetermineAttackSide();
        ExecuteAttack(attackNumber, level, attackSide);
    }

    private bool CanPerformAttack()
    {
        return characterController.CanStartAction(HandlerTypes.Attack);
    }

    private Side DetermineAttackSide()
    {
        return weaponManager?.GetCurrentWeaponSide() ?? Side.None;
    }

    private void ExecuteAttack(int attackNumber, AttackLevel level, Side side)
    {
        var context = new AttackContext(HandlerTypes.Attack, side, attackNumber, level);
        characterController.StartAction(HandlerTypes.Attack, context);
        attackHandler.StartAttack(level);
    }

    /// <summary>
    /// Initiates blocking state if conditions are met
    /// </summary>
    public void PerformBlock()
    {
        if (!characterController.CanStartAction(HandlerTypes.Block)) return;
        
        isBlocking = true;
        characterController.StartAction(HandlerTypes.Block);
    }

    /// <summary>
    /// Ends blocking state
    /// </summary>
    public void EndBlock()
    {
        if (!isBlocking) return;
        
        isBlocking = false;
        characterController.EndAction(HandlerTypes.Block);
    }

    /// <summary>
    /// Executes a dodge action if conditions are met
    /// </summary>
    public void PerformDodge()
    {
        if (!CanPerformDodge()) return;

        ExecuteDodge();
    }

    private bool CanPerformDodge()
    {
        return characterController.CanStartAction(HandlerTypes.Dodge) && !isDodging;
    }

    private void ExecuteDodge()
    {
        isDodging = true;
        characterController.Dodge(DodgeType.Backward);
        StartCoroutine(EndDodgeCoroutine());
    }
    #endregion

    #region Hit Detection
    private void Update()
    {
        if (isInImpactPhase)
        {
            DetectHit(attackHandler.CurrentAttackSide);
        }
    }

    private void DetectHit(Side attackSide)
    {
        EnsureHitTargetCollection();
        
        var weaponInfo = GetCurrentWeaponInfo(attackSide);
        foreach (var attackPoint in weaponInfo.attackPoints)
        {
            CheckHitAtPoint(attackPoint, weaponInfo.attackRadius, weaponInfo.weapon);
        }
    }

    private void EnsureHitTargetCollection()
    {
        if (!hitTargets.ContainsKey(currentAttackId))
        {
            hitTargets[currentAttackId] = new HashSet<IDamageable>();
        }
    }

    private (Weapon weapon, List<Transform> attackPoints, float attackRadius) GetCurrentWeaponInfo(Side attackSide)
    {
        Weapon weapon = (attackSide == Side.Left) ? characterController.leftWeapon : characterController.rightWeapon;
        return (weapon,
            weaponManager.GetAttackPoints(weapon),
            weaponManager.GetAttackRadius(weapon));
    }

    private void CheckHitAtPoint(Transform attackPoint, float attackRadius, Weapon weapon)
    {
        var hitColliders = Physics.OverlapSphere(attackPoint.position, attackRadius);
        foreach (var hitCollider in hitColliders)
        {
            ProcessPotentialTarget(hitCollider, attackPoint.position, weapon);
        }
    }
    #endregion

    #region Hit Processing
    private void ProcessPotentialTarget(Collider hitCollider, Vector3 hitPosition, Weapon weapon)
    {
        var damageable = hitCollider.GetComponent<IDamageable>();
        if (!IsValidTarget(damageable, hitCollider.gameObject)) return;

        hitTargets[currentAttackId].Add(damageable);
        ProcessHit(damageable, hitPosition, weapon);
    }

    private bool IsValidTarget(IDamageable damageable, GameObject hitObject)
    {
        return damageable != null && 
               hitObject != gameObject && 
               !hitTargets[currentAttackId].Contains(damageable);
    }

    private void ProcessHit(IDamageable target, Vector3 hitPosition, Weapon weapon)
    {
        var targetBehaviour = target as MonoBehaviour;
        if (targetBehaviour == null) return;

        var targetSystem = targetBehaviour.GetComponent<MeleeCombatSystem>();
        if (targetSystem != null && ProcessCombatInteraction(targetSystem)) return;

        if (IsTargetBlocking(targetBehaviour))
        {
            LogDebug("Hit blocked!");
            return;
        }

        ApplyDamage(target, hitPosition);
    }

    private bool IsTargetBlocking(MonoBehaviour targetBehaviour)
    {
        return targetBehaviour.GetComponent<DefenseHandler>()?.IsDefending ?? false;
    }

    private void ApplyDamage(IDamageable target, Vector3 hitPosition)
    {
        (target as DamageHandler)?.ReceiveHit(hitPosition, attackHandler.CurrentAttackLevel);
    }
    #endregion

    #region Combat Interaction
    private bool ProcessCombatInteraction(MeleeCombatSystem targetSystem)
    {
        if (targetSystem.isDodging) return true;
        if (targetSystem.IsBlocking)
        {
            attackHandler.TryInterruptAttack(AttackLevel.Heavy);
            return true;
        }
        if (targetSystem.isInImpactPhase)
        {
            return HandleAttackClash(targetSystem);
        }
        return false;
    }

    private bool HandleAttackClash(MeleeCombatSystem targetSystem)
    {
        bool shouldInterruptCurrentAttack = ShouldInterruptAttack(targetSystem);
        
        if (shouldInterruptCurrentAttack)
        {
            attackHandler.TryInterruptAttack(targetSystem.attackHandler.CurrentAttackLevel);
        }
        else
        {
            targetSystem.attackHandler.TryInterruptAttack(attackHandler.CurrentAttackLevel);
        }
        
        return shouldInterruptCurrentAttack;
    }

    private bool ShouldInterruptAttack(MeleeCombatSystem targetSystem)
    {
        return attackHandler.CurrentAttackLevel < targetSystem.attackHandler.CurrentAttackLevel ||
               (attackHandler.CurrentAttackLevel == targetSystem.attackHandler.CurrentAttackLevel &&
                attackHandler.AttackStartTime > targetSystem.attackHandler.AttackStartTime);
    }
    #endregion

    #region Phase Management
    private void StartImpactPhase()
    {
        currentAttackId++;
        isInImpactPhase = true;
    }

    private void EndImpactPhase()
    {
        isInImpactPhase = false;
    }

    private void EndAttackHandler()
    {
        hitTargets.Remove(currentAttackId);
    }

    private IEnumerator EndDodgeCoroutine()
    {
        yield return new WaitForSeconds(0.55f);
        isDodging = false;
    }
    #endregion

    #region Utility
    private void LogDebug(string message)
    {
        if (debugMode)
        {
            Debug.Log($"[MeleeCombatSystem] {message}");
        }
    }
    #endregion

    #region Cleanup
    private void OnDestroy()
    {
        if (attackHandler != null)
        {
            attackHandler.OnImpactPhaseStart -= StartImpactPhase;
            attackHandler.OnImpactPhaseEnd -= EndImpactPhase;
            attackHandler.OnAttackActionEnd -= EndAttackHandler;
        }
        hitTargets.Clear();
    }
    #endregion
}
