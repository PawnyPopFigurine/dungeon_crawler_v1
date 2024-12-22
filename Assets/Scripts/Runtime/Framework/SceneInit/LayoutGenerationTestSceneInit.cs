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

            foreach(GenerationRoomData roomData in _currentLayout.Room_LUT.Values)
            {
                if(!RoomLoadSystem.Instance.RequestRoom(roomData.PrefabId, out RoomController controller))
                {
                    //complain
                }

                controller.gameObject.SetActive(true);
                controller.transform.position = roomPrefabPos;
                roomPrefabPos.x += roomSpacing;
            }
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