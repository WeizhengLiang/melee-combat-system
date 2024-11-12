using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CombatAnimationConfig", menuName = "Combat/Animation Config")]
public class CombatAnimationConfig : ScriptableObject
{
    public List<AttackAnimationData> attackAnimations = new List<AttackAnimationData>();
    private Dictionary<AttackAnimationType, AttackAnimationData> animationCache;

    private void OnEnable()
    {
        InitializeCache();
    }

    private void InitializeCache()
    {
        animationCache = new Dictionary<AttackAnimationType, AttackAnimationData>();
        foreach (var anim in attackAnimations)
        {
            animationCache[anim.animationType] = anim;
        }
    }

    public AttackAnimationData GetAnimationData(AttackAnimationType type)
    {
        return animationCache.TryGetValue(type, out var data) ? data : null;
    }
}