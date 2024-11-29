using UnityEngine;
using RPGCharacterAnims;
using RPGCharacterAnims.Actions;
using RPGCharacterAnims.Lookups;

/// <summary>
/// Handles melee combat input and manages combat state including combos
/// </summary>
public class MeleeCombatInput : MonoBehaviour
{
    /// <summary>
    /// Delegate and event for weapon toggle actions
    /// </summary>
    public delegate void WeaponToggleEventHandler();
    public static event WeaponToggleEventHandler OnWeaponToggle;

    private MeleeCombatSystem meleeCombatSystem;
    private WeaponManager weaponManager;
    private RPGCharacterController characterController;
    private float lastAttackTime;
    private int comboCount;
    private float comboTimeout;  // Dynamic combo timeout duration
    private AttackLevel currentAttackLevel = AttackLevel.Light;
    private Side currentAttackSide = Side.Right;

    /// <summary>
    /// Initializes components and combat settings
    /// </summary>
    private void Start()
    {
        meleeCombatSystem = GetComponent<MeleeCombatSystem>();
        weaponManager = GetComponent<WeaponManager>();
        characterController = GetComponent<RPGCharacterController>();
        
        // Initialize combo timeout as 1.5x the longest attack animation duration
        comboTimeout = GetMaxAttackDuration() * 1.5f;
        lastAttackTime = -comboTimeout;
        comboCount = 0;
    }

    /// <summary>
    /// Gets the duration of the longest attack animation
    /// </summary>
    private float GetMaxAttackDuration()
    {
        float maxDuration = 0f;
        foreach (AttackAnimationType type in System.Enum.GetValues(typeof(AttackAnimationType)))
        {
            float duration = AnimationData.AttackDuration(type);
            if (duration > maxDuration)
            {
                maxDuration = duration;
            }
        }
        return maxDuration > 0 ? maxDuration : 1.0f;
    }

    private void Update()
    {
        CustomMeleeCombatInputs();
    }

    /// <summary>
    /// Processes combat-related inputs including attacks, blocks, and dodges
    /// </summary>
    private void CustomMeleeCombatInputs()
    {
        if (Input.GetKeyDown(KeyCode.J)) 
        {   
            HandleAttackInput();
        }

        if (Input.GetKeyDown(KeyCode.K)) 
        {   
            meleeCombatSystem.PerformBlock();
        }
        else if (Input.GetKeyUp(KeyCode.K))
        {
            meleeCombatSystem.EndBlock();
        }
        
        if (Input.GetKeyDown(KeyCode.H)) 
        {   
            meleeCombatSystem.PerformDodge();
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            OnWeaponToggle?.Invoke();
        }

        // Reset combo if timeout exceeded
        if (Time.time - lastAttackTime > comboTimeout)
        {
            ResetCombo();
        }
    }

    /// <summary>
    /// Handles attack input and combo system
    /// </summary>
    private void HandleAttackInput()
    {
        if (!characterController.CanStartAction(HandlerTypes.Attack))
        {
            return;
        }

        var attackData = GetAttackDataFromCombo();
        if (attackData != null)
        {
            meleeCombatSystem.PerformAttack(attackData.legacyAnimationNumber, attackData.attackLevel);
        }
        UpdateComboState();
    }

    /// <summary>
    /// Updates the combo state and timing
    /// </summary>
    private void UpdateComboState()
    {
        if (Time.time - lastAttackTime <= comboTimeout)
        {
            comboCount = (comboCount + 1) % GetMaxComboCount();
            lastAttackTime = Time.time;
        }
        else
        {
            ResetCombo();
        }
    }

    /// <summary>
    /// Gets attack data based on current combo state
    /// </summary>
    private AttackAnimationData GetAttackDataFromCombo()
    {
        currentAttackLevel = GetAttackLevelFromCombo(comboCount);
        var attackType = GetAttackTypeFromCombo(comboCount, currentAttackLevel);
        return AnimationData.GetAttackData(attackType);
    }

    /// <summary>
    /// Determines attack type based on combo count and current weapon
    /// </summary>
    private AttackAnimationType GetAttackTypeFromCombo(int combo, AttackLevel level)
    {
        Weapon currentWeapon = characterController.rightWeapon;
        
        if (currentWeapon == Weapon.TwoHandSword)
        {
            return level switch
            {
                AttackLevel.Light => combo switch
                {
                    0 => AttackAnimationType.TwoHandSword_Light1,
                    1 => AttackAnimationType.TwoHandSword_Light2,
                    _ => AttackAnimationType.TwoHandSword_Light1
                },
                AttackLevel.Medium => combo switch
                {
                    0 => AttackAnimationType.TwoHandSword_Medium1,
                    1 => AttackAnimationType.TwoHandSword_Medium2,
                    _ => AttackAnimationType.TwoHandSword_Medium1
                },
                AttackLevel.Heavy => AttackAnimationType.TwoHandSword_Heavy1,
                _ => AttackAnimationType.TwoHandSword_Light1
            };
        }
        else // Unarmed attacks
        {
            return level switch
            {
                AttackLevel.Light => combo switch
                {
                    0 => AttackAnimationType.Unarmed_Light1,
                    1 => AttackAnimationType.Unarmed_Light2,
                    _ => AttackAnimationType.Unarmed_Light1
                },
                AttackLevel.Medium => combo switch
                {
                    0 => AttackAnimationType.Unarmed_Medium1,
                    1 => AttackAnimationType.Unarmed_Medium2,
                    _ => AttackAnimationType.Unarmed_Medium1
                },
                AttackLevel.Heavy => AttackAnimationType.Unarmed_Heavy1,
                _ => AttackAnimationType.Unarmed_Light1
            };
        }
    }

    /// <summary>
    /// Determines attack level based on combo count
    /// </summary>
    private AttackLevel GetAttackLevelFromCombo(int combo)
    {
        return combo switch
        {
            0 => AttackLevel.Light,    // First hit: Light attack
            1 => AttackLevel.Medium,   // Second hit: Medium attack
            2 => AttackLevel.Heavy,    // Third hit: Heavy attack
            _ => AttackLevel.Light     // Default: Light attack
        };
    }

    /// <summary>
    /// Gets the maximum number of hits in a combo
    /// </summary>
    private int GetMaxComboCount()
    {
        return 3; // Fixed 3-hit combo system
    }

    /// <summary>
    /// Resets the combo counter and updates last attack time
    /// </summary>
    private void ResetCombo()
    {
        comboCount = 0;
        lastAttackTime = Time.time;
    }
}
