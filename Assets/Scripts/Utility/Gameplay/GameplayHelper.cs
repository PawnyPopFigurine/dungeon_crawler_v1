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

    public enum ELevelTheme
    {
        None = 0,
        Undead = 1,
        Food = 2,
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

        public static WeightedListItem GetWeightedListItem(List<WeightedListItem> items, System.Random random)
		{
            int totalWeighting = 0;
            foreach(WeightedListItem item in items)
			{
                totalWeighting += item.Weighting;
			}

            int randomNumber = random.Next(totalWeighting);

            foreach(WeightedListItem item in items)
			{
                if(randomNumber < item.Weighting)
				{
                    return item;
				}

                randomNumber -= item.Weighting;
			}

            Debug.LogError("[GAMEPLAYHELPER] failed to find list item - returning null");
            return null;
		}

        public static Vector2 GetVectorForDirection(EOrthogonalDirection direction)
		{
            switch(direction)
			{
                case EOrthogonalDirection.Up:
                    return new(0, 1);
                case EOrthogonalDirection.Right:
                    return new(1, 0);
                case EOrthogonalDirection.Down:
                    return new(0, -1);
                case EOrthogonalDirection.Left:
                    return new(-1, 0);
                default:
                    return new(0, 0);
			}
		}

        public static Vector2 GetRotatedVectorForDirection(EOrthogonalDirection direction, float rotateByDegrees)
		{
            Vector2 directionVector = GetVectorForDirection(direction);
            float rotateByRadians = rotateByDegrees * (Mathf.PI / 180);

            float rotatedDirectionX = (directionVector.x * Mathf.Cos(rotateByRadians)) - (directionVector.y * Mathf.Sin(rotateByRadians));
            float rotatedDirectionY = (directionVector.x * Mathf.Sin(rotateByRadians)) + (directionVector.y * Mathf.Cos(rotateByRadians));

            Vector2 rotatedDirection = new(rotatedDirectionX, rotatedDirectionY);
            return rotatedDirection;
        }
    }
}