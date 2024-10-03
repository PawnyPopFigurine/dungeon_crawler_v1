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
                var path = SimpleRandomWalk(currentPos, walkLength, random);
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

        public static List<Vector2Int> RandomWalkCorridor(Vector2Int startPos, int corridorLength, Random random)
        {
            List<Vector2Int> corridor = new();
            Vector2Int direction = Direction2D.GetRandomCardinalDirection(random);
            Vector2Int currentPos = startPos;
            corridor.Add(currentPos);

            for (int stepIndex = 0; stepIndex < corridorLength; stepIndex++)
            {
                currentPos += direction;
                corridor.Add(currentPos);
            }

            return corridor;
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

        public static List<Vector2Int> FindAllDeadEnds(HashSet<Vector2Int> floorPositions)
		{
            List<Vector2Int> deadEnds = new();
            foreach(var floorPos in floorPositions)
			{
                int numNeighbours = 0;
                foreach(Vector2Int direction in Direction2D.DIRECTIONS)
				{
                    if(!floorPositions.Contains(floorPos + direction))
					{
                        continue;
					}

                    numNeighbours++;
				}

                if(numNeighbours == 1)
				{
                    deadEnds.Add(floorPos);
				}
			}

            return deadEnds;
		}

        public static List<BoundsInt> BinarySpacePartitioning(BoundsInt spaceToSplit, int minWidth, int minHeight, Random random)
		{
            Queue<BoundsInt> roomsQueue = new Queue<BoundsInt>();
            List<BoundsInt> roomsList = new List<BoundsInt>();
            roomsQueue.Enqueue(spaceToSplit);
            while(roomsQueue.Count > 0)
			{
                var room = roomsQueue.Dequeue();
                if(room.size.y >= minHeight && room.size.x >= minWidth)
				{
                    if(random.NextDouble() <= 0.5)
					{
                        if(room.size.y >= minHeight * 2)
						{
                            SplitRoomHorizontally(minHeight, roomsQueue, room, random);
						}
                        else if(room.size.x >= minWidth * 2)
						{
                            SplitRoomVertically(minWidth, roomsQueue, room, random);
						}
                        else
						{
                            roomsList.Add(room);
						}
					}
                    else
					{
                        if(room.size.x >= minWidth * 2)
						{
                            SplitRoomVertically(minWidth, roomsQueue, room, random);
						}
                        else if(room.size.y >= minHeight * 2)
						{
                            SplitRoomHorizontally(minHeight, roomsQueue, room, random);
						}
                        else
						{
                            roomsList.Add(room);
						}
					}
				}
			}

            return roomsList;
		}

        static void SplitRoomVertically(int minWidth, Queue<BoundsInt> roomsQueue, BoundsInt roomToSplit, Random random)
		{
            var xSplit = random.Next(1, roomToSplit.size.x);    //random.Next(minWidth, room.size.x - minWidth)
            BoundsInt splitRoom1 = new BoundsInt(roomToSplit.min, new Vector3Int(xSplit, roomToSplit.min.y, roomToSplit.min.z));
            BoundsInt splitRoom2 = new BoundsInt(new Vector3Int(roomToSplit.min.x + xSplit, roomToSplit.min.y, roomToSplit.min.z),
                new Vector3Int(roomToSplit.size.x - xSplit, roomToSplit.size.y, roomToSplit.size.z));
            roomsQueue.Enqueue(splitRoom1);
            roomsQueue.Enqueue(splitRoom2);
		}

        static void SplitRoomHorizontally(int minHeight, Queue<BoundsInt> roomsQueue, BoundsInt roomToSplit, Random random)
        {
            var ySplit = random.Next(1, roomToSplit.size.y);    //random.Next(minHeight, room.size.y - minHeight)
            BoundsInt splitRoom1 = new BoundsInt(roomToSplit.min, new Vector3Int(roomToSplit.size.x, ySplit, roomToSplit.size.z));
            BoundsInt splitRoom2 = new BoundsInt(new Vector3Int(roomToSplit.min.x, roomToSplit.min.y + ySplit, roomToSplit.min.z),
                new Vector3Int(roomToSplit.size.x, roomToSplit.size.y - ySplit, roomToSplit.size.z));
            roomsQueue.Enqueue(splitRoom1);
            roomsQueue.Enqueue(splitRoom2);
        }
    }
}

