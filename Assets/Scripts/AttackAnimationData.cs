using RPGCharacterAnims.Lookups;

[System.Serializable]
public class AttackAnimationData
{
    public AttackAnimationType animationType;
    public AttackLevel attackLevel;
    public KnockbackType knockbackType;
    public float duration;
    public int legacyAnimationNumber;
}