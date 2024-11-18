using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JZK.Framework;
using JZK.Gameplay;

namespace JZK.Camera
{
    public class CameraSystem : PersistentSystem<CameraSystem>
    {
		private SystemLoadData _loadData = new SystemLoadData()
		{
			LoadStates = SystemLoadState.NoLoadingNeeded,
			UpdateAfterLoadingState = ELoadingState.Game
		};

		public override SystemLoadData LoadData => _loadData;

		UnityEngine.Camera _camera;

		const float CAMERA_Z_POS = -10f;

		public override void Initialise()
		{
			base.Initialise();

			_camera = UnityEngine.Camera.main;
		}

		public override void UpdateSystem()
		{
			base.UpdateSystem();

			Vector2 playerPos = PlayerSystem.Instance.GetPlayerPos();
			if (playerPos != GameplayHelper.INVALID_PLAYER_POS)
			{
				_camera.transform.position = new(playerPos.x, playerPos.y, CAMERA_Z_POS);
			}
		}


	}
}