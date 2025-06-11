using UnityEngine;

public static class MathUtilities
{
    /// <summary>
    /// 检查两个区间是否重叠
    /// </summary>
    public static bool IsOverlappingInterval(int min1, int max1, int min2, int max2)
    {
        return Mathf.Max(min1, min2) <= Mathf.Min(max1, max2);
    }
}