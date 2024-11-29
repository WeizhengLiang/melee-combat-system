using UnityEngine;
using RPGCharacterAnims;
using RPGCharacterAnims.Actions;
using RPGCharacterAnims.Lookups;

/// <summary>
/// Manages input handling and weapon switching for the RPG character
/// </summary>
public class InputManager : MonoBehaviour
{
    private RPGCharacterController rpgCharacterController;
    private RPGCharacterInputController rpgCharacterInputController;
    private MeleeCombatSystem meleeCombatSystem;

    /// <summary>
    /// Initializes components and subscribes to input events
    /// </summary>
    private void Start()
    {
        rpgCharacterController = GetComponent<RPGCharacterController>();
        rpgCharacterInputController = GetComponent<RPGCharacterInputController>();
        meleeCombatSystem = GetComponent<MeleeCombatSystem>();

        MeleeCombatInput.OnWeaponToggle += HandleWeaponToggle;
    }

    /// <summary>
    /// Unsubscribes from input events when the object is destroyed
    /// </summary>
    private void OnDestroy()
    {
        MeleeCombatInput.OnWeaponToggle -= HandleWeaponToggle;
    }

    /// <summary>
    /// Handles weapon toggle input and switches between unarmed and two-hand sword
    /// </summary>
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
