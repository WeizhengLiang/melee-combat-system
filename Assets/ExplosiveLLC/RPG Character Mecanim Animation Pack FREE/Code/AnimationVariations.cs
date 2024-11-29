using System.Collections.Generic;
using RPGCharacterAnims.Lookups;

namespace RPGCharacterAnims
{
    /// <summary>
    /// This class contains all the variations for given action types.
    /// </summary>
    public class AnimationVariations
    {
        public static readonly KnockbackType[] Knockbacks = { KnockbackType.Knockback1, KnockbackType.Knockback2 };
        public static readonly KnockdownType[] Knockdowns = { KnockdownType.Knockdown1 };
        public static readonly HitType[] Hits = { HitType.Forward1, HitType.Forward2, HitType.Back1, HitType.Left1, HitType.Right1 };
        public static readonly BlockedHitType[] BlockedHits = { BlockedHitType.BlockedHit1, BlockedHitType.BlockedHit2 };

        public static readonly TwoHandedSwordAttack[] TwoHandedSwordAttacks =
        {
            TwoHandedSwordAttack.Attack1, TwoHandedSwordAttack.Attack2, TwoHandedSwordAttack.Attack3,
            TwoHandedSwordAttack.Attack4, TwoHandedSwordAttack.Attack5, TwoHandedSwordAttack.Attack6,
            TwoHandedSwordAttack.Attack7, TwoHandedSwordAttack.Attack8, TwoHandedSwordAttack.Attack9,
            TwoHandedSwordAttack.Attack10, TwoHandedSwordAttack.Attack11
        };

        public static readonly UnarmedAttack[] UnarmedLeftAttacks =
        { UnarmedAttack.LeftAttack1, UnarmedAttack.LeftAttack2, UnarmedAttack.LeftAttack3 };

        public static readonly UnarmedAttack[] UnarmedRightAttacks =
        { UnarmedAttack.RightAttack1, UnarmedAttack.RightAttack2, UnarmedAttack.RightAttack3 };

        // new add
        public static readonly Dictionary<AttackLevel, AttackAnimationType[]> TwoHandSwordAttacksByLevel = new()
        {
            { AttackLevel.Light, new[] { 
                AttackAnimationType.TwoHandSword_Light1, 
                AttackAnimationType.TwoHandSword_Light2
            }},
            { AttackLevel.Medium, new[] { 
                AttackAnimationType.TwoHandSword_Medium1, 
                AttackAnimationType.TwoHandSword_Medium2
            }},
            { AttackLevel.Heavy, new[] { 
                AttackAnimationType.TwoHandSword_Heavy1
            }}
        };

    }
}