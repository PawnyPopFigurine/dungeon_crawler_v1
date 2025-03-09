using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.Gameplay
{
    public class KeyPickupCollision : MonoBehaviour
    {
        [SerializeField] ItemController _parentController;

        public void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.tag == "PlayerTrigger")
            {
                //trigger some key logic here
                GameplaySystem.Instance.OnKeyCollected(_parentController.ItemIndex);
                _parentController.OnCollected();
            }
        }
    }
}