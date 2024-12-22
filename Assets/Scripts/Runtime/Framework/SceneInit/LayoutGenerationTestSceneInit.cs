using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JZK.Utility;
using System.Linq;
using Random = System.Random;
using UnityEngine.Tilemaps;
using JZK.Level;
using JZK.Gameplay;
using System;

namespace JZK.Framework
{
    public class LayoutGenerationTestSceneInit : SceneInit
    {
        ISystemReference<MonoBehaviour>[] _systems = new ISystemReference<MonoBehaviour>[]
        {
            new SystemReference<Input.InputSystem>(),

            new SystemReference<Level.DungeonLayoutGenerationSystem>(),
            new SystemReference<Level.RoomDefinitionLoadSystem>(),
            new SystemReference<Level.RoomLoadSystem>(),


            new SystemReference<UI.UIStateSystem>(),

        };

        [SerializeField] private bool _useDebugSeed = false;
        [SerializeField] private int _debugSeed;

        [SerializeField] bool _printDebug;

        [SerializeField] LayoutGenerationSettings _settings;

        private int _currentDebugTileIndex;

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

            _currentLayout = DungeonLayoutGenerationSystem.Instance.GenerateDungeonLayout(_settings, random);

            CreateDungeonFromLayoutData();
        }

        public void CreateDungeonFromLayoutData()
        {
            if (_currentLayout == null)
            {
                return;
            }

            Vector2 roomPrefabPos = Vector2.zero;
            int roomSpacing = 30;

            List<RoomController> activeControllers = new();

            Dictionary<Guid, RoomController> roomController_LUT = new();
            //Dictionary<Guid, RoomDoor> roomDoor_LUT = new();

            foreach(GenerationRoomData roomData in _currentLayout.Room_LUT.Values)
            {
                if(!RoomLoadSystem.Instance.RequestRoom(roomData.PrefabId, out RoomController controller))
                {
                    //complain
                }

                controller.DisableAllDoors();
                controller.gameObject.SetActive(true);
                controller.transform.position = roomPrefabPos;
                roomPrefabPos.x += roomSpacing;
                activeControllers.Add(controller);
                roomController_LUT.Add(roomData.Id, controller);
                for (int doorIndex = 0; doorIndex < controller.Doors.Count; doorIndex++)
                {
                    RoomDoor roomDoor = controller.Doors[doorIndex];
                    Guid doorDataId = roomData.AllDoorIds[doorIndex];
                    GenerationDoorData doorData = _currentLayout.Door_LUT[doorDataId];
                    //roomDoor_LUT.Add(doorData.Id, roomDoor);
                    //roomDoor.OnPlacedInGeneration(doorData);
                }
                //controller.OnPlacedInGeneration(roomData);
            }

            foreach(GenerationDoorData doorData in _currentLayout.Door_LUT.Values)
            {
                RoomDoor door = roomController_LUT[doorData.ParentRoomId].Doors[doorData.IndexInRoom];
                //GenerationDoorData doorGenData = _currentLayout.Door_LUT[doorId];
                door.SetDoorEnabled(doorData.Enabled);
                Guid linkDoorId = doorData.LinkDoorId;
                if (linkDoorId != Guid.Empty)
                {
                    GenerationDoorData linkDoorData = _currentLayout.Door_LUT[linkDoorId];
                    RoomDoor linkDoor = roomController_LUT[linkDoorData.ParentRoomId].Doors[linkDoorData.IndexInRoom];
                    door.LinkToDoor(linkDoor);
                    Debug.Log("[GENERATION] RUNTIME PLACEMENT: linked door " + doorData.Id.ToString() + " to door " + linkDoorData.Id.ToString());
                    RoomController startRoom = door.GetComponentInParent<RoomController>();
                    RoomController endRoom = linkDoor.GetComponentInParent<RoomController>();
                    if(startRoom == endRoom)
                    {
                        Debug.Log("[GENERATION] RUNTIME PLACEMENT: SAME ROOM");
                    }
                }
            }
            /*foreach(Guid doorId in roomDoor_LUT.Keys)
            {
                RoomDoor door = roomDoor_LUT[doorId];
                GenerationDoorData doorGenData = _currentLayout.Door_LUT[doorId];
                door.SetDoorEnabled(doorGenData.Enabled);
                Guid linkDoorId = doorGenData.LinkDoorId;
                if(linkDoorId != Guid.Empty)
                {
                    GenerationDoorData linkDoorData = _currentLayout.Door_LUT[linkDoorId];
                    RoomDoor linkDoor = roomDoor_LUT[linkDoorData.Id];
                    door.LinkToDoor(linkDoor);
                    Debug.Log("[GENERATION] RUNTIME PLACEMENT: linked door " + doorGenData.Id.ToString() + " to door " + linkDoorData.Id.ToString());
                }
                
                //door.LinkToDoor()
            }*/
        }

        public void ClearRooms()
		{
            if(null == _currentLayout)
			{
                return;
			}

            /*List<RoomController> activeCache = new(_currentLayout.ActiveRoomsList);
            foreach (RoomController room in activeCache)
            {
                RoomLoadSystem.Instance.ClearRoom(room);
            }*/

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
    }
}