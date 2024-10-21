using UnityEngine;
using RPGCharacterAnims;
using RPGCharacterAnims.Actions;
using RPGCharacterAnims.Lookups;

public class MeleeCombatInput : MonoBehaviour
{
    public delegate void WeaponToggleEventHandler();
    public static event WeaponToggleEventHandler OnWeaponToggle;

    private MeleeCombatSystem meleeCombatSystem;

    private void Start()
    {
        meleeCombatSystem = GetComponent<MeleeCombatSystem>();
    }

    private void Update()
    {
        CustomMeleeCombatInputs();
    }

    private void CustomMeleeCombatInputs()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {   
            meleeCombatSystem.PerformAttack(Random.Range(1, 4), Side.Right);
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            meleeCombatSystem.PerformAttack(Random.Range(4, 7), Side.Left);
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            OnWeaponToggle?.Invoke();
        }
    }
}
