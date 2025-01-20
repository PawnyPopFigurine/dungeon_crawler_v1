using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace JZK.Gameplay
{
    public class EnemyProjectileController : ProjectileController
    {
		[SerializeField] bool _immuneToWallCollision;
		private void OnTriggerEnter2D(Collider2D collider)
		{
			switch (collider.gameObject.tag)
			{
				case "CollideAllProjectiles":
					if(!_immuneToWallCollision)
					{
						ProjectileSystem.Instance.ClearProjectile(this);
					}
					break;
				case "PlayerTrigger":
					PlayerSystem.Instance.OnPlayerHitHazard(this.gameObject);
					ProjectileSystem.Instance.ClearProjectile(this);
					break;
			}
		}
	}
}