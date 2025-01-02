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

        [SerializeField] Tilemap _requiredFloorTiles;
        public Tilemap RequiredFloorTiles => _requiredFloorTiles;

        [SerializeField] PatrolBehaviour _patrolBehaviour;
        bool _hasPatrolBehaviour;

        [SerializeField] EnemyProjectileBehaviour _projectileBehaviour;

        bool _isAlive;
        public bool IsAlive => _isAlive;

        public delegate void EnemyEvent();
        public event EnemyEvent OnEnemyKilled;

        bool _initialised;

        bool _playerInRoom;
        public bool PlayerInRoom => _playerInRoom;

        [SerializeField] SpriteRenderer _visuals;
        [SerializeField] SpriteRenderer _spawnPointVisuals;

        EOrthogonalDirection _currentFacing;
        public EOrthogonalDirection CurrentFacing => _currentFacing;

        public void Initialise()
        {
            if(_initialised)
            {
                return;
            }

            _initialised = true;

            _occupyTiles.gameObject.GetComponent<TilemapRenderer>().enabled = false;
            _requiredFloorTiles.gameObject.GetComponent<TilemapRenderer>().enabled = false;
            _hasPatrolBehaviour = null != _patrolBehaviour;

            SetCallbacks();
        }

        public void SetCallbacks()
        {
            _destructibleComponent.OnObjectDestroyed -= OnDestroyed;
            _destructibleComponent.OnObjectDestroyed += OnDestroyed;
        }

        public void UpdateController(float deltaTime)
        {
            if (_playerInRoom && _isAlive)
            {
                if(null != _patrolBehaviour)
				{
                    _patrolBehaviour.UpdateBehaviour(deltaTime);
                }
                
                if(null != _projectileBehaviour)
				{
                    _projectileBehaviour.UpdateProjectileBehaviour(deltaTime);
				}
            }
        }

        public void OnLevelPlacement()
        {
            _isAlive = true;

            _visuals.enabled = false;
            _spawnPointVisuals.enabled = true;

            if (null != _patrolBehaviour)
            {
                _patrolBehaviour.OnLevelPlacement();
            }
        }

        public void OnDestroyed()
        {
            _isAlive = false;
            OnEnemyKilled?.Invoke();
        }

        public void OnRoomEntered()
        {
            _playerInRoom = true;

            _spawnPointVisuals.enabled = false;
            _visuals.enabled = true;
        }

        public void ResetController()
        {
            _destructibleComponent.ResetObject();

            _playerInRoom = false;
            _isAlive = false;
        }

        public void SetCurrentFacing(EOrthogonalDirection facing)
		{
            _currentFacing = facing;
		}
    }
}