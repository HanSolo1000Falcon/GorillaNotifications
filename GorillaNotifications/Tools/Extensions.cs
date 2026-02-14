using UnityEngine;

namespace GorillaNotifications.Tools;

public static class Extensions
{
    public static Transform TakeChild(this Transform parent, params int[] childPath)
    {
        Transform child = parent.GetChild(childPath[0]);
        for (int i = 1; i < childPath.Length; i++)
            child = child.GetChild(childPath[i]);

        return child;
    }
}