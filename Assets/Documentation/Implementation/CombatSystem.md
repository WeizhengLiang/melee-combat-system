# Combat System Implementation

## Overview
The combat system is built around a modular, event-driven architecture that handles melee combat mechanics, weapon management, and damage calculations.

## Core Components

### MeleeCombatSystem

see details in [MeleeCombatSystem](../Scripts/Combat/MeleeCombatSystem.cs)

#### Attack System
- **Combo System**
  * Time-window based combo detection
  * Progressive attack level system
  * Dynamic combo state management

- **Attack Validation**
  * State-based attack validation
  * Animation-driven hit detection
  * Weapon-specific attack patterns

#### Block System
- Block state management
- Damage reduction calculations
- Block animation integration

## Performance Considerations
- Input buffering for responsive controls
- Optimized hit detection
- Efficient animation state management