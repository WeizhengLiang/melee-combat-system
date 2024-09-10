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

    public event Action OnImpactPhaseStart;
    public event Action OnImpactPhaseEnd;

    private AttackPhase currentPhase = AttackPhase.None;
    private Side currentAttackSide;
    public Side CurrentAttackSide => currentAttackSide;
    private int currentAttackNumber;

    [Header("Debug")]
    public bool debugMode = true;

    public void Initialize(RPGCharacterController controller)
    {
        characterController = controller;
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
        EndAction(characterController);
    }
}