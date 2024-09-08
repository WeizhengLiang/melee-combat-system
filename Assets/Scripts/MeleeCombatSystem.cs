using UnityEngine;
using RPGCharacterAnims;
using RPGCharacterAnims.Lookups;

public class MeleeCombatSystem : MonoBehaviour
{
    private RPGCharacterController characterController;
    private RPGCharacterMovementController movementController;
    private RPGCharacterWeaponController weaponController;
    private AttackHandler attackHandler;

    private void Start()
    {
        characterController = GetComponent<RPGCharacterController>();
        movementController = GetComponent<RPGCharacterMovementController>();
        weaponController = GetComponent<RPGCharacterWeaponController>();
        attackHandler = GetComponent<AttackHandler>();
    }

    public void PerformAttack(int attackNumber)
    {
        if (characterController.canAction)
        {
            attackHandler.PerformAttack(attackNumber, Side.Left);
        }
    }

    public void EquipWeapon(Weapon weapon)
    {
        weaponController.UnsheathWeapon(weapon);
    }

    public void UnequipWeapon()
    {
        weaponController.SheathWeapon(characterController.rightWeapon, Weapon.Unarmed);
    }

    // 添加更多近战相关的方法
}