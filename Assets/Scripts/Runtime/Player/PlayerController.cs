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

        bool _active;
        public bool Active => _active;

        public void UpdateController()
        {
            if (!_active)
            {
                return;
            }

            UpdateInput();
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