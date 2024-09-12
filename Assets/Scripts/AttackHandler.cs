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

    private AttackPhase currentPhase = AttackPhase.None;
    public AttackPhase CurrentAttackPhase => currentPhase;
    private Side currentAttackSide;
    public Side CurrentAttackSide => currentAttackSide;
    private int currentAttackNumber;

    [Header("Debug")]
    public bool debugMode = false;

    public void Initialize(RPGCharacterController controller, CharacterInstance character)
    {
        characterController = controller;
        characterInstance = character;
    }

    public override bool CanStartAction(RPGCharacterController controller)
    {
        if (characterController.gameObject.name == "RPG-Character")
        {
            
        }
        return base.CanStartAction(controller) && (currentPhase == AttackPhase.None || currentPhase == AttackPhase.Recovery);
    }

    // 这些方法将由动画事件调用
    public void OnAttackAnticipationStart()
    {
        currentPhase = AttackPhase.Anticipation;
        if (debugMode) Debug.Log("AttackHandler: Entering Anticipation phase");
    }

    public void OnAttackImpactStart()
    {
        currentPhase = AttackPhase.Impact;
        if (debugMode) Debug.Log("AttackHandler: Entering Impact phase");
        OnImpactPhaseStart?.Invoke();
    }

    public void OnAttackRecoveryStart()
    {
        currentPhase = AttackPhase.Recovery;
        if (debugMode) Debug.Log("AttackHandler: Entering Recovery phase");
        OnImpactPhaseEnd?.Invoke();
    }

    public void OnAttackEnd()
    {
        ResetAttackPhase();
    }

    protected override void _EndAction(RPGCharacterController controller)
    {
        
    }

    private void ResetAttackPhase()
    {
        currentPhase = AttackPhase.None;
        characterController.Unlock(true, true);
    }

    public bool TryInterruptAttack(float attackerToughness)
    {
        switch (currentPhase)
        {
            case AttackPhase.Anticipation:
                // Anticipation 阶段总是可以被打断
                ResetAttackPhase();
                return true;
            case AttackPhase.Impact:
                // Impact 阶段根据韧性决定是否可以被打断
                if (attackerToughness > characterInstance.Toughness)
                {
                    ResetAttackPhase();
                    return true;
                }
                return false;
            case AttackPhase.Recovery:
                // Recovery 阶段可以被打断
                ResetAttackPhase();
                return true;
            default:
                return false;
        }
    }
}