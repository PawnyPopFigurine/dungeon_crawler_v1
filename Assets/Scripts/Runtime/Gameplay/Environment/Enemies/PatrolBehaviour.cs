using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace JZK.Gameplay
{
    public class PatrolBehaviour : MonoBehaviour
    {
        [SerializeField] List<Transform> _patrolPointTransforms;
        List<Vector3> _patrolPoints = new();
        Vector3 _currentPatrolTarget;

        [SerializeField] float _speed;

        [SerializeField] EnemyController _controller;

        [SerializeField] EnemyProjectileBehaviour _projectileBehaviour;

        [SerializeField] bool _chasePlayer;
        public bool ChasePlayer => _chasePlayer;

		public void ResetBehaviour()
		{

		}

		public void UpdateBehaviour(float deltaTime)
        {
            var step = _speed * deltaTime; // calculate distance to move
            if(_chasePlayer)
			{
                if(_controller.PlayerInRadius)
				{
                    Vector3 playerPos = PlayerSystem.Instance.GetPlayerPos();
                    transform.position = Vector3.MoveTowards(transform.position, playerPos, step);
                    UpdateFacing(playerPos);
                }
            }
            else
			{
                transform.position = Vector3.MoveTowards(transform.position, _currentPatrolTarget, step);

                if (Vector3.Distance(transform.position, _currentPatrolTarget) < 0.001f)
                {
                    int nextPatrolIndex = _patrolPoints.IndexOf(_currentPatrolTarget) + 1;
                    if (nextPatrolIndex >= _patrolPoints.Count)
                    {
                        nextPatrolIndex = 0;
                    }

                    if (_projectileBehaviour != null)
                    {
                        _projectileBehaviour.OnPatrolTurning();
                    }

                    _currentPatrolTarget = _patrolPoints[nextPatrolIndex];
                    UpdateFacing(_currentPatrolTarget);
                }
            }
            
        }

        void UpdateFacing(Vector3 referencePos)
        {
            EOrthogonalDirection newFacing = EOrthogonalDirection.Invalid;

            if(transform.position.x - referencePos.x < 0.001f)
            {
                if(transform.position.y > referencePos.y)
                {
                    newFacing = EOrthogonalDirection.Down;
                }
                if(transform.position.y < referencePos.y)
                {
                    newFacing = EOrthogonalDirection.Up;
                }
            }

            if(transform.position.y - referencePos.y < 0.001f)
            {
                if(transform.position.x > referencePos.x)
                {
                    newFacing = EOrthogonalDirection.Left;
                }
                if(transform.position.x < referencePos.x)
                {
                    newFacing = EOrthogonalDirection.Right;
                }
            }

            if(newFacing == EOrthogonalDirection.Invalid)
			{
                Debug.LogWarning("[ENEMY] tried to set facing to invalid - reverting to Up");
                _controller.SetCurrentFacing(EOrthogonalDirection.Up);
                return;
			}

            _controller.SetCurrentFacing(newFacing);
        }

        public void OnLevelPlacement()
        {
            _patrolPoints.Clear();

            foreach(Transform t in _patrolPointTransforms)
            {
                Vector3 patrolPos = t.position;
                _patrolPoints.Add(patrolPos);
            }

            if(_patrolPoints.Count >= 1)
			{
                transform.position = _patrolPoints[0];
                _currentPatrolTarget = _patrolPoints.Count >= 2 ? _patrolPoints[1] : _patrolPoints[0];
            }

            UpdateFacing(_currentPatrolTarget);
        }
    }
}