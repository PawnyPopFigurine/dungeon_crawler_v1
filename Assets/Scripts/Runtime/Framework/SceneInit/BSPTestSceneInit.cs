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
    public class BSPTestSceneInit : SceneInit
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

        [SerializeField] private TileBase _wallTile_Full;

        [SerializeField] List<Tile> DebugRoomBoundsTiles;

        [SerializeField] protected Vector2Int _startPos;
        [SerializeField]
        private int minRoomWidth = 4, minRoomHeight = 4;
        [SerializeField]
        private int dungeonWidth = 20, dungeonHeight = 20;

        [SerializeField] bool _showRoomBounds;

        private int _currentDebugTileIndex;



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

            var roomsList = ProceduralGeneration.BinarySpacePartitioning(new BoundsInt((Vector3Int)_startPos, new Vector3Int(dungeonWidth, dungeonHeight, 0)), minRoomWidth, minRoomHeight, random);

            if(_showRoomBounds)
			{
                VisualiseRoomBounds(roomsList, random);
            }
        }

        Tile GetNextDebugTile()
		{
            int nextTileIndex = _currentDebugTileIndex + 1;
            if(nextTileIndex >= DebugRoomBoundsTiles.Count)
			{
                nextTileIndex = 0;
			}

            _currentDebugTileIndex = nextTileIndex;
            return DebugRoomBoundsTiles[_currentDebugTileIndex];
		}

        void VisualiseRoomBounds(List<BoundsInt> roomBoundsList, Random random)
		{
            foreach (var roomBounds in roomBoundsList)
            {
                Tile visualTile = GetNextDebugTile();
                HashSet<Vector2Int> roomBoundsPositions = new HashSet<Vector2Int>();
                for (int xPos = roomBounds.xMin; xPos < roomBounds.xMax; ++xPos)
                {
                    for (int yPos = roomBounds.yMin; yPos < roomBounds.yMax; ++yPos)
                    {
                        Vector2Int tilePos = new Vector2Int(xPos, yPos);
                        roomBoundsPositions.Add(tilePos);
                    }
                }

                PaintTiles(roomBoundsPositions, _floorTilemap, visualTile);
            }
        }

        void PaintTiles(IEnumerable<Vector2Int> positions, Tilemap tileMap, TileBase tile)
        {
            foreach (var pos in positions)
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
        }
    }
}