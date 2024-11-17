using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JZK.Framework
{
    public class GameSceneInit : SceneInit
    {

        ISystemReference<MonoBehaviour>[] _systems = new ISystemReference<MonoBehaviour>[]
        {
            new SystemReference<Input.InputSystem>(),

            new SystemReference<UI.PressStartUISystem>(),
            new SystemReference<UI.UIStateSystem>(),

        };

        [SerializeField] private bool _useDebugSeed = false;
        [SerializeField] private int _debugSeed;
        private int _currentSeed;
        public int CurrentSeed => _currentSeed;

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
            _currentSeed = _useDebugSeed ? _debugSeed : System.DateTime.Now.Millisecond;
            UnityEngine.Random.InitState(_currentSeed);
        }

#if UNITY_EDITOR
        public void OnApplicationQuit()
        {

        }
#endif
    }
}