using UnityEngine;

namespace JZK.Framework
{
	public class PlayerTestSceneInit : SceneInit
	{

		ISystemReference<MonoBehaviour>[] _systems = new ISystemReference<MonoBehaviour>[]
		{
			new SystemReference<Camera.CameraSystem>(),

			new SystemReference<Input.InputSystem>(),

			new SystemReference<Gameplay.PlayerSystem>(),
			new SystemReference<Gameplay.ProjectileSystem>(),

			new SystemReference<UI.PressStartUISystem>(),
			new SystemReference<UI.UIStateSystem>(),

		};

		[SerializeField] private bool _useDebugSeed = false;
		[SerializeField] private int _debugSeed;
		private int _currentSeed;
		public int CurrentSeed => _currentSeed;

		public void Start()
		{
			_isTestScene = true;

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

		public override void LoadingStateComplete(ELoadingState state)
		{
			base.LoadingStateComplete(state);

			if(state != ELoadingState.Game)
			{
				return;
			}

			Gameplay.PlayerSystem.Instance.StartForPlayerTestScene();
		}

	}
}