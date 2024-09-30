using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.InputSystem;
using Framework;

namespace Input
{
	public enum EControllerType
	{
		Mouse,
		Keyboard,
		Gamepad,

	}

	public enum EControllerPlatformType
	{
		None,
		Xbox,
		Playstation,
		Switch,

	}

	public class InputSystem : PersistentSystem<InputSystem>
	{
		private static readonly string[] INPUT_ACTION_MAP_IDS =
		{
			"UI",
			//"Player",
		};

		public delegate void ControllerChangeEvent(EControllerType previous, EControllerType current);
		public delegate void InputEvent();
		public event ControllerChangeEvent OnControllerTypeChanged;
		public event InputEvent OnControllerPlatformTypeChanged;

		[System.Serializable]
		public class ControlState
		{
			[SerializeField]
			bool _currentState;
			bool _previousState;

			public void SetState(bool state)
			{
				_previousState = _currentState;
				_currentState = state;
			}

			public void Clear()
			{
				_previousState = false;
				_currentState = false;
			}

			public bool IsDown
			{
				get => _currentState;
			}

			public bool Pressed
			{
				get => !_previousState && _currentState;
			}

			public bool Released
			{
				get => _previousState && !_currentState;
			}
		}

		protected InputSystem() { }

		public Vector2 MoveCursor { get; private set; }
		public Vector2 MousePosition { get; private set; }

		public bool MouseOverlapsUI { get; private set; }
		public bool FastForward { get; private set; }

		public bool UIConfirm { get; private set; }
		public bool UIConfirmPressed { get; private set; }

		public bool UICancel { get; private set; }
		public bool UICancelPressed { get; private set; }

		/*public bool UIQuickRespawn { get; private set; }
		public bool UIQuickRespawnPressed { get; private set; }

		public bool LevelPause { get; private set; }
		public bool LevelPausePressed { get; private set; }

		public float PlayerMoveHorizontal { get; private set; }
		public bool PlayerMoveLeft { get; private set; }
		public bool PlayerMoveLeftPressed { get; private set; }
		public bool PlayerMoveRight { get; private set; }
		public bool PlayerMoveRightPressed { get; private set; }
		public bool PlayerJump { get; private set; }
		public bool PlayerJumpPressed { get; private set; }
		public bool PlayerJumpReleased { get; private set; }
		public bool PlayerJumpHeld { get; private set; }
		public bool PlayerDash { get; private set; }
		public bool PlayerDashPressed { get; private set; }
		public bool PlayerDashHeld { get; private set; }*/

		EControllerType _lastControllerType;
		public EControllerType LastControllerType
		{
			get { return _lastControllerType; }
			set
			{
				EControllerType prevType = _lastControllerType;
				bool change = _lastControllerType != value;
				_lastControllerType = value;
				if (change)
				{
					OnControllerTypeChanged?.Invoke(prevType, _lastControllerType);
				}
			}
		}

		EControllerPlatformType _lastControllerPlatformType;
		public EControllerPlatformType LastControllerPlatformType
		{
			get { return _lastControllerPlatformType; }
			set
			{
				bool change = _lastControllerPlatformType != value;
				_lastControllerPlatformType = value;
				if(change)
				{
					OnControllerPlatformTypeChanged?.Invoke();
				}
			}
		}

		public InputDevice LastInputDevice;

		private PlayerInput _playerInput;
		private int InputDelay = 0;

		#region PersistentSystem

		private SystemLoadData _loadData = new SystemLoadData()
		{
			LoadStates = new SystemLoadState[] { new SystemLoadState { LoadStartState = ELoadingState.FrontEndData, BlockStateUntilFinished = ELoadingState.FrontEndData } },
			UpdateAfterLoadingState = ELoadingState.FrontEndData,
		};

		public override SystemLoadData LoadData
		{
			get { return _loadData; }
		}

		public override void StartLoading(ELoadingState state)
		{
			base.StartLoading(state);

			Load();
		}

		public override void UpdateSystem()
		{
			//base.UpdateSystem();

#if !UNITY_EDITOR
			if(!Application.isFocused)
			{
				Clear();
				return;
			}
#endif

			if (InputDelay > 0)
			{
				InputDelay -= 1;
				return;
			}

			UICancelPressed = false;
			UIConfirmPressed = false;

			/*UIQuickRespawnPressed = false;

			PlayerMoveLeftPressed = false;
			PlayerMoveRightPressed = false;

			PlayerJumpPressed = false;
			PlayerJumpReleased = false;

			PlayerJumpHeld = false;

			LevelPausePressed = false;

			PlayerDashHeld = false;
			PlayerDashPressed = false;*/

			EControllerPlatformType platformType = LastControllerPlatformType;
			LastControllerType = GetCurrentController(out platformType, out LastInputDevice);
			LastControllerPlatformType = platformType;

#if (UNITY_XBOXONE || UNITY_PS4 || UNITY_GAMECORE || UNITY_SWITCH) && !UNITY_EDITOR
			MousePosition = new Vector2(-9999, -9999);
			Cursor.lockState = CursorLockMode.Locked;
#else
			MousePosition = Mouse.current.position.ReadValue();
#endif

			#region UI
			//UI Cancel
			bool lastUICancel = UICancel;

			UICancel = inputAction_Cancel.triggered;

			if (UICancel && !lastUICancel)
			{
				UICancelPressed = true;
			}


			//UI Confirm
			bool lastUIConfirm = UIConfirm;

			UIConfirm = inputAction_Confirm.ReadValue<float>() > 0;

			if (UIConfirm && !lastUIConfirm)
			{
				UIConfirmPressed = true;
			}

			//UI Quick Restart
			/*bool lastQuickRespawn = UIQuickRespawn;

			UIQuickRespawn = inputAction_QuickRespawn.ReadValue<float>() > 0;

			if(UIQuickRespawn && !lastQuickRespawn)
			{
				UIQuickRespawnPressed = true;
			}*/
			#endregion //UI

			#region PlayerMovement

			//Move Horizontal
			/*bool lastMoveLeft = PlayerMoveLeft;
			bool lastMoveRight = PlayerMoveRight;

			PlayerMoveHorizontal = inputAction_PlayerMoveHorizontal.ReadValue<float>();

			PlayerMoveLeft = (PlayerMoveHorizontal < 0);
			PlayerMoveRight = (PlayerMoveHorizontal > 0);

			if (!lastMoveLeft && PlayerMoveLeft)
			{
				PlayerMoveLeftPressed = true;
			}

			if (!lastMoveRight && PlayerMoveRight)
			{
				PlayerMoveRightPressed = true;
			}*/

			//Jump
			/*bool lastPlayerJump = PlayerJump;

			PlayerJump = (inputAction_PlayerJump.ReadValue<float>() > 0.5f);

			if (!lastPlayerJump && PlayerJump)
			{
				PlayerJumpPressed = true;
			}

			if (lastPlayerJump && !PlayerJump)
			{
				PlayerJumpReleased = true;
			}

			if (lastPlayerJump && PlayerJump)
			{
				PlayerJumpHeld = true;
			}*/

			#endregion //PlayerMovement

			//Pause
			/*bool lastPause = LevelPause;
			LevelPause = inputAction_LevelPause.triggered;

			if (!lastPause && LevelPause)
			{
				LevelPausePressed = true;
			}*/

			//Dash
			/*bool lastPlayerDash = PlayerDash;

			PlayerDash = (inputAction_PlayerDash.ReadValue<float>() > 0.5f);

			if (!lastPlayerDash && PlayerDash)
			{
				PlayerDashPressed = true;
			}

			if (lastPlayerDash && !PlayerDash)
			{
				//PlayerDashReleased = true;
			}

			if (lastPlayerDash && PlayerDash)
			{
				PlayerDashHeld = true;
			}*/
		}
		#endregion // PersistentSystem

			#region Load

			public void Load()
		{
			Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Input/PlayerInput.prefab").Completed += InputLoadCompleted;
		}

		void InputLoadCompleted(AsyncOperationHandle<GameObject> op)
		{
			if(op.Result == null)
			{
				Debug.LogError(this.GetType().ToString() + "- Failed to load addressable.");
				return;
			}

			_initPerfMarker.Begin(this);
			float startTime = Time.realtimeSinceStartup;

			_playerInput = PlayerInput.Instantiate(op.Result);

			SetUpActionMaps(_playerInput);
			SetUpActions();

			FinishLoading(ELoadingState.FrontEndData);

			float endTime = Time.realtimeSinceStartup - startTime;
			_initPerfMarker.End();
			Debug.Log("INIT: " + GetType() + ".InputLoadCompleted " + endTime.ToString("F2") + " sec.)");

		}

#endregion //Load

		void SetUpActionMaps(PlayerInput playerInput)
		{
#if UNITY_SWITCH
			//SetUpActionMapsForSwitch(playerInput)
			return;
#else
			foreach(string inputActionMapId in INPUT_ACTION_MAP_IDS)
			{
				playerInput.actions.FindActionMap(inputActionMapId).Enable();
			}
#endif  //UNITY_SWITCH
		}

		private InputAction inputAction_Confirm;
		private InputAction inputAction_Cancel;

		/*private InputAction inputAction_PlayerMoveHorizontal;
		private InputAction inputAction_PlayerJump;

		private InputAction inputAction_LevelPause;

		private InputAction inputAction_PlayerDash;

		private InputAction inputAction_QuickRespawn;*/

		private void SetUpActions()
		{
			inputAction_Cancel = _playerInput.actions["UI/Cancel"];
			inputAction_Confirm = _playerInput.actions["UI/Confirm"];

			/*inputAction_PlayerMoveHorizontal = _playerInput.actions["Player/MoveHorizontal"];
			inputAction_PlayerJump = _playerInput.actions["Player/Jump"];

			inputAction_LevelPause = _playerInput.actions["Player/Pause"];

			inputAction_PlayerDash = _playerInput.actions["Player/Run"];

			inputAction_QuickRespawn = _playerInput.actions["UI/QuickRespawn"];*/
		}

		public void Clear()
		{
			UICancel = false;
			UICancelPressed = false;

			UIConfirm = false;
			UIConfirmPressed = false;

			/*PlayerJump = false;
			PlayerJumpHeld = false;
			PlayerJumpPressed = false;
			PlayerJumpReleased = false;

			PlayerMoveHorizontal = 0;
			PlayerMoveLeft = false;
			PlayerMoveRight = false;

			LevelPause = false;
			LevelPausePressed = false;

			PlayerDash = false;
			PlayerDashHeld = false;
			PlayerDashPressed = false;

			UIQuickRespawn = false;
			UIQuickRespawnPressed = false;*/
		}

		EControllerType GetCurrentController(out EControllerPlatformType lastPlatformType, out InputDevice lastDevice)
		{
			lastDevice = LastInputDevice;
			EControllerType mostRecentType = LastControllerType;

			InputDevice activeDevice = null;
			InputDevice activeNonGamepad = null;
			InputDevice activeGamepad = null;
			double lastUpdate = 0;
			lastPlatformType = LastControllerPlatformType;

			bool hasKeyboard = Keyboard.current != null;
			bool hasMouse = Mouse.current != null;
			bool hasGamepad = Gamepad.current != null;

			if(hasGamepad && Gamepad.current.lastUpdateTime > lastUpdate)
			{
				mostRecentType = EControllerType.Gamepad;
				activeDevice = Gamepad.current;
				activeGamepad = Gamepad.current;
				lastUpdate = activeDevice.lastUpdateTime;
			}

			if(hasKeyboard && Keyboard.current.lastUpdateTime > lastUpdate)
			{
				mostRecentType = EControllerType.Keyboard;
				activeDevice = Keyboard.current;
				activeGamepad = Keyboard.current;
				lastUpdate = activeDevice.lastUpdateTime;
			}

			if (hasMouse && Mouse.current.lastUpdateTime > lastUpdate)
			{
				mostRecentType = EControllerType.Mouse;
				activeDevice = Mouse.current;
				activeGamepad = Mouse.current;
				lastUpdate = activeDevice.lastUpdateTime;
			}

			if(activeDevice == lastDevice)
			{
				lastPlatformType = LastControllerPlatformType;
				return LastControllerType;
			}

			if(hasGamepad && Gamepad.current.lastUpdateTime == lastUpdate && IsGamepadInputAcceptable(Gamepad.current))
			{
				lastDevice = activeGamepad;
#if UNITY_SWITCH
				lastPlatformType = EControllerPlatformType.Switch;
#else
				if(Gamepad.current is UnityEngine.InputSystem.XInput.XInputController)
				{
					lastPlatformType = EControllerPlatformType.Xbox;
				}
				else if(Gamepad.current is UnityEngine.InputSystem.DualShock.DualShockGamepad)
				{
					lastPlatformType = EControllerPlatformType.Playstation;
				}
				else if (Gamepad.current is UnityEngine.InputSystem.Switch.SwitchProControllerHID)
				{
					lastPlatformType = EControllerPlatformType.Switch;
				}
#endif
				return EControllerType.Gamepad;
			}
			else
			{
				lastDevice = activeNonGamepad;
#if UNITY_GAMECORE
				lastPlatformType = EControllerPlatformType.Xbox;
#elif UNITY_SWITCH
				lastPlatformType =e EControllerPlatformType.Switch;
#else
				lastPlatformType = EControllerPlatformType.None;
#endif
				return mostRecentType;
			}

		}

		private bool IsGamepadInputAcceptable(Gamepad gamepad)
		{
			for(int controlIndex = 0; controlIndex < gamepad.allControls.Count; ++controlIndex)
			{
				InputControl control = gamepad.allControls[controlIndex];
				if (control.valueType == typeof(float))
				{
					float inputFloat = (float)control.ReadValueAsObject();
					if (inputFloat >= 0.2f)
					{
						return true;
					}
				}
				else if (control.valueType == typeof(Vector2))
				{
					Vector2 inputAxes = (Vector2)control.ReadValueAsObject();
					if (inputAxes.sqrMagnitude >= 0.5f)
					{
						return true;
					}
				}
			}

			return false;
		}

		public void UseSelectEvent()
		{
			UICancel = false;
			UICancelPressed = false;

			UIConfirm = false;
			UIConfirmPressed = false;
		}


	}
}