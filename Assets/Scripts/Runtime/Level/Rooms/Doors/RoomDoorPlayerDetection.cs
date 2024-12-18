using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.Gameplay
{
	public class RoomDoorPlayerDetection : MonoBehaviour
	{
		[SerializeField] RoomDoor _parentDoor;

		public void OnTriggerEnter2D(Collider2D collision)
		{
			if (collision.gameObject.tag == "PlayerTrigger")
			{
				_parentDoor.OnDoorEntered();
			}
		}
	}
}