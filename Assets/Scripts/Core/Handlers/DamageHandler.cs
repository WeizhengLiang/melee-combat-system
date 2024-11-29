using UnityEngine;
using RPGCharacterAnims;
using RPGCharacterAnims.Actions;
using RPGCharacterAnims.Lookups;

/// <summary>
/// Handles damage reception and hit reactions for a character.
/// Manages hit reactions based on attack levels and character states.
/// </summary>
public class DamageHandler : MonoBehaviour, IDamageable
{
    #region Component References
    private RPGCharacterController characterController;
    private RPGCharacterMovementController movementController;
    private AttackHandler attackHandler;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        attackHandler = characterController.GetHandler(HandlerTypes.Attack) as AttackHandler;
    }
    #endregion

    #region Component Initialization
    private void InitializeComponents()
    {
        characterController = GetComponent<RPGCharacterController>();
        movementController = GetComponent<RPGCharacterMovementController>();
    }
    #endregion

    #region Hit Reception
    /// <summary>
    /// Processes incoming hits and determines appropriate reactions
    /// </summary>
    /// <param name="hitPosition">World position of the hit impact</param>
    /// <param name="attackerLevel">Strength level of the incoming attack</param>
    public void ReceiveHit(Vector3 hitPosition, AttackLevel attackerLevel)
    {
        if (attackHandler == null) return;

        // Only process hit reaction when not attacking or when attack is interrupted
        if (attackHandler.CurrentAttackPhase == AttackHandler.AttackPhase.None || 
            attackHandler.IsAttackInterrupted)
        {
            ProcessHitReaction(hitPosition, attackerLevel);
        }
    }

    /// <summary>
    /// Determines and applies the appropriate hit reaction based on attack level
    /// </summary>
    private void ProcessHitReaction(Vector3 hitPosition, AttackLevel attackerLevel)
    {
        Vector3 hitDirection = CalculateHitDirection(hitPosition);
        float force = GetHitForce(attackerLevel);
        float variableForce = force * 0.2f;

        if (attackerLevel == AttackLevel.Heavy)
        {
            ApplyKnockdown(hitDirection, force, variableForce);
        }
        else
        {
            ApplyKnockback(hitDirection, force, variableForce, attackerLevel);
        }
    }
    #endregion

    #region Hit Calculations
    /// <summary>
    /// Calculates the direction of the hit relative to the character
    /// </summary>
    private Vector3 CalculateHitDirection(Vector3 hitPosition)
    {
        return (transform.position - hitPosition).normalized;
    }

    /// <summary>
    /// Determines the force of the hit based on attack level
    /// </summary>
    private float GetHitForce(AttackLevel attackerLevel)
    {
        return attackerLevel switch
        {
            AttackLevel.Light => 2f,
            AttackLevel.Medium => 3f,
            AttackLevel.Heavy => 4f,
            _ => 2f
        };
    }
    #endregion

    #region Hit Reactions
    /// <summary>
    /// Applies knockdown reaction for heavy attacks
    /// </summary>
    private void ApplyKnockdown(Vector3 hitDirection, float force, float variableForce)
    {
        if (!characterController.HandlerExists(HandlerTypes.Knockdown)) return;

        characterController.StartAction(HandlerTypes.Knockdown, 
            new HitContext((int)KnockdownType.Knockdown1, hitDirection, force, variableForce));
    }

    /// <summary>
    /// Applies knockback reaction for light and medium attacks
    /// </summary>
    private void ApplyKnockback(Vector3 hitDirection, float force, float variableForce, AttackLevel attackerLevel)
    {
        if (!characterController.HandlerExists(HandlerTypes.Knockback)) return;

        KnockbackType knockbackType = attackerLevel == AttackLevel.Medium ? 
            KnockbackType.Knockback2 : KnockbackType.Knockback1;

        characterController.StartAction(HandlerTypes.Knockback, 
            new HitContext((int)knockbackType, hitDirection, force, variableForce));
    }
    #endregion
}
