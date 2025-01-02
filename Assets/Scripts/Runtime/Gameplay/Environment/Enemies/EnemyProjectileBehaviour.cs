using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.Gameplay
{
    public enum EEnemyBehaviour
	{
        None = 0,
        Automatic = 1,
        OnPatrolTurning = 2,
	}

    public class EnemyProjectileBehaviour : MonoBehaviour
    {
        [SerializeField] EnemyController _parentController;

        [SerializeField] EProjectile _projectileType;
        public EProjectile ProjectileType => _projectileType;

        [SerializeField] EEnemyBehaviour _behaviourType;
        public EEnemyBehaviour BehaviourType => _behaviourType;

        [SerializeField] float _autoFireInterval = 2;
        public float AutoFireInterval => _autoFireInterval;

        [SerializeField] float _projectileSpeed = 5;
        [SerializeField] float _projectileLifetime = 10;

        float _timeSinceLastAutoFire;

        public void Initialise()
		{

		}
        
        public void UpdateProjectileBehaviour(float deltaTime)
		{
            if(_behaviourType == EEnemyBehaviour.Automatic)
			{
                UpdateAutoFire(deltaTime);
			}
		}

        void UpdateAutoFire(float deltaTime)
		{
            _timeSinceLastAutoFire += deltaTime;

            if(_timeSinceLastAutoFire >= _autoFireInterval)
			{
                _timeSinceLastAutoFire = 0;
                FireProjectile();
			}
		}

        void FireProjectile()
		{
            Vector2 projectileVel = new(0, 0);
            switch (_parentController.CurrentFacing)
            {
                case EOrthogonalDirection.Up:
                    projectileVel = new(0, 1);
                    break;
                case EOrthogonalDirection.Right:
                    projectileVel = new(1, 0);
                    break;
                case EOrthogonalDirection.Down:
                    projectileVel = new(0, -1);
                    break;
                case EOrthogonalDirection.Left:
                    projectileVel = new(-1, 0);
                    break;
            }

            ProjectileSystem.Instance.RequestProjectileLaunch(_projectileType, projectileVel, _projectileSpeed, transform.position, _projectileLifetime, 1, out _);
        }

        public void OnLevelPlacement()
		{
            _timeSinceLastAutoFire = 0;
		}

        public void ResetBehaviour()
		{

		}
    }
}