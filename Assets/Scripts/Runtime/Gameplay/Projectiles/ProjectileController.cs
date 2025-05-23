using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace JZK.Gameplay
{
	public class ProjectileController : MonoBehaviour
	{
		protected Guid _id;
		public Guid ID => _id;

		[SerializeField] protected EProjectile _projectileType;
		public EProjectile ProjectileType => _projectileType;

		protected float _timeSinceFired = 0;
		public float TimeSinceFired => _timeSinceFired;

		protected Vector2 _trajectory;

		protected float _launchSpeed;
		[SerializeField] protected float _speedMult = 1;
		public float Speed => _launchSpeed * _speedMult;

		protected Vector2 _launchPos;

		protected float _lifespan;
		public float Lifespan => _lifespan;

		[SerializeField] SpriteRenderer _sprite;
		public SpriteRenderer Sprite => _sprite;

		[SerializeField] bool _rotateToMatchTrajectory;

		[SerializeField] bool _wobblyPattern;

		private Vector2 _perpendicularTrajectory;

		[SerializeField] float _frequency;

		[SerializeField] float _amplitude;

		public void InitialiseOnLoad()
		{
			_id = Guid.NewGuid();
		}

		public virtual void PrepareForLaunch()
		{
			//do stuff here
			if(_wobblyPattern)
			{
				float rotateByDegrees = 90;

				float rotateByRadians = rotateByDegrees * (Mathf.PI / 180);

				float rotatedShootDirectionX = (_trajectory.x * Mathf.Cos(rotateByRadians)) - (_trajectory.y * Mathf.Sin(rotateByRadians));
				float rotatedShootDirectionY = (_trajectory.x * Mathf.Sin(rotateByRadians)) + (_trajectory.y * Mathf.Cos(rotateByRadians));

				_perpendicularTrajectory = new(rotatedShootDirectionX, rotatedShootDirectionY);
			}
		}

		public void Launch(Vector2 trajectory, float speed, Vector2 launchPos, float lifespan)
		{
			transform.position = launchPos;

			SetTrajectory(trajectory);
			_launchSpeed = speed;
			_launchPos = launchPos;
			_lifespan = lifespan;

			if (_rotateToMatchTrajectory)
			{
				RotateVisualsToMatchTrajectory();
			}

			PrepareForLaunch();
		}

		public virtual void UpdateProjectile(float deltaTime)
		{
			UpdateLifetime(deltaTime);
			UpdateMovement(deltaTime);
		}

		protected void UpdateLifetime(float deltaTime)
		{
			_timeSinceFired += deltaTime;
		}

		public virtual void UpdateMovement(float deltaTime)
		{
			Vector2 nextPosition = new(transform.position.x, transform.position.y);
			if (_wobblyPattern)
			{
				nextPosition.x += _trajectory.x * Speed * deltaTime;
				nextPosition.y += _trajectory.y * Speed * deltaTime;

				float currentAmplitude = Mathf.Cos(_timeSinceFired * _frequency);
				nextPosition.x += currentAmplitude * _amplitude * _perpendicularTrajectory.x * deltaTime;
				nextPosition.y += currentAmplitude * _amplitude * _perpendicularTrajectory.y * deltaTime;
			}
			else
			{
				nextPosition.x += _trajectory.x * Speed * deltaTime;
				nextPosition.y += _trajectory.y * Speed * deltaTime;
			}
			

			transform.position = nextPosition;
		}

		public virtual void ResetController()
		{
			_timeSinceFired = 0;
		}

		public void FlipHorizontalTrajectory()
		{
			SetTrajectory(new(-_trajectory.x,
								_trajectory.y));
		}

		public void FlipVerticalTrajectory()
		{
			SetTrajectory(new(_trajectory.x,
								-_trajectory.y));
		}

		public void RotateVisualsToMatchTrajectory()
		{
			if (!_rotateToMatchTrajectory)
			{
				return;
			}

			float rotation = (float)(Math.Atan2(_trajectory.y, _trajectory.x) * (180 / Math.PI)) - 90;
			Quaternion newVisualsRotation = Quaternion.Euler(0, 0, rotation);

			transform.rotation = newVisualsRotation;
		}

		public void SetTrajectory(Vector2 newTrajectory)
		{
			_trajectory = newTrajectory;
		}

		public virtual void OnLifetimeEnd()
		{
			ProjectileSystem.Instance.ClearProjectile(this);
		}
	}
}