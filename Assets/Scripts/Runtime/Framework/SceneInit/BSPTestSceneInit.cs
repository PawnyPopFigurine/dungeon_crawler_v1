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

        //[SerializeField] private Tilemap _floorTilemap;
        [SerializeField] private TileBase _floortile;
        //[SerializeField] private Tilemap _wallTilemap;
        [SerializeField] private TileBase _wallTile_Full;

        [SerializeField] GameObject _roomSpaceParent;

        [SerializeField] DungeonGenerator _generator;

        [SerializeField] List<Color> _roomSpaceColours;

        [SerializeField] protected Vector2Int _startPos;
        [SerializeField]
        private int minRoomWidth = 4, minRoomHeight = 4;
        [SerializeField]
        private int dungeonWidth = 20, dungeonHeight = 20;
        [SerializeField]
        [Range(0, 10)]
        private int offset = 1;



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

            int roomSpaceIndex = 0;

            foreach ( var room in roomsList )
            {
                print("[GENERATION] Room " + roomSpaceIndex.ToString() + " has room at centre " + room.center.ToString() + " and position " + room.position.ToString() + " and size " + room.size.ToString());
                roomSpaceIndex++;
                if(true)
                {
                    GameObject roomSpaceGO = new GameObject();
                    roomSpaceGO.transform.SetParent(_roomSpaceParent.transform);
                    Tilemap roomSpaceTilemap = roomSpaceGO.AddComponent<Tilemap>();
                    TilemapRenderer tilemapRenderer = roomSpaceGO.AddComponent<TilemapRenderer>();
                    Color roomSpaceColour = GetRandomRoomSpaceColour(random);
                    roomSpaceTilemap.color = roomSpaceColour;

                    for (int column = 0; column < room.size.x; column++)
                    {
                        for (int row = 0; row < room.size.y; row++)
                        {
                            Vector2Int position = (Vector2Int)room.min + new Vector2Int(column, row);
                            PaintTile(roomSpaceTilemap, _floortile, position);
                        }
                    }
                }
                
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

        Color GetRandomRoomSpaceColour(Random random)
        {
            return _roomSpaceColours[random.Next(0, _roomSpaceColours.Count)];
        }

        public void ClearTiles()
        {
            foreach(Transform child in _roomSpaceParent.transform)
            {
                DestroyImmediate(child.gameObject);
            }
        }

        /*public void ClearTiles()
        {
            _floorTilemap.ClearAllTiles();
            _wallTilemap.ClearAllTiles();
        }*/
    }
}