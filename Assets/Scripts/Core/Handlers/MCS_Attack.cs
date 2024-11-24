using System;
using System.Collections;
using RPGCharacterAnims;
using RPGCharacterAnims.Actions;
using RPGCharacterAnims.Lookups;
using UnityEngine;

/// <summary>
/// Handles the character's attack states, phase transitions, and interruption logic.
/// Inherits from base Attack class to implement specific attack behaviors and state management.
/// </summary>
public class MCS_Attack : Attack
{
    /// <summary>
    /// Defines the phases of an attack action:
    /// None: Not in attack state
    /// Anticipation: Preparation phase, can be interrupted by equal or higher level attacks
    /// Impact: Strike phase, can only be interrupted by higher level attacks
    /// Recovery: End phase, can be interrupted by any attack
    /// </summary>
    public enum AttackPhase { None, Anticipation, Impact, Recovery }

    private RPGCharacterController characterController;
    private MeleeCombatSystemConfig combatSystemConfig;

    /// <summary>
    /// Events for notifying other systems about attack state changes
    /// </summary>
    public event Action OnImpactPhaseStart;  // Triggered when entering impact phase
    public event Action OnImpactPhaseEnd;    // Triggered when exiting impact phase
    public event Action OnAttackActionEnd;   // Triggered when attack action completes

    // Attack state properties
    private AttackPhase currentPhase = AttackPhase.None;
    public AttackPhase CurrentAttackPhase => currentPhase;
    private Side currentAttackSide;
    public Side CurrentAttackSide => currentAttackSide;
    private int currentAttackNumber;
    private bool isAttackInterrupted = false;
    public bool IsAttackInterrupted => isAttackInterrupted;

    /// <summary>
    /// Attack level properties: Used for determining attack priority and interruption logic
    /// </summary>
    private AttackLevel currentAttackLevel;
    public AttackLevel CurrentAttackLevel => currentAttackLevel;
    private float attackStartTime;           // Records attack start time for handling same-level attack priority
    public float AttackStartTime => attackStartTime;

    private KnockbackType currentKnockbackType;
    public KnockbackType CurrentKnockbackType => currentKnockbackType;

    [Header("Debug")]
    public bool debugMode = true;

    /// <summary>
    /// Constructor: Initializes the attack handler with required dependencies
    /// </summary>
    /// <param name="controller">Reference to the character controller</param>
    /// <param name="combatConfig">Combat system configuration</param>
    public MCS_Attack(RPGCharacterController controller)
    {
        characterController = controller;
    }

    /// <summary>
    /// Checks if a new attack action can be started
    /// Conditions: Base class allows attack AND (No current attack OR In recovery phase)
    /// </summary>
    public override bool CanStartAction(RPGCharacterController controller)
    {
        return base.CanStartAction(controller) && (currentPhase == AttackPhase.None || currentPhase == AttackPhase.Recovery);
    }

    /// <summary>
    /// Internal implementation of attack action start
    /// Sets up animation data and related properties
    /// </summary>
    protected override void _StartAction(RPGCharacterController controller, AttackContext context)
    {
        var attackType = (AttackAnimationType)(context.number - 1);
        var animData = AnimationData.GetAttackData(attackType);
        if (animData != null)
        {
            currentAttackNumber = animData.legacyAnimationNumber;
            currentAttackLevel = animData.attackLevel;
            currentKnockbackType = animData.knockbackType;
        }
        base._StartAction(controller, context);
    }

    // The following methods are called by animation events to manage attack phases

    /// <summary>
    /// Called when entering anticipation phase
    /// This phase can be interrupted by equal or higher level attacks
    /// </summary>
    public void OnAttackAnticipationStart()
    {
        currentPhase = AttackPhase.Anticipation;
        if (debugMode) Debug.Log($"AttackHandler: Entering Anticipation phase, currentgameobject: {characterController.gameObject.name}");
    }

    /// <summary>
    /// Called when entering impact phase
    /// Skips if attack was interrupted
    /// </summary>
    public void OnAttackImpactStart()
    {
        if (isAttackInterrupted)
        {
            if (debugMode) Debug.Log($"Attack was interrupted, skipping Impact phase, currentgameobject: {characterController.gameObject.name}");
            return;
        }
        currentPhase = AttackPhase.Impact;
        if (debugMode) Debug.Log($"AttackHandler: Entering Impact phase, currentgameobject: {characterController.gameObject.name}");
        OnImpactPhaseStart?.Invoke();
    }

    /// <summary>
    /// Called when entering recovery phase
    /// This phase can be interrupted by any attack
    /// </summary>
    public void OnAttackRecoveryStart()
    {
        currentPhase = AttackPhase.Recovery;
        if (debugMode) Debug.Log($"AttackHandler: Entering Recovery phase, currentgameobject: {characterController.gameObject.name}");
        OnImpactPhaseEnd?.Invoke();
    }

    public void OnAttackEnd()
    {
        if (debugMode) Debug.Log($"AttackHandler: Entering Attack end, currentgameobject: {characterController.gameObject.name}");
        ResetAttackPhase();
        ResetInterruptFlag();
        OnAttackActionEnd?.Invoke();
    }

    protected override void _EndAction(RPGCharacterController controller)
    {
        
    }

    private void ResetAttackPhase()
    {
        if (debugMode) Debug.Log($"ResetAttackPhase, currentgameobject: {characterController.gameObject.name}");
        currentPhase = AttackPhase.None;
        characterController.Unlock(true, true);
    }

    public void ResetInterruptFlag()
    {
        isAttackInterrupted = false;
    }
    /// <summary>
    /// Attempts to interrupt the current attack based on phase and attacker level
    /// </summary>
    /// <param name="attackerLevel">The level of the incoming attack</param>
    /// <returns>True if attack was successfully interrupted</returns>
    public bool TryInterruptAttack(AttackLevel attackerLevel)
    {
        switch (currentPhase)
        {
            case AttackPhase.Anticipation:
                // In anticipation phase, can be interrupted by equal or higher level attacks
                if (attackerLevel >= currentAttackLevel)
                {
                    if (debugMode) Debug.Log($"TryInterruptAttack: Anticipation, currentgameobject: {characterController.gameObject.name}");
                    ResetAttackPhase();
                    isAttackInterrupted = true;
                    return true;
                }
                return false;
            case AttackPhase.Impact:
                // In impact phase, can only be interrupted by higher level attacks
                if (attackerLevel > currentAttackLevel)
                {
                    if (debugMode) Debug.Log($"TryInterruptAttack: Impact, currentgameobject: {characterController.gameObject.name}");
                    ResetAttackPhase();
                    isAttackInterrupted = true;
                    return true;
                }
                return false;
            case AttackPhase.Recovery:
                // Recovery phase can be interrupted by any attack
                if (debugMode) Debug.Log($"TryInterruptAttack: Recovery, currentgameobject: {characterController.gameObject.name}");
                ResetAttackPhase();
                isAttackInterrupted = true;
                return true;
            default:
                return false;
        }
    }

    public void StartAttack(AttackLevel level)
    {
        currentAttackLevel = level;
        attackStartTime = Time.time;
    }
}