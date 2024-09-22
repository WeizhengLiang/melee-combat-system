using System;
using System.Collections;
using RPGCharacterAnims;
using RPGCharacterAnims.Actions;
using RPGCharacterAnims.Lookups;
using UnityEngine;

public class AttackHandler : Attack
{
    public enum AttackPhase { None, Anticipation, Impact, Recovery }

    private RPGCharacterController characterController;
    private CharacterInstance characterInstance;

    public event Action OnImpactPhaseStart;
    public event Action OnImpactPhaseEnd;
    public event Action OnAttackActionEnd;

    private AttackPhase currentPhase = AttackPhase.None;
    public AttackPhase CurrentAttackPhase => currentPhase;
    private Side currentAttackSide;
    public Side CurrentAttackSide => currentAttackSide;
    private int currentAttackNumber;
    private bool isAttackInterrupted = false;

    [Header("Debug")]
    public bool debugMode = true;

    public void Initialize(RPGCharacterController controller, CharacterInstance character)
    {
        characterController = controller;
        characterInstance = character;
    }

    public override bool CanStartAction(RPGCharacterController controller)
    {
        return base.CanStartAction(controller) && (currentPhase == AttackPhase.None || currentPhase == AttackPhase.Recovery);
    }

    // 这些方法将由动画事件调用
    public void OnAttackAnticipationStart()
    {
        currentPhase = AttackPhase.Anticipation;
        if (debugMode) Debug.Log($"AttackHandler: Entering Anticipation phase, currentgameobject: {characterController.gameObject.name}");
    }

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

    public bool TryInterruptAttack(float attackerToughness)
    {
        switch (currentPhase)
        {
            case AttackPhase.Anticipation:
                // Anticipation 阶段总是可以被打断
                if (debugMode) Debug.Log($"TryInterruptAttack: Anticipation, currentgameobject: {characterController.gameObject.name}");
                ResetAttackPhase();
                isAttackInterrupted = true;
                return true;
            case AttackPhase.Impact:
                // Impact 阶段根据韧性决定是否可以被打断
                if (attackerToughness > characterInstance.Toughness)
                {
                    if (debugMode) Debug.Log($"TryInterruptAttack: Impact, currentgameobject: {characterController.gameObject.name}");
                    ResetAttackPhase();
                    isAttackInterrupted = true;
                    return true;
                }
                return false;
            case AttackPhase.Recovery:
                // Recovery 阶段可以被打断
                if (debugMode) Debug.Log($"TryInterruptAttack: Recovery, currentgameobject: {characterController.gameObject.name}");
                ResetAttackPhase();
                isAttackInterrupted = true;
                return true;
            default:
                return false;
        }
    }
}