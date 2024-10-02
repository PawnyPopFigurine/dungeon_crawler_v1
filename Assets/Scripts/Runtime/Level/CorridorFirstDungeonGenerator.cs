using JZK.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Level
{
    public class CorridorFirstDungeonGenerator : SimpleRandomWalkDungeonGenerator
    {
        [SerializeField]
        int _corridorLength;
        [SerializeField]
        int _corridorCount;

        [SerializeField]
        [Range(0, 1)]
        float _roomPercent;

        public override HashSet<Vector2Int> CreateFloorPositions(System.Random random)
        {
            HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
            HashSet<Vector2Int> potentialRoomPositions = new HashSet<Vector2Int>();

            CreateCorridors(floorPositions, potentialRoomPositions, random);

            HashSet<Vector2Int> roomPositions = CreateRooms(potentialRoomPositions, random);

            floorPositions.UnionWith(roomPositions);

            return floorPositions;
            //throw new NotImplementedException();
        }

        private HashSet<Vector2Int> CreateRooms(HashSet<Vector2Int> potentialRoomPositions, System.Random random)
        {
            HashSet<Vector2Int> roomPositions = new HashSet<Vector2Int>();
            int roomToCreateCount = Mathf.RoundToInt(potentialRoomPositions.Count * _roomPercent);

            List<Vector2Int> roomsToCreate = potentialRoomPositions.ToList();
            ListHelper.Shuffle(roomsToCreate, random.Next());
            roomsToCreate = roomsToCreate.Take(roomToCreateCount).ToList();

            //List<Vector2Int> roomsToCreate = potentialRoomPositions.OrderBy(x => Guid.NewGuid()).Take(roomToCreateCount).ToList();

            int index = 0;
            foreach(var roomPos in roomsToCreate)
            {
                Debug.Log("[HELLO] creating room at position " + roomsToCreate[index]);
                ++index;
                var roomFloor = ProceduralGeneration.RunRandomWalk(roomPos, _settings.Iterations, _settings.WalkLength, _settings.StartRandomEachIteration, random);
                roomPositions.UnionWith(roomFloor);
            }

            return roomPositions;
            //return null;
        }

        private void CreateCorridors(HashSet<Vector2Int> floorPositions, HashSet<Vector2Int> potentialRoomPositions, System.Random random)
        {
            Vector2Int currentPos = _startPos;
            potentialRoomPositions.Add(currentPos);

            for(int corridorIndex = 0; corridorIndex < _corridorCount; ++corridorIndex)
            {
                var corridor = ProceduralGeneration.RandomWalkCorridor(currentPos, _corridorLength, random);
                currentPos = corridor[corridor.Count - 1];
                potentialRoomPositions.Add(currentPos);
                floorPositions.UnionWith(corridor);
            }
        }
    }
}