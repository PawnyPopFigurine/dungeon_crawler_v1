using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.Gameplay
{
    public class GnomePelletController : ProjectileController
    {
		public void CollisionEnter(Collision collision)
		{
			if(collision.gameObject.tag == "DestroyPlayerProjectiles")
			{
				ProjectileSystem.Instance.ClearProjectile(this);
			}
		}

		public void TriggerEnter(Collider other)
		{
			if(other.gameObject.tag == "DestroyPlayerProjectiles")
			{
				ProjectileSystem.Instance.ClearProjectile(this);
			}
		}
	}
}