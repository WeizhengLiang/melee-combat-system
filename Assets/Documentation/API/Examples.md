Usage Examples
Basic Combat Implementation
Setting Up Combat System


```csharp

// Basic setup of combat system
public class CombatSetup : MonoBehaviour
{
    private MeleeCombatSystem combatSystem;
    private InputManager inputManager;

    void Start()
    {
        combatSystem = GetComponent<MeleeCombatSystem>();
        inputManager = GetComponent<InputManager>();
        
        // Subscribe to events
        MeleeCombatInput.OnWeaponToggle += HandleWeaponToggle;
    }
}

```

Implementing Basic Attack

```csharp
// Example of basic attack implementation
void HandleAttackInput()
{
    if (!characterController.CanStartAction(HandlerTypes.Attack))
        return;

    var attackData = GetAttackDataFromCombo();
    if (attackData != null)
    {
        combatSystem.PerformAttack(
            attackData.legacyAnimationNumber,
            attackData.attackLevel
        );
    }
}
```

Weapon Switching Example

```csharp
// Example of weapon switching implementation
void HandleWeaponToggle()
{
    if (!rpgCharacterController.HandlerExists(HandlerTypes.SwitchWeapon))
        return;

    var context = new SwitchWeaponContext
    {
        type = HandlerTypes.Switch,
        side = "None",
        leftWeapon = newWeapon,
        rightWeapon = newWeapon
    };

    rpgCharacterController.StartAction(HandlerTypes.SwitchWeapon, context);
}
```