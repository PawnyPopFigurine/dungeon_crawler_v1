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

        [SerializeField] int _maxHealth;
        [SerializeField] float _invincibilityTime;

        float _timeSinceLastHit;

        int _currentHealth;
        public int CurrentHealth => _currentHealth;

        bool _playerAlive;
        public bool PlayerAlive => _playerAlive;

        bool _inInvincibilityPeriod;


        [SerializeField] Rigidbody2D _rigidbody;

        [SerializeField] SpriteRenderer _renderer;

        [SerializeField] Sprite _faceDown;
        [SerializeField] Sprite _faceUp;
        [SerializeField] Sprite _faceLeft;
        [SerializeField] Sprite _faceRight;

        bool _active;
        public bool Active => _active;

        private EOrthogonalDirection _faceDirection;

        public void Initialise()
		{
            _currentHealth = _maxHealth;
            _playerAlive = true;
		}

        public void UpdateController()
        {
            if (!_active)
            {
                return;
            }

            if(_playerAlive)
			{
                UpdateInput();
                UpdateFacingVisuals();
                UpdateShooting();
                UpdateInvincibilityPeriod();
            }

            _timeSinceLastHit += Time.deltaTime;
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

                ProjectileSystem.Instance.RequestProjectileLaunch(EProjectile.Gnome_Pellet, projectileVel, _projectileSpeed, transform.position, _projectileLifetime, 1, out _);
			}
		}

        void UpdateFacingVisuals()
		{
            if(InputSystem.Instance.PlayerMoveUp)
			{
                _faceDirection = EOrthogonalDirection.Up;
			}
            if(InputSystem.Instance.PlayerMoveDown)
			{
                _faceDirection = EOrthogonalDirection.Down;
			}
            if(InputSystem.Instance.PlayerMoveLeft)
			{
                _faceDirection = EOrthogonalDirection.Left;
			}
            if(InputSystem.Instance.PlayerMoveRight)
			{
                _faceDirection = EOrthogonalDirection.Right;
			}

            switch(_faceDirection)
			{
                case EOrthogonalDirection.Up:
                    _renderer.sprite = _faceUp;
                    break;
                case EOrthogonalDirection.Right:
                    _renderer.sprite = _faceRight;
                    break;
                case EOrthogonalDirection.Down:
                    _renderer.sprite = _faceDown;
                    break;
                case EOrthogonalDirection.Left:
                    _renderer.sprite = _faceLeft;
                    break;
			}
		}

        void UpdateInvincibilityPeriod()
        {
            if(!_inInvincibilityPeriod)
			{
                return;
			}

            _timeSinceLastHit += Time.deltaTime;

            Color invincibilityColour = new(1, 1, 1, 0.5f);

            if(_timeSinceLastHit >= _invincibilityTime)
			{
                _inInvincibilityPeriod = false;
                invincibilityColour = Color.white;
			}

            _renderer.color = invincibilityColour;
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

        public void OnPlayerHitHazard(GameObject hazard)
        {
            if (_currentHealth > 0 &&
                !_inInvincibilityPeriod)
			{
                _timeSinceLastHit = 0;
                _inInvincibilityPeriod = true;
                _currentHealth -= 1;
                if(_currentHealth <= 0)
				{
                    PlayerSystem.Instance.KillPlayer();
                    _playerAlive = false;
                    _rigidbody.velocity = Vector2.zero;
				}
			}
        }
    }
}