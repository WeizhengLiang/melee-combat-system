using UnityEngine;
using RPGCharacterAnims;
using RPGCharacterAnims.Actions;
using RPGCharacterAnims.Lookups;

public class MeleeCombatInput : MonoBehaviour
{
    public delegate void WeaponToggleEventHandler();
    public static event WeaponToggleEventHandler OnWeaponToggle;

    private MeleeCombatSystem meleeCombatSystem;
    private float lastAttackTime;
    private int comboCount;
    private const float COMBO_TIMEOUT = 1.0f; // Time window for combo

    private void Start()
    {
        meleeCombatSystem = GetComponent<MeleeCombatSystem>();
        lastAttackTime = -COMBO_TIMEOUT;
        comboCount = 0;
    }

    private void Update()
    {
        CustomMeleeCombatInputs();
    }

    private void CustomMeleeCombatInputs()
    {
        if (Input.GetKeyDown(KeyCode.J)) // Single attack button
        {   
            PerformAttack();
        }

        if (Input.GetKeyDown(KeyCode.K)) // Single attack button
        {   
            meleeCombatSystem.PerformBlock();
        }
        
        if (Input.GetKeyDown(KeyCode.H)) // Single attack button
        {   
            meleeCombatSystem.PerformDodge();
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            OnWeaponToggle?.Invoke();
        }

        // Reset combo if timeout
        if (Time.time - lastAttackTime > COMBO_TIMEOUT)
        {
            comboCount = 0;
        }
    }

    private void PerformAttack()
    {
        if (Time.time - lastAttackTime <= COMBO_TIMEOUT)
        {
            comboCount = (comboCount + 1) % 6; // Cycle through 6 attack types
        }
        else
        {
            comboCount = 0; // Reset combo if timeout
        }

        int attackNumber;
        if (comboCount < 3)
        {
            attackNumber = comboCount + 1; // Light attacks (1, 2, 3)
        }
        else
        {
            attackNumber = comboCount + 1; // Heavy attacks (4, 5, 6)
        }

        meleeCombatSystem.PerformAttack(attackNumber, Side.Right);
        lastAttackTime = Time.time;
    }
}
