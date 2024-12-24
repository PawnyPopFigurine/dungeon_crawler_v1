using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.Gameplay
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField] DestructibleObject _destructibleComponent; //when this is destroyed, the enemy is considered dead

        bool _isAlive;
        public bool IsAlive => _isAlive;

        public delegate void EnemyEvent();
        public event EnemyEvent OnEnemyKilled;

        public void Start()
        {
            _destructibleComponent.OnObjectDestroyed -= OnDestroyed;
            _destructibleComponent.OnObjectDestroyed += OnDestroyed;

            _isAlive = true;
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
    }
}