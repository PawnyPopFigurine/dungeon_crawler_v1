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

        bool _doorEnabled;
        public bool DoorEnabled => _doorEnabled;

        bool _isOpen;

        public void SetDoorEnabled(bool enabled)
        {
            gameObject.SetActive(enabled);
            _doorEnabled = enabled;
            _fillInWallTiles.gameObject.SetActive(!enabled);
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
    }
}