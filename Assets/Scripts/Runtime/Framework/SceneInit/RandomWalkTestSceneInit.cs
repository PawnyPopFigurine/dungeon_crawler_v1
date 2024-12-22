using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JZK.Utility;
using System.Linq;
using Random = System.Random;
using UnityEngine.Tilemaps;
using JZK.Level;

namespace JZK.Framework
{
    public class RandomWalkTestSceneInit : SceneInit
    {
        ISystemReference<MonoBehaviour>[] _systems = new ISystemReference<MonoBehaviour>[]
        {
            new SystemReference<Input.InputSystem>(),

            new SystemReference<UI.UIStateSystem>(),

        };

        [SerializeField] private bool _useDebugSeed = false;
        [SerializeField] private int _debugSeed;
        private int _currentSeed;
        public int CurrentSeed => _currentSeed;

        

        [SerializeField] bool _printDebug;

        [SerializeField] private Tilemap _floorTilemap;
        [SerializeField] private TileBase _floortile;
        [SerializeField] private Tilemap _wallTilemap;
        [SerializeField] private TileBase _wallTile_Full;

        [SerializeField] DungeonGenerator _generator;

        

        public void Start()
        {
            Setup(_systems);

            InitialiseSeed();

            GenerateDungeon();
        }

        private void Update()
        {
            UpdateScene();
        }

        void InitialiseSeed()
        {
            _currentSeed = _useDebugSeed ? _debugSeed : System.DateTime.Now.Millisecond;
            UnityEngine.Random.InitState(_currentSeed);
        }

        public void GenerateDungeon()
        {
            System.Random random = _useDebugSeed ? new(_debugSeed) : new();

            HashSet<Vector2Int> floorPositions = _generator.CreateFloorPositions(random);
            PaintTiles(floorPositions, _floorTilemap, _floortile);

            HashSet<Vector2Int> wallPositions = ProceduralGeneration.FindWallsInDirections(floorPositions, Direction2D.DIRECTIONS);
            PaintTiles(wallPositions, _wallTilemap, _wallTile_Full);

            /*HashSet<Vector2Int> floorPositions = ProceduralGeneration.RunRandomWalk(_startPos, _settings.Iterations, _settings.WalkLength, _settings.StartRandomEachIteration, random);
            PaintTiles(floorPositions, _floorTilemap, _floortile);

            HashSet<Vector2Int> wallPositions = ProceduralGeneration.FindWallsInDirections(floorPositions, Direction2D.DIRECTIONS);
            PaintTiles(wallPositions, _wallTilemap, _wallTile_Full);*/
        }

        void PaintTiles(IEnumerable<Vector2Int> positions, Tilemap tileMap, TileBase tile)
        {
            foreach(var pos in positions)
            {
                PaintTile(tileMap, tile, pos);
            }
        }

        void PaintTile(Tilemap tileMap, TileBase tile, Vector2Int position)
        {
            var tilePosition = tileMap.WorldToCell((Vector3Int)position);
            tileMap.SetTile(tilePosition, tile);
        }

        public void ClearTiles()
        {
            _floorTilemap.ClearAllTiles();
            _wallTilemap.ClearAllTiles();
        }
    }
}