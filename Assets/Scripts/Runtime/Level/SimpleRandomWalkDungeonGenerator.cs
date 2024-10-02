using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JZK.Utility;

namespace Level
{
    public class SimpleRandomWalkDungeonGenerator : DungeonGenerator
    {
        [SerializeField] protected Vector2Int _startPos;
        [SerializeField] protected RandomWalkSettingsSO _settings;

        public override HashSet<Vector2Int> CreateFloorPositions(System.Random random)
        {
            return ProceduralGeneration.RunRandomWalk(_startPos, _settings.Iterations, _settings.WalkLength, _settings.StartRandomEachIteration, random); 
        }
    }
}

