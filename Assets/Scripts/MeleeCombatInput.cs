using UnityEngine;
using RPGCharacterAnims;
using RPGCharacterAnims.Lookups;

public class MeleeCombatInput : MonoBehaviour
{
    private MeleeCombatSystem meleeCombatSystem;
    private RPGCharacterController characterController;

    private void Start()
    {
        meleeCombatSystem = GetComponent<MeleeCombatSystem>();
        characterController = GetComponent<RPGCharacterController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            meleeCombatSystem.PerformAttack(1);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (characterController.rightWeapon == Weapon.Unarmed)
            {
                meleeCombatSystem.EquipWeapon(Weapon.TwoHandSword);
            }
            else
            {
                meleeCombatSystem.UnequipWeapon();
            }
        }

        // 添加更多输入控制
    }
}