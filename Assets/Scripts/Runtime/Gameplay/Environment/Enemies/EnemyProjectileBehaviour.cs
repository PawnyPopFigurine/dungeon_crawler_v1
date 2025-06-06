using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.Gameplay
{
    public enum EEnemyFiringBehaviour
	{
        None = 0,
        Automatic = 1,
        OnPatrolTurning = 2,
	}

    public enum EEnemyFiringPattern
	{
        None = 0,
        SingleStraight = 1,
        TwinStraight = 2,
        TwinSpread = 3,
        AimAtPlayer = 4,
        X_Shape = 5,
	}

    public class EnemyProjectileBehaviour : MonoBehaviour
    {
        [SerializeField] EnemyController _parentController;

        [SerializeField] EProjectile _projectileType;
        public EProjectile ProjectileType => _projectileType;

        [SerializeField] EEnemyFiringBehaviour _behaviourType;
        public EEnemyFiringBehaviour BehaviourType => _behaviourType;

        [SerializeField] EEnemyFiringPattern _patternType;
        public EEnemyFiringPattern PatternType => _patternType;

        [SerializeField] float _autoFireInterval = 2;
        public float AutoFireInterval => _autoFireInterval;

        [SerializeField] float _projectileSpeed = 5;
        [SerializeField] float _projectileLifetime = 10;

        float _timeSinceLastAutoFire;

        [SerializeField] float _straightSpreadShotDistance = 0.4f;
        [SerializeField] float _spreadShotRotation = 35f;

        public void Initialise()
		{

		}
        
        public void UpdateProjectileBehaviour(float deltaTime)
		{
            if(_behaviourType == EEnemyFiringBehaviour.Automatic)
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

        public void OnPatrolTurning()
		{
            if(_behaviourType != EEnemyFiringBehaviour.OnPatrolTurning)
			{
                return;
			}

            FireProjectile();
		}

        void FireProjectile()
		{
            switch(_patternType)
			{
                case EEnemyFiringPattern.SingleStraight:
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
                    break;
                case EEnemyFiringPattern.TwinStraight:
					{
                        Vector3 launchPos1 = transform.position;
                        Vector3 launchPos2 = transform.position;

                        float twinStraightDistance = _straightSpreadShotDistance;

                        Vector2 projectileVel = new(0, 0);
                        switch (_parentController.CurrentFacing)
                        {
                            case EOrthogonalDirection.Up:
                                projectileVel = new(0, 1);
                                launchPos1.x -= twinStraightDistance;
                                launchPos2.x += twinStraightDistance;
                                break;
                            case EOrthogonalDirection.Right:
                                projectileVel = new(1, 0);
                                launchPos1.y -= twinStraightDistance;
                                launchPos2.y += twinStraightDistance;
                                break;
                            case EOrthogonalDirection.Down:
                                projectileVel = new(0, -1);
                                launchPos1.x += twinStraightDistance;
                                launchPos2.x -= twinStraightDistance;
                                break;
                            case EOrthogonalDirection.Left:
                                projectileVel = new(-1, 0);
                                launchPos1.y += twinStraightDistance;
                                launchPos2.y -= twinStraightDistance;
                                break;
                        }

                        ProjectileSystem.Instance.RequestProjectileLaunch(_projectileType, projectileVel, _projectileSpeed, launchPos1, _projectileLifetime, 1, out _);
                        ProjectileSystem.Instance.RequestProjectileLaunch(_projectileType, projectileVel, _projectileSpeed, launchPos2, _projectileLifetime, 1, out _);
                    }
                    break;
                case EEnemyFiringPattern.TwinSpread:
					{
                        for(int directionIndex = 0; directionIndex <= 1; ++directionIndex)
						{
                            float rotateByDegrees = 0;
                            switch (directionIndex)
                            {
                                //left
                                case 0:
                                    rotateByDegrees = -_spreadShotRotation;
                                    break;
                                //right
                                case 1:
                                    rotateByDegrees = _spreadShotRotation;
                                    break;
                            }

                            Vector2 shootDirection = GameplayHelper.GetRotatedVectorForDirection(_parentController.CurrentFacing, rotateByDegrees);

                            if (!ProjectileSystem.Instance.RequestProjectileLaunch(_projectileType, shootDirection, _projectileSpeed, transform.position, _projectileLifetime, 1, out _))
                            {

                                //complain here
                            };
                        }
					}
                    break;
                case EEnemyFiringPattern.AimAtPlayer:
					{
                        Vector2 playerPos = PlayerSystem.Instance.GetPlayerPos();

                        Vector2 shootDirection = playerPos - (Vector2)transform.position;
                        shootDirection.Normalize();
                        if (!ProjectileSystem.Instance.RequestProjectileLaunch(_projectileType, shootDirection, _projectileSpeed, transform.position, _projectileLifetime, 1, out _))
                        {

                            //complain here
                        };
                    }
                    break;
                case EEnemyFiringPattern.X_Shape:
                    for(int directionIndex = 0; directionIndex <= 3; ++directionIndex)
					{
                        float rotateDegrees = -1;
                        switch(directionIndex)
						{
                            case 0:
                                rotateDegrees = 45;
                                break;
                            case 1:
                                rotateDegrees = 135;
                                break;
                            case 2:
                                rotateDegrees = 225;
                                break;
                            case 3:
                                rotateDegrees = 315;
                                break;
						}

                        Vector2 shootDirection = GameplayHelper.GetRotatedVectorForDirection(_parentController.CurrentFacing, rotateDegrees);

                        if (!ProjectileSystem.Instance.RequestProjectileLaunch(_projectileType, shootDirection, _projectileSpeed, transform.position, _projectileLifetime, 1, out _))
                        {

                            //complain here
                        };
                    }
                    break;
			}
            
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