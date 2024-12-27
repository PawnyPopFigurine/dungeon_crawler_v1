using JZK.Level;
using System;
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

		private EOrthogonalDirection _sideOfRoom;
		public EOrthogonalDirection SideOfRoom => _sideOfRoom;

		bool _isLinked = false;
		public bool IsLinked => _isLinked;


        public void Initialise()
		{
			float rotation = transform.eulerAngles.z;
			switch (rotation)
			{
				case 0:
					_sideOfRoom = EOrthogonalDirection.Up;
					break;
				case 90:
					_sideOfRoom = EOrthogonalDirection.Left;
					break;
				case 180:
					_sideOfRoom = EOrthogonalDirection.Down;
					break;
				case 270:
					_sideOfRoom = EOrthogonalDirection.Right;
					break;

			}
		}


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
			if (!_doorEnabled)
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
			if (null == newDestination)
			{
				_travelToDoor = null;
				_isLinked = false;
				return;
			}

			_travelToDoor = newDestination;
			_isLinked = true;
			if (updateLinkingDoor)
			{
				_travelToDoor.LinkToDoor(this, false);
			}
		}

		public void OnDoorEntered()
		{
			if (!_doorEnabled)
			{
				return;
			}

			if (null == _travelToDoor)
			{
				return;
			}

			if (!_travelToDoor.enabled)
			{
				return;
			}

			PlayerSystem.Instance.SetPlayerPos(_travelToDoor.SetPlayerPos.position);
		}

		public void SetDoorSprites(Sprite openSprite, Sprite shutSprite)
		{
			_openSprite = openSprite;
			_shutSprite = shutSprite;
		}
	}
}