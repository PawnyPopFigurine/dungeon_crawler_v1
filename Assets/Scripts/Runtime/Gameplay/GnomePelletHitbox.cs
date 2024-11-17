using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.Gameplay
{
    public class GnomePelletHitbox : MonoBehaviour
    {
        [SerializeField] GnomePelletController _controller;
		public void OnCollisionEnter(Collision collision)
		{
			_controller.CollisionEnter(collision);
		}

		public void OnTriggerEnter(Collider other)
		{
			_controller.TriggerEnter(other);
		}
	}
}