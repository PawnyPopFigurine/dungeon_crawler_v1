using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.Gameplay
{
    public class EndPortalCollision : MonoBehaviour
    {
        [SerializeField] EndPortal _parentPortal;

        public void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.tag == "PlayerTrigger")
            {
                _parentPortal.OnPlayerTouchPortal();
            }
        }
    }
}