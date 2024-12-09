using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.Level
{
    public abstract class DungeonGenerator : MonoBehaviour
    {
        public abstract HashSet<Vector2Int> CreateFloorPositions(System.Random random);
    }
}

