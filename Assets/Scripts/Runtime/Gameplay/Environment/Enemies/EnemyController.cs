using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace JZK.Gameplay
{
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] string _definitionId;
        public string DefinitionId => _definitionId;

        [SerializeField] DestructibleObject _destructibleComponent; //when this is destroyed, the enemy is considered dead

        [SerializeField] Tilemap _occupyTiles;
        public Tilemap OccupyTiles => _occupyTiles;

        bool _isAlive;
        public bool IsAlive => _isAlive;

        public delegate void EnemyEvent();
        public event EnemyEvent OnEnemyKilled;

        public void Start()
        {
            _destructibleComponent.OnObjectDestroyed -= OnDestroyed;
            _destructibleComponent.OnObjectDestroyed += OnDestroyed;

            _isAlive = true;

            _occupyTiles.gameObject.GetComponent<TilemapRenderer>().enabled = false;
        }

        public void OnDestroyed()
        {
            _isAlive = false;
            OnEnemyKilled?.Invoke();
        }

        public void OnRoomEntered()
        {
            //do stuff here
        }

        public void ResetController()
        {
            _destructibleComponent.ResetObject();
        }
    }
}