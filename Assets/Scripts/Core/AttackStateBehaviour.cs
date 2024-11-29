using UnityEngine;
using RPGCharacterAnims;
using RPGCharacterAnims.Lookups;

/// <summary>
/// Handles attack animation state behavior in the animator state machine.
/// Manages attack phase transitions and interruption handling.
/// </summary>
public class AttackStateBehaviour : StateMachineBehaviour
{
    #region Fields
    private AttackHandler attackHandler;
    private bool debugMode;
    #endregion

    #region State Machine Callbacks
    /// <summary>
    /// Called when entering the attack animation state
    /// Initializes attack handler if not already initialized
    /// </summary>
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (attackHandler == null)
        {
            InitializeAttackHandler(animator);
        }
    }

    /// <summary>
    /// Called when exiting the attack animation state
    /// Handles attack interruption and cleanup
    /// </summary>
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (attackHandler == null) return;

        // If still in Recovery phase when exiting, the attack was interrupted
        if (attackHandler.CurrentAttackPhase == AttackHandler.AttackPhase.Recovery)
        {
            LogDebug($"Attack interrupted during Recovery phase: {animator.gameObject.name}");
            attackHandler.OnAttackEnd();
        }
    }
    #endregion

    #region Initialization
    /// <summary>
    /// Initializes the attack handler component from the character controller
    /// </summary>
    private void InitializeAttackHandler(Animator animator)
    {
        var characterController = animator.GetComponentInParent<RPGCharacterController>();
        if (characterController == null)
        {
            LogError($"RPGCharacterController not found on {animator.gameObject.name}");
            return;
        }

        attackHandler = characterController.GetHandler(HandlerTypes.Attack) as AttackHandler;
        if (attackHandler == null)
        {
            LogError($"AttackHandler not found on {animator.gameObject.name}");
        }
    }
    #endregion

    #region Utility
    private void LogDebug(string message)
    {
        if (debugMode)
        {
            Debug.Log($"[AttackStateBehaviour] {message}");
        }
    }

    private void LogError(string message)
    {
        Debug.LogError($"[AttackStateBehaviour] {message}");
    }
    #endregion
}