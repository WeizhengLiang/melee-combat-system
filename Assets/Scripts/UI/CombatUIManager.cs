using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using RPGCharacterAnims;
using RPGCharacterAnims.Actions;
using RPGCharacterAnims.Lookups;

public class CombatUIManager : MonoBehaviour
{
    [Header("Player UI")]
    public TextMeshProUGUI playerAttackPhase;
    public TextMeshProUGUI playerAnimationName;
    public TextMeshProUGUI playerAttackLevel;

    [Header("NPC UI")]
    public TextMeshProUGUI npcAttackPhase;
    public TextMeshProUGUI npcAnimationName;
    public TextMeshProUGUI npcAttackLevel;
    public TMP_Dropdown npcActionDropdown;

    [Header("References")]
    public RPGCharacterController playerController;
    public RPGCharacterController npcController;
    public MeleeCombatSystem playerCombatSystem;
    public MeleeCombatSystem npcCombatSystem;
    public Animator playerAnimator;
    public Animator npcAnimator;

    private void Start()
    {
        SetupNPCDropdown();
        StartCoroutine(UpdateUI());
    }

    private void SetupNPCDropdown()
    {
        npcActionDropdown.ClearOptions();
        npcActionDropdown.AddOptions(new List<string> { "Idle", "Attack", "Block" });
        npcActionDropdown.onValueChanged.AddListener(OnNPCActionChanged);
    }

    private IEnumerator UpdateUI()
    {
        while (true)
        {
            UpdatePlayerUI();
            UpdateNPCUI();
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void UpdatePlayerUI()
    {
        var attackHandler = playerController.GetHandler(HandlerTypes.Attack) as MCS_Attack;
        if (attackHandler == null) return;

        playerAttackPhase.text = $"Attack Phase: {attackHandler.CurrentAttackPhase}";
        
        // 安全地获取动画名称
        var animInfo = playerAnimator.GetCurrentAnimatorClipInfo(0);
        if (animInfo != null && animInfo.Length > 0)
        {
            playerAnimationName.text = $"Animation Name: {animInfo[0].clip.name}";
        }
        else
        {
            playerAnimationName.text = "Animation Name: None";
        }
        
        playerAttackLevel.text = $"Attack Level: {attackHandler.CurrentAttackLevel}";
    }

    private void UpdateNPCUI()
    {
        var attackHandler = npcController.GetHandler(HandlerTypes.Attack) as MCS_Attack;
        if (attackHandler == null) return;

        npcAttackPhase.text = $"Attack Phase: {attackHandler.CurrentAttackPhase}";
        
        // 安全地获取动画名称
        var animInfo = npcAnimator.GetCurrentAnimatorClipInfo(0);
        if (animInfo != null && animInfo.Length > 0)
        {
            npcAnimationName.text = $"Animation Name: {animInfo[0].clip.name}";
        }
        else
        {
            npcAnimationName.text = "Animation Name: None";
        }
        
        npcAttackLevel.text = $"Attack Level: {attackHandler.CurrentAttackLevel}";
    }

    private void OnNPCActionChanged(int index)
    {
        switch (index)
        {
            case 0: // Idle
                npcController.EndAction(HandlerTypes.Attack);
                npcController.EndAction(HandlerTypes.Block);
                break;
            case 1: // Attack
                StartCoroutine(PerformNPCAttackCombo());
                break;
            case 2: // Block
                var meleeSystem = npcController.GetComponent<MeleeCombatSystem>();
                if (meleeSystem != null)
                {
                    meleeSystem.PerformBlock();
                }
                break;
            case 3: // Dodge
                var meleeSys = npcController.GetComponent<MeleeCombatSystem>();
                if (meleeSys != null)
                {
                    meleeSys.PerformDodge();
                }
                break;
        }
    }

    private IEnumerator PerformNPCAttackCombo()
    {
        if (npcController.rightWeapon != Weapon.TwoHandSword)
        {
            if (!npcController.HandlerExists(HandlerTypes.SwitchWeapon))
            {
                yield break;
            }
            if (!npcController.CanStartAction(HandlerTypes.SwitchWeapon))
            {
                yield break;
            }

            var context = new SwitchWeaponContext();
            Weapon newWeapon = Weapon.TwoHandSword;

            context.type = HandlerTypes.Switch;
            context.side = "None";
            context.leftWeapon = newWeapon;
            context.rightWeapon = newWeapon;

            npcController.StartAction(HandlerTypes.SwitchWeapon, context);
            yield return new WaitForSeconds(1f); // 等待武器切换完成
        }

        int comboCount = 0;
        while (npcActionDropdown.value == 1)
        {
            var attackType = GetAttackTypeFromCombo(comboCount);
            var attackData = AnimationData.GetAttackData(attackType);
            
            if (attackData != null)
            {
                npcController.StartAction(HandlerTypes.Attack, 
                    new AttackContext(HandlerTypes.Attack, Side.Right, attackData.legacyAnimationNumber, attackData.attackLevel));
            }
            
            comboCount = (comboCount + 1) % 3;
            yield return new WaitForSeconds(1f);
        }
    }

    private AttackAnimationType GetAttackTypeFromCombo(int combo)
    {
        return combo switch
        {
            0 => AttackAnimationType.TwoHandSword_Light1,
            1 => AttackAnimationType.TwoHandSword_Medium1,
            2 => AttackAnimationType.TwoHandSword_Heavy1,
            _ => AttackAnimationType.TwoHandSword_Light1
        };
    }
}