using UnityEngine;
using RPGCharacterAnims.Lookups;

public interface IAttacker
{
    void PerformAttack(int attackNumber, Side attackSide);
}