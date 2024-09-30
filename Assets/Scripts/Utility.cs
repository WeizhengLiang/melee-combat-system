
using UnityEngine;

public static class Utility
{
    public static Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
            {
                return child;
            }
            else
            {
                Transform found = FindDeepChild(child, childName);
                if (found != null)
                {
                    return found;
                }
            }
        }
        return null;
    }
}
