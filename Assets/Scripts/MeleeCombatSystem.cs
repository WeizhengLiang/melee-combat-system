using UnityEngine;
using RPGCharacterAnims;
using RPGCharacterAnims.Lookups;

public class MeleeCombatSystem : MonoBehaviour
{
    private RPGCharacterController characterController;
    private RPGCharacterMovementController movementController;
    private RPGCharacterWeaponController weaponController;

    private void Start()
    {
        characterController = GetComponent<RPGCharacterController>();
        movementController = GetComponent<RPGCharacterMovementController>();
        weaponController = GetComponent<RPGCharacterWeaponController>();
    }

    public void PerformAttack(int attackNumber)
    {
        if (characterController.canAction)
        {
            characterController.Attack(attackNumber, Side.Left,characterController.leftWeapon, characterController.rightWeapon, 0.5f);
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