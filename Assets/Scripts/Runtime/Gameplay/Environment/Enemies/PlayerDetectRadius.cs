using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.Gameplay
{
    public class PlayerDetectRadius : MonoBehaviour
    {
		[SerializeField] EnemyController _parentController;

		public void OnTriggerEnter2D(Collider2D collision)
		{
			if(collision.gameObject.tag == "PlayerTrigger")
			{
				_parentController.OnPlayerDetected();
			}
		}

		public void OnTriggerExit2D(Collider2D collision)
		{
			if (collision.gameObject.tag == "PlayerTrigger")
			{
				_parentController.OnPlayerDetected();
			}
		}
	}
}