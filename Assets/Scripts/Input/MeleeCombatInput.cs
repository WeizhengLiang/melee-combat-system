using UnityEngine;
using RPGCharacterAnims;
using RPGCharacterAnims.Actions;
using RPGCharacterAnims.Lookups;

public class MeleeCombatInput : MonoBehaviour
{
    public delegate void WeaponToggleEventHandler();
    public static event WeaponToggleEventHandler OnWeaponToggle;

    private MeleeCombatSystem meleeCombatSystem;
    private WeaponManager weaponManager;
    private RPGCharacterController characterController;
    private float lastAttackTime;
    private int comboCount;
    private float comboTimeout;  // 动态计算的连击超时时间
    private AttackLevel currentAttackLevel = AttackLevel.Light;
    private Side currentAttackSide = Side.Right;

    private void Start()
    {
        meleeCombatSystem = GetComponent<MeleeCombatSystem>();
        weaponManager = GetComponent<WeaponManager>();
        characterController = GetComponent<RPGCharacterController>();
        
        // 初始化连击超时时间为最长的攻击动画时长的1.5倍
        comboTimeout = GetMaxAttackDuration() * 1.5f;
        lastAttackTime = -comboTimeout;
        comboCount = 0;
    }

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

        // 重置连击计数
        if (Time.time - lastAttackTime > comboTimeout)
        {
            ResetCombo();
        }
    }

    private void HandleAttackInput()
    {
        // 首先检查是否可以开始新的攻击动作
        if (!characterController.CanStartAction(HandlerTypes.Attack))
        {
            return;
        }

        UpdateComboState();
        var attackData = GetAttackDataFromCombo();
        if (attackData != null)
        {
            meleeCombatSystem.PerformAttack(attackData.legacyAnimationNumber, attackData.attackLevel);
        }
    }

    private void UpdateComboState()
    {
        if (Time.time - lastAttackTime <= comboTimeout)
        {
            comboCount = (comboCount + 1) % GetMaxComboCount();
            lastAttackTime = Time.time;  // 更新最后攻击时间
        }
        else
        {
            ResetCombo();
        }
    }

    private AttackAnimationData GetAttackDataFromCombo()
    {
        currentAttackLevel = GetAttackLevelFromCombo(comboCount);
        var attackType = GetAttackTypeFromCombo(comboCount, currentAttackLevel);
        return AnimationData.GetAttackData(attackType);
    }

    private AttackAnimationType GetAttackTypeFromCombo(int combo, AttackLevel level)
    {
        // 获取当前武器类型
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
        else // 空手攻击
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

    private AttackLevel GetAttackLevelFromCombo(int combo)
    {
        return combo switch
        {
            0 => AttackLevel.Light,    // 第一击为轻攻击
            1 => AttackLevel.Medium,   // 第二击为中攻击
            2 => AttackLevel.Heavy,    // 第三击为重攻击
            _ => AttackLevel.Light     // 默认为轻攻击
        };
    }

    private int GetMaxComboCount()
    {
        return 3; // 固定为3连击
    }

    private void ResetCombo()
    {
        comboCount = 0;
        lastAttackTime = Time.time;
    }
}
