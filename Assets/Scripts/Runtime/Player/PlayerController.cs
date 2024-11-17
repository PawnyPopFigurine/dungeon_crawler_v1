using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JZK.Input;

namespace JZK.Gameplay
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] float _walkSpeed;
        [SerializeField] Rigidbody2D _rigidbody;

        [SerializeField] SpriteRenderer _renderer;

        [SerializeField] Sprite _faceDown;
        [SerializeField] Sprite _faceUp;
        [SerializeField] Sprite _faceLeft;
        [SerializeField] Sprite _faceRight;

        bool _active;
        public bool Active => _active;

        public void UpdateController()
        {
            if (!_active)
            {
                return;
            }

            UpdateInput();
            UpdateFacingVisuals();
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

        void UpdateFacingVisuals()
		{
            if(InputSystem.Instance.PlayerMoveUp)
			{
                _renderer.sprite = _faceUp;
			}

            if(InputSystem.Instance.PlayerMoveDown)
			{
                _renderer.sprite = _faceDown;
			}

            if(InputSystem.Instance.PlayerMoveLeft)
			{
                _renderer.sprite = _faceLeft;
			}

            if(InputSystem.Instance.PlayerMoveRight)
			{
                _renderer.sprite = _faceRight;
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