using Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace JZK.Utility
{
    public static class ProceduralGeneration
    {
        public static HashSet<Vector2Int> SimpleRandomWalk(Vector2Int startPos, int length, Random random)
        {
            HashSet<Vector2Int> path = new();

            path.Add(startPos);
            var prevPos = startPos;

            for (int stepIndex = 0; stepIndex < length; ++stepIndex)
            {
                var newPos = prevPos + Direction2D.GetRandomCardinalDirection(random);
                path.Add(newPos);
                prevPos = newPos;
            }
            return path;
        }

        public static HashSet<Vector2Int> RunRandomWalk(Vector2Int startPos, int iterations, int walkLength, bool startRandomlyEachIteration, Random random)
        {
            var currentPos = startPos;
            HashSet<Vector2Int> floorPositions = new();
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                //System.Random random = _useDebugSeed ? new(_debugSeed) : new();
                var path = ProceduralGeneration.SimpleRandomWalk(currentPos, walkLength, random);
                floorPositions.UnionWith(path);
                if (startRandomlyEachIteration)
                {
                    int nextWalkStartIndex = random.Next(0, floorPositions.Count);
                    currentPos = floorPositions.ElementAt(nextWalkStartIndex);
                }
            }

            if (false)
            {
                foreach (var position in floorPositions)
                {
                    Debug.Log("[HELLO] floor includes tile at " + position.ToString());
                }
            }

            return floorPositions;
        }

        public static HashSet<Vector2Int> FindWallsInDirections(HashSet<Vector2Int> floorPositions, List<Vector2Int> directionList)
        {
            HashSet<Vector2Int> wallPositions = new();
            foreach (var position in floorPositions)
            {
                foreach (var direction in directionList)
                {
                    var neighbourPos = position + direction;
                    if (!floorPositions.Contains(neighbourPos))
                    {
                        wallPositions.Add(neighbourPos);
                    }
                }
            }

            return wallPositions;
        }
    }
}

