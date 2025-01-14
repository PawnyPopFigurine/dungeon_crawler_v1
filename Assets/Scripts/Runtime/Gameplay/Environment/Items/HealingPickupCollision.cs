using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.Gameplay
{
    public class HealingPickupCollision : MonoBehaviour
    {
		[SerializeField] int _healingAmount = 1;
		public int HealingAmount => _healingAmount;

		[SerializeField] bool _maxHeal;
		public bool MaxHeal => _maxHeal;

		public void OnTriggerEnter2D(Collider2D collision)
		{
			if(collision.tag == "PlayerTrigger")
			{
				if(PlayerSystem.Instance.AtMaxHealth)
				{
					return;
				}

				if(_maxHeal)
				{
					PlayerSystem.Instance.MaxHealPlayer();
				}
				else
				{
					PlayerSystem.Instance.HealPlayerByAmount(_healingAmount);
				}
				
				gameObject.SetActive(false);	//TODO: replace with pooling clear item
			}
		}
	}
}