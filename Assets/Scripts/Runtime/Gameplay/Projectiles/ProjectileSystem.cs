using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using JZK.Framework;
using System;

namespace JZK.Gameplay
{
	public enum EProjectile
	{
		None = 0,

		Gnome_Pellet = 1,
	}

	public class ProjectileSystem : PersistentSystem<ProjectileSystem>
	{
		#region PersistentSystem

		private SystemLoadData _loadData = new SystemLoadData()
		{
			LoadStates = new SystemLoadState[] { new SystemLoadState { LoadStartState = ELoadingState.GameData, BlockStateUntilFinished = ELoadingState.GameData } },
			UpdateAfterLoadingState = ELoadingState.GameData,
		};

		public override SystemLoadData LoadData => _loadData;

		public override void SetCallbacks()
		{
			base.SetCallbacks();
		}

		public override void StartLoading(ELoadingState state)
		{
			base.StartLoading(state);
			Addressables.LoadAssetsAsync<GameObject>("projectile_prefabs", LoadedAsset).Completed += LoadCompleted;
		}

		public override void UpdateSystem()
		{
			base.UpdateSystem();

			UpdateProjectiles(Time.timeSinceLevelLoad, Time.deltaTime);	//TODO: move to a gameplay system when that exists
		}

		#endregion //PersistentSystema



		public static int MAX_PROJECTILES_PER_TYPE = 30;

		private Dictionary<EProjectile, List<ProjectileController>> _projectilesPool = new();
		private List<ProjectileController> _activeProjectiles = new();

		public Vector2 PROJECTILE_RESET_POS => transform.position;

		public bool RequestProjectileLaunch(EProjectile type, Vector2 trajectory, float speed, Vector2 launchPos, float lifespan, float projectileScale, out Guid projectileID)
		{
			if (!GetProjectileOfTypeFromPool(type, out ProjectileController controller))
			{
				projectileID = Guid.Empty;
				return false;
			}

			controller.gameObject.SetActive(true);

			controller.transform.localScale = new Vector2(projectileScale, projectileScale);

			controller.Launch(trajectory, speed, launchPos, lifespan);
			projectileID = controller.ID;
			return true;
		}

		private bool GetProjectileOfTypeFromPool(EProjectile type, out ProjectileController controller)
		{
			controller = null;
			if (!_projectilesPool.TryGetValue(type, out List<ProjectileController> pool))
			{
				Debug.LogWarning("Tried to access projectile pool of invalid type " + type.ToString());
				return false;
			}

			if (pool.Count == 0)
			{
				Debug.LogWarning("Projectile pool for type " + type.ToString() + " is empty!");
				return false;
			}

			controller = pool[0];
			pool.Remove(controller);
			_activeProjectiles.Add(controller);

			return true;
		}

		public void UpdateProjectiles(float timePassed, float deltaTime)
		{
			List<ProjectileController> projectileInstanceCache = new(_activeProjectiles);

			foreach (ProjectileController projectile in projectileInstanceCache)
			{
				projectile.UpdateProjectile(deltaTime);
				if (projectile.TimeSinceFired >= projectile.Lifespan)
				{
					projectile.OnLifetimeEnd();
				}
			}
		}

		private void ReturnProjectileToPool(ProjectileController controller)
		{
			if (!_activeProjectiles.Contains(controller))
			{
				return;
			}

			_projectilesPool[controller.ProjectileType].Add(controller);
			_activeProjectiles.Remove(controller);
		}

		public void ClearProjectile(ProjectileController controller)
		{
			if (!_activeProjectiles.Contains(controller))
			{
				Debug.LogWarning(this.name + " - tried to clear projectile controller " + controller.ID.ToString() + " but it is not active - aborting action");
				return;
			}

			controller.transform.position = PROJECTILE_RESET_POS;
			controller.gameObject.SetActive(false);
			controller.ResetController();

			ReturnProjectileToPool(controller);
		}

		public void ClearAllActiveProjectiles()
		{
			foreach (ProjectileController projectile in _activeProjectiles)
			{
				projectile.transform.position = PROJECTILE_RESET_POS;
				projectile.gameObject.SetActive(false);
				projectile.ResetController();
			}

			ReturnAllActiveProjectilesToPool();
		}

		public void ReturnAllActiveProjectilesToPool()
		{
			List<ProjectileController> activeCache = new(_activeProjectiles);

			foreach (ProjectileController controller in activeCache)
			{
				ReturnProjectileToPool(controller);
			}
		}

		#region Load

		void LoadedAsset(GameObject asset)
		{

		}

		void LoadCompleted(AsyncOperationHandle<IList<GameObject>> assets)
		{
			if (!assets.IsDone)
			{
				//complain here
				return;
			}

			IList<GameObject> prefabs = assets.Result;
			if (prefabs == null)
			{
				//complain here
				return;
			}

			foreach (GameObject prefab in prefabs)
			{
				ProjectileController projectileComponent = prefab.GetComponent<ProjectileController>();

				if (projectileComponent == null)
				{
					Debug.LogError(this.name + " - tried to load projectile " + prefab.name + " with no ProjectileController component!!");
					continue;
				}

				EProjectile type = projectileComponent.ProjectileType;

				if (type == EProjectile.None)
				{
					Debug.Log(this.name + " - did not load projectile " + prefab.name + " as its type is None");
					continue;
				}

				List<ProjectileController> projectilesOfType = new(MAX_PROJECTILES_PER_TYPE);
				for (int projIndex = 0; projIndex < MAX_PROJECTILES_PER_TYPE; ++projIndex)
				{
					GameObject projectile = Instantiate(prefab);
					ProjectileController controller = projectile.GetComponent<ProjectileController>();
					controller.InitialiseOnLoad();
					projectilesOfType.Add(controller);
					projectile.SetActive(false);
					projectile.transform.SetParent(transform);
				}

				_projectilesPool.Add(type, projectilesOfType);
			}

			FinishLoading(ELoadingState.GameData);
		}

		#endregion //Load

		#region Callbacks

		public void OnLevelReset()
		{
			ClearAllActiveProjectiles();
		}

		#endregion //Callbacks

	}
}