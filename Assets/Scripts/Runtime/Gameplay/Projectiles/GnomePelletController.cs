using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.Gameplay
{
    public class GnomePelletController : ProjectileController
    {
		private void OnTriggerEnter2D(Collider2D collider)
		{
			switch(collider.gameObject.tag)
			{
				case "DestroyPlayerProjectiles":
				case "CollideAllProjectiles":
					ProjectileSystem.Instance.ClearProjectile(this);
					break;
				case "DestroyAndDestroyedByPlayerProj":
					DestructibleObject destructibleObject = collider.gameObject.GetComponent<DestructibleObject>();
					if(null != destructibleObject)
					{
						destructibleObject.OnHitByPlayerProjectile();
					}
					ProjectileSystem.Instance.ClearProjectile(this);
					break;
			}
		}
	}
}