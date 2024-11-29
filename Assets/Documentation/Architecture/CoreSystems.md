# Core Systems Documentation

## Combat System
### MeleeCombatSystem
The central component managing all combat-related functionality.

#### Key Responsibilities
- Combat state management
- Attack execution
- Damage calculation
- Hit detection
- Combat feedback

#### Key Methods
void PerformAttack(int animationNumber, AttackLevel attackLevel)
void PerformBlock()
void PerformDodge()


### WeaponManager
Handles weapon switching and weapon-specific behaviors.

#### Key Features
- Weapon state management
- Weapon switching logic
- Weapon-specific animations

## Input System
### InputManager
Processes and manages all combat-related inputs.

#### Features
- Input detection
- Command buffering
- Input state management

## Animation System
### AnimationController
Manages character animations and state transitions.

#### Features
- Combat animation states
- Transition management
- Animation events