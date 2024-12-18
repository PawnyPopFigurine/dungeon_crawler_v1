using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace JZK.Gameplay
{
    public class RoomDoor : MonoBehaviour
    {

        [SerializeField] Tilemap _fillInWallTiles;
        [SerializeField] Sprite _shutSprite;
        [SerializeField] Sprite _openSprite;
        [SerializeField] Collider2D _doorCollision;
        [SerializeField] SpriteRenderer _renderer;
        [SerializeField] Collider2D _openCollisionLeft;
        [SerializeField] Collider2D _openCollisionRight;
        [SerializeField] GameObject _corridorGO;

        bool _doorEnabled;
        public bool DoorEnabled => _doorEnabled;

        bool _isOpen;

        RoomDoor _travelToDoor;
        public RoomDoor TravelToDoor => _travelToDoor;

        [SerializeField] Transform _setPlayerPos;
        public Transform SetPlayerPos => _setPlayerPos;


        public void SetDoorEnabled(bool enabled)
        {
            gameObject.SetActive(enabled);
            _doorEnabled = enabled;
            _fillInWallTiles.gameObject.SetActive(!enabled);
            if (null != _corridorGO)
			{
                _corridorGO.SetActive(enabled);
            }
        }

        public void SetIsOpen(bool isOpen)
        {
            if(!_doorEnabled)
            {
                return;
            }

            _doorCollision.enabled = !isOpen;
            _openCollisionLeft.enabled = isOpen;
            _openCollisionRight.enabled = isOpen;

            _renderer.sprite = isOpen ? _openSprite : _shutSprite;
        }

        public void LinkToDoor(RoomDoor newDestination, bool updateLinkingDoor = true)
		{
            if(null == newDestination)
			{
                _travelToDoor = null;
                return;
			}

            _travelToDoor = newDestination;
            if(updateLinkingDoor)
			{
                _travelToDoor.LinkToDoor(this, false);
            }
        }

        public void OnDoorEntered()
		{
            if(!_doorEnabled)
			{
                return;
			}

            if(null == _travelToDoor)
			{
                return;
			}

            if(!_travelToDoor.enabled)
			{
                return;
			}

            PlayerSystem.Instance.SetPlayerPos(_travelToDoor.SetPlayerPos.position);
		}
    }
}