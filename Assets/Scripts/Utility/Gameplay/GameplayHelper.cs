using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.Gameplay
{
    public enum EOrthogonalDirection
	{
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3,
        Invalid = 4,
	}

    public static class GameplayHelper
    {
		public static Vector2 INVALID_PLAYER_POS = new(-1, -1);

        public static EOrthogonalDirection GetRandomDirection(System.Random random)
        {
            int directionIndex = random.Next(4);
            return (EOrthogonalDirection)directionIndex;
        }

        public static EOrthogonalDirection GetOppositeDirection(EOrthogonalDirection direction)
		{
            switch(direction)
			{
                case EOrthogonalDirection.Up:
                    return EOrthogonalDirection.Down;
                case EOrthogonalDirection.Down:
                    return EOrthogonalDirection.Up;
                case EOrthogonalDirection.Right:
                    return EOrthogonalDirection.Left;
                case EOrthogonalDirection.Left:
                    return EOrthogonalDirection.Right;
                default:
                    Debug.LogWarning("[GAMEPLAYHELPER] shouldn't be here");
                    return EOrthogonalDirection.Invalid;
			}
		}

        public static List<EOrthogonalDirection> GetPerpendicularDirections(EOrthogonalDirection direction)
        {
            switch(direction)
            {
                case EOrthogonalDirection.Up:
                    return new()
                    {
                        EOrthogonalDirection.Left,
                        EOrthogonalDirection.Right,
                    };
                case EOrthogonalDirection.Down:
                    return new()
                    {
                        EOrthogonalDirection.Left,
                        EOrthogonalDirection.Right,
                    };
                case EOrthogonalDirection.Left:
                    return new()
                    {
                        EOrthogonalDirection.Up,
                        EOrthogonalDirection.Down,
                    };
                case EOrthogonalDirection.Right:
                    return new()
                    {
                        EOrthogonalDirection.Up,
                        EOrthogonalDirection.Down,
                    };
                default:
                    Debug.LogWarning("[GAMEPLAYHELPER] shouldn't be here");
                    return null;
            }
        }
    }
}