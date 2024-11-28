using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace JZK.Gameplay
{
    public class RoomDoor : MonoBehaviour
    {

        [SerializeField] Tilemap _fillInWallTiles;

        bool _isActive;

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
            _isActive = active;
            _fillInWallTiles.gameObject.SetActive(!active);
        }
    }
}