using System.Collections.Generic;
using UnityEngine;

public static class OrientationUtilities
{
    private static readonly Dictionary<Orientation, Orientation> OppositeOrientations =
        new Dictionary<Orientation, Orientation>
        {
            { Orientation.west, Orientation.east },
            { Orientation.east, Orientation.west },
            { Orientation.north, Orientation.south },
            { Orientation.south, Orientation.north }
        };

    public static Orientation GetReverseOrientation(this Orientation orientation)
    {
        return OppositeOrientations[orientation];
    }

    public static bool IsReverseOrientation(this Orientation o1, Orientation o2)
    {
        return OppositeOrientations[o1] == o2;
    }

    public static Vector2Int NormalizePosition(this Orientation orientation)
    {
        switch (orientation)
        {
            case Orientation.north:
                return Vector2Int.up;
            case Orientation.south:
                return Vector2Int.down;
            case Orientation.west:
                return Vector2Int.left;
            case Orientation.east:
                return Vector2Int.right;
            default:
                return Vector2Int.zero;
        }
    }
}