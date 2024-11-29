using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.Gameplay
{
    public class RoomPlayerDetectionField : MonoBehaviour
    {
        [SerializeField] RoomController _controller;

        public void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.tag == "PlayerTrigger")
            {
                _controller.OnPlayerEnterRoom();
            }
        }
    }
}