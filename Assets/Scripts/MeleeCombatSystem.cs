using UnityEngine;
using RPGCharacterAnims;
using RPGCharacterAnims.Lookups;

public class MeleeCombatSystem : MonoBehaviour
{
    public CharacterInstance UserCharacter;

    private RPGCharacterController          characterController;
    private RPGCharacterMovementController  movementController;
    private RPGCharacterWeaponController    weaponController;
    private AttackHandler                   attackHandler;

    private void Start()
    {
        characterController = UserCharacter.GetComponent<RPGCharacterController>();
        movementController  = UserCharacter.GetComponent<RPGCharacterMovementController>();
        weaponController    = UserCharacter.GetComponent<RPGCharacterWeaponController>();
        attackHandler       = UserCharacter.GetComponent<AttackHandler>();
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