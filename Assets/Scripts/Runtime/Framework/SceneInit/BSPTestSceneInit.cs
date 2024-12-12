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

            new SystemReference<Level.DungeonLayoutGenerationSystem>(),
            new SystemReference<Level.RoomLoadSystem>(),

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

        //[SerializeField] protected Vector2Int _startPos;
        [SerializeField] LayoutGenerationSettings _settings;
        /*private int minRoomWidth = 4, minRoomHeight = 4;
        [SerializeField]
        private int dungeonWidth = 20, dungeonHeight = 20;*/

        [SerializeField] bool _showRoomBounds;

        private int _currentDebugTileIndex;

        [SerializeField] GameObject _roomPrefab;

        private List<BoundsInt> _roomBoundsList;

        private List<GameObject> _activeRoomsList;



        public void Start()
        {
            Setup(_systems);

            InitialiseSeed();

            /*_roomBoundsList = new List<BoundsInt>();
            _activeRoomsList = new List<GameObject>();

            GenerateDungeon();*/
        }


        private void Update()
        {
            UpdateScene();
        }

        void InitialiseSeed()
        {
            _currentSeed = _useDebugSeed ? _debugSeed : System.DateTime.Now.Millisecond;
            UnityEngine.Random.InitState(_currentSeed);

            if(_useDebugSeed)
			{
                _settings.Seed = _debugSeed;
			}
        }

        public void GenerateDungeon()
        {
            System.Random random = new(_settings.Seed);

            DungeonLayoutGenerationSystem.Instance.GenerateLayout(_settings);
           // var roomsList = ProceduralGeneration.BinarySpacePartitioning(new BoundsInt((Vector3Int)_settings.StartPos, new Vector3Int(dungeonWidth, dungeonHeight, 0)), minRoomWidth, minRoomHeight, random);

            if (_showRoomBounds)
			{
                VisualiseRoomBounds(DungeonLayoutGenerationSystem.Instance.RoomBoundsList, random);
            }

            DungeonLayoutGenerationSystem.Instance.SimpleRoomPrefabPlacement(random, _floorTilemap);
            //SimpleRoomPrefabPlacement(random);
        }

        void SimpleRoomPrefabPlacement(Random random)
		{
            foreach(BoundsInt roomBounds in _roomBoundsList)
			{
                CreateRoomAtCoordinate(roomBounds.center, _roomPrefab);
			}
		}

        void CreateRoomAtCoordinate(Vector3 coordinate, GameObject prefab)
		{
            Vector3Int intCoord = new Vector3Int((int)coordinate.x, (int)coordinate.y, (int)coordinate.z);
            GameObject roomGO = Instantiate(prefab);
            _activeRoomsList.Add(roomGO);

            Vector3 roomWorldPos = _floorTilemap.CellToWorld(intCoord);
            roomGO.transform.position = roomWorldPos;
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

        public void ClearRooms()
		{
            List<GameObject> activeCache = new(_activeRoomsList);
            foreach(GameObject room in activeCache)
			{
                DestroyImmediate(room);
			}

            _activeRoomsList.Clear();
		}

        public override void LoadingStateComplete(ELoadingState state)
        {
            switch (state)
            {
                case ELoadingState.Game:
                    GenerateDungeon();
                    break;
            }
        }
    }
}