using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JZK.Utility;
using System.Linq;
using Random = System.Random;
using UnityEngine.Tilemaps;
using JZK.Level;
using JZK.Gameplay;

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



        [SerializeField] bool _printDebug;

        [SerializeField] private Tilemap _floorTilemap;
        [SerializeField] private TileBase _floortile;

        [SerializeField] private TileBase _wallTile_Full;

        [SerializeField] List<Tile> DebugRoomBoundsTiles;

        [SerializeField] LayoutGenerationSettings _settings;

        [SerializeField] bool _showRoomBounds;

        private int _currentDebugTileIndex;

        [SerializeField] RoomController _roomPrefab;

        LayoutData _currentLayout;



        public void Start()
        {
            Setup(_systems);

            InitialiseSeed();
        }


        private void Update()
        {
            UpdateScene();
        }

        void InitialiseSeed()
        {
            _settings.Seed = _useDebugSeed ? _debugSeed : System.DateTime.Now.Millisecond;
            UnityEngine.Random.InitState(_settings.Seed);
        }

        public void GenerateDungeon()
        {
            InitialiseSeed();

            System.Random random = new(_settings.Seed);

            _currentLayout = DungeonLayoutGenerationSystem.Instance.GenerateDungeonLayout(_settings, random, _floorTilemap);

            if (_showRoomBounds)
			{
                VisualiseRoomBounds(_currentLayout.RoomBoundsList, random);
            }

            SimpleRoomPrefabPlacement(random, _floorTilemap, _currentLayout);

            RoomController room1 = _currentLayout.ActiveRoomsList[0];
            RoomController room2 = _currentLayout.ActiveRoomsList[1];

            TryConnectRooms(room1, room2);
        }

        void TryConnectRooms(RoomController room1, RoomController room2)
		{
            FindDoorsWithShortestPath(room1, room2, out RoomDoor door1, out RoomDoor door2);

            Debug.Log("[DOORS] door 1 is " + door1.name);
            Debug.Log("[DOORS] door 2 is " + door2.name);
		}

        void FindDoorsWithShortestPath(RoomController room1, RoomController room2, out RoomDoor room1Door, out RoomDoor room2Door)
		{
            room1Door = null;
            room2Door = null;

            float shortestDistance = float.MaxValue;

            foreach(RoomDoor doorRoom1 in room1.Doors)
			{
                Vector3 doorPos1 = doorRoom1.transform.position;

                foreach(RoomDoor doorRoom2 in room2.Doors)
				{
                    Vector3 doorPos2 = doorRoom2.transform.position;

                    float distance = Vector3.Distance(doorPos1, doorPos2);
                    if(distance < shortestDistance)
					{
                        room1Door = doorRoom1;
                        room2Door = doorRoom2;
                        shortestDistance = distance;
					}
				}
			}
		}

        void TryConnectDoors(RoomDoor door1, RoomDoor door2)
		{

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
            if(null == _currentLayout)
			{
                return;
			}

            List<RoomController> activeCache = new(_currentLayout.ActiveRoomsList);
            foreach (RoomController room in activeCache)
            {
                RoomLoadSystem.Instance.ClearRoom(room);
            }

            _currentLayout = null;
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

        void SimpleRoomPrefabPlacement(System.Random random, Tilemap tileMap, LayoutData layoutData)
        {
            layoutData.ActiveRoomsList.Clear();
            List<RoomController> activeRoomsCache = new();

            int numRooms = 0;
            int maxRooms = 2;

            foreach (BoundsInt roomBounds in layoutData.RoomBoundsList)
            {
                if(numRooms >= maxRooms)
				{
                    continue;
				}
                if(RoomLoadSystem.Instance.RequestRoom(_roomPrefab.Id, out RoomController controller))
				{
                    controller.transform.parent = transform;
                    Vector3Int intCentreCoord = new((int)roomBounds.center.x, (int)roomBounds.center.y, (int)roomBounds.center.z);
                    Debug.Log("[HELLO] placing room " + controller.name + " at bounds centre " + intCentreCoord.ToString());
                    Vector3 roomWorldPos = tileMap.CellToWorld(intCentreCoord);
                    controller.transform.position = roomWorldPos;
                    controller.gameObject.SetActive(true);
                    activeRoomsCache.Add(controller);
                    controller.DisableAllDoors();
                    //controller.OnGridPlacement();
                    List<(int, int)> roomFloorPositions = controller.GetFloorNodePositions(intCentreCoord);
                    foreach ((int, int) pos in roomFloorPositions)
                    {
                        layoutData.Nodes[pos.Item1, pos.Item2].IsFloor = true;
                    }

                    List<(int, int)> roomWallPositions = controller.GetWallNodePositions(intCentreCoord);
                    foreach ((int, int) pos in roomWallPositions)
                    {
                        layoutData.Nodes[pos.Item1, pos.Item2].IsWall = true;
                    }
                }
                numRooms++;
            }

            layoutData.SetActiveRooms(activeRoomsCache);
        }
    }
}