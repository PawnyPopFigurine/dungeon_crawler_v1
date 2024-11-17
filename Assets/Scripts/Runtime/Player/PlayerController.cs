using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JZK.Input;

namespace JZK.Gameplay
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] float _walkSpeed;
        [SerializeField] float _projectileSpeed;
        [SerializeField] float _projectileLifetime;


        [SerializeField] Rigidbody2D _rigidbody;

        [SerializeField] SpriteRenderer _renderer;

        [SerializeField] Sprite _faceDown;
        [SerializeField] Sprite _faceUp;
        [SerializeField] Sprite _faceLeft;
        [SerializeField] Sprite _faceRight;

        bool _active;
        public bool Active => _active;

        private EFacing _faceDirection;

        public void UpdateController()
        {
            if (!_active)
            {
                return;
            }

            UpdateInput();
            UpdateFacingVisuals();
            UpdateShooting();
        }

        void UpdateInput()
		{
            UpdateWalking();
		}

        void UpdateWalking()
		{
            float newVelocityX;
            float newVelocityY;

            if (InputSystem.Instance.PlayerMoveHorizontal != 0)
			{
                newVelocityX = InputSystem.Instance.PlayerMoveHorizontal * _walkSpeed;
			}
            else
			{
                newVelocityX = 0;
			}

            if(InputSystem.Instance.PlayerMoveVertical != 0)
			{
                newVelocityY = InputSystem.Instance.PlayerMoveVertical * _walkSpeed;
			}
            else
			{
                newVelocityY = 0;
			}

            _rigidbody.velocity = new(newVelocityX, newVelocityY);
		}

        void UpdateShooting()
		{
            if(InputSystem.Instance.PlayerShootPressed)
			{
                Vector2 projectileVel = new(0, 0);
                switch(_faceDirection)
				{
                    case EFacing.Up:
                        projectileVel = new(0, 1);
                        break;
                    case EFacing.Right:
                        projectileVel = new(1, 0);
                        break;
                    case EFacing.Down:
                        projectileVel = new(0, -1);
                        break;
                    case EFacing.Left:
                        projectileVel = new(-1, 0);
                        break;
				}

                ProjectileSystem.Instance.RequestProjectileLaunch(EProjectile.Gnome_Pellet, projectileVel, _projectileSpeed, transform.position, _projectileLifetime, 1, out _);
			}
		}

        void UpdateFacingVisuals()
		{
            if(InputSystem.Instance.PlayerMoveUp)
			{
                _faceDirection = EFacing.Up;
			}
            if(InputSystem.Instance.PlayerMoveDown)
			{
                _faceDirection = EFacing.Down;
			}
            if(InputSystem.Instance.PlayerMoveLeft)
			{
                _faceDirection = EFacing.Left;
			}
            if(InputSystem.Instance.PlayerMoveRight)
			{
                _faceDirection = EFacing.Right;
			}

            switch(_faceDirection)
			{
                case EFacing.Up:
                    _renderer.sprite = _faceUp;
                    break;
                case EFacing.Right:
                    _renderer.sprite = _faceRight;
                    break;
                case EFacing.Down:
                    _renderer.sprite = _faceDown;
                    break;
                case EFacing.Left:
                    _renderer.sprite = _faceLeft;
                    break;
			}
		}


        public void SetActive(bool active)
		{
            if(_active == active)
			{
                return;
			}

            gameObject.SetActive(active);
            _active = active;
		}
    }
}