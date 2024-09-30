using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;


namespace JZK.Utility
{
    public static class Direction2D
    {
        public static Vector2Int UP => new(0, 1);
        public static Vector2Int RIGHT => new(1, 0);
        public static Vector2Int DOWN => new(0, -1);
        public static Vector2Int LEFT => new(-1, 0);

        public static List<Vector2Int> DIRECTIONS => new() { UP, RIGHT, DOWN, LEFT };

        public static Vector2Int GetRandomCardinalDirection(Random random)
        {
            int directionIndex = random.Next(0, 3);
            switch (directionIndex)
            {
                case 0:
                    return UP;
                case 1:
                    return RIGHT;
                case 2:
                    return DOWN;
                case 3:
                    return LEFT;
            }

            Debug.LogError("Direction index shouldn't exceed 3 - returning 0");
            return Vector2Int.zero;
        }
    }
}
