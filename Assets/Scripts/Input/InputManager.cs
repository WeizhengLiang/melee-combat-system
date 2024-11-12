using UnityEngine;
using RPGCharacterAnims;
using RPGCharacterAnims.Actions;
using RPGCharacterAnims.Lookups;

public class InputManager : MonoBehaviour
{
    private RPGCharacterController rpgCharacterController;
    private RPGCharacterInputController rpgCharacterInputController;
    private MeleeCombatSystem meleeCombatSystem;

    private void Start()
    {
        rpgCharacterController = GetComponent<RPGCharacterController>();
        rpgCharacterInputController = GetComponent<RPGCharacterInputController>();
        meleeCombatSystem = GetComponent<MeleeCombatSystem>();

        MeleeCombatInput.OnWeaponToggle += HandleWeaponToggle;
    }

    private void OnDestroy()
    {
        MeleeCombatInput.OnWeaponToggle -= HandleWeaponToggle;
    }

    private void HandleWeaponToggle()
    {
        if (!rpgCharacterController.HandlerExists(HandlerTypes.SwitchWeapon)) { return; }
        if (!rpgCharacterController.CanStartAction(HandlerTypes.SwitchWeapon)) { return; }

        var context = new SwitchWeaponContext();
        Weapon newWeapon = (rpgCharacterController.rightWeapon == Weapon.Unarmed) ? Weapon.TwoHandSword : Weapon.Unarmed;

        context.type = HandlerTypes.Switch;
        context.side = "None";
        context.leftWeapon = newWeapon;
        context.rightWeapon = newWeapon;

        rpgCharacterController.StartAction(HandlerTypes.SwitchWeapon, context);
    }
}
