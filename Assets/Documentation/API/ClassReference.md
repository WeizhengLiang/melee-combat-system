# Class Reference

## Core Classes

### MeleeCombatSystem
Main class handling combat mechanics and state management.

```csharp

public class MeleeCombatSystem : MonoBehaviour
{
    /// <summary>
    /// Performs an attack with specified parameters
    /// </summary>
    /// <param name="animationNumber">Animation ID to play</param>
    /// <param name="attackLevel">Level of attack (Light/Medium/Heavy)</param>
    public void PerformAttack(int animationNumber, AttackLevel attackLevel);
    /// <summary>
    /// Initiates blocking state
    /// </summary>
    public void PerformBlock();
    /// <summary>
    /// Ends blocking state
    /// </summary>
    public void EndBlock();
    /// <summary>
    /// Performs a dodge action
    /// </summary>
    public void PerformDodge();
}

```

### WeaponManager
Handles weapon states and transitions.

```csharp

public class WeaponManager : MonoBehaviour
{
    /// <summary>
    /// Switches to specified weapon type
    /// </summary>
    /// <param name="weaponType">Type of weapon to switch to</param>
    /// <returns>True if switch successful</returns>
    public bool SwitchWeapon(WeaponType weaponType);
    /// <summary>
    /// Gets current weapon data
    /// </summary>
    /// <returns>Current WeaponDataSO</returns>
    public WeaponDataSO GetCurrentWeapon();
}

```


### InputManager
Manages input processing and routing.

```csharp

public class InputManager : MonoBehaviour
{
    /// <summary>
    /// Processes combat inputs
    /// </summary>
    private void CustomMeleeCombatInputs();
    /// <summary>
    /// Handles weapon toggle input
    /// </summary>
    private void HandleWeaponToggle();
}

```


## Enums and Types

### AttackLevel

```csharp

public enum AttackLevel
{
    Light,
    Medium,
    Heavy
}
```



### WeaponType

```csharp

public enum Weapon
{
    Unarmed,
    TwoHandSword
}
```


## Events and Delegates

```csharp
// Weapon toggle event
public delegate void WeaponToggleEventHandler();
public static event WeaponToggleEventHandler OnWeaponToggle;
// Combat events
public delegate void CombatEventHandler(AttackData attackData);
public static event CombatEventHandler OnAttackStart;
public static event CombatEventHandler OnHitDetection;
```