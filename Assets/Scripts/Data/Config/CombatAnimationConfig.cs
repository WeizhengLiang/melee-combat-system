using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Configuration for combat animations and their associated data
/// </summary>
[CreateAssetMenu(fileName = "CombatAnimationConfig", menuName = "Combat/Animation Config")]
public class CombatAnimationConfig : ScriptableObject
{
    #region Fields
    [Header("Animation Data")]
    [Tooltip("List of all available attack animations")]
    public List<AttackAnimationData> attackAnimations = new();

    private Dictionary<AttackAnimationType, AttackAnimationData> animationCache;
    #endregion

    #region Unity Lifecycle
    private void OnEnable()
    {
        InitializeCache();
    }
    #endregion

    #region Cache Management
    private void InitializeCache()
    {
        animationCache = new Dictionary<AttackAnimationType, AttackAnimationData>();
        foreach (var anim in attackAnimations)
        {
            if (anim != null)
            {
                animationCache[anim.animationType] = anim;
            }
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Retrieves animation data for the specified attack type
    /// </summary>
    public AttackAnimationData GetAnimationData(AttackAnimationType type)
    {
        return animationCache.TryGetValue(type, out var data) ? data : null;
    }
    #endregion
}