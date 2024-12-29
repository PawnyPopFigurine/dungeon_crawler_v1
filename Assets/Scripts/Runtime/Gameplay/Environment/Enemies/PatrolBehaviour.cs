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

        EOrthogonalDirection _currentFacing;

        public void UpdateBehaviour(float deltaTime)
        {
            var step = _speed * deltaTime; // calculate distance to move
            transform.position = Vector3.MoveTowards(transform.position, _currentPatrolTarget, step);

            if (Vector3.Distance(transform.position, _currentPatrolTarget) < 0.001f)
            {
                int nextPatrolIndex = _patrolPoints.IndexOf(_currentPatrolTarget) + 1;
                if (nextPatrolIndex >= _patrolPoints.Count)
                {
                    nextPatrolIndex = 0;
                }

                _currentPatrolTarget = _patrolPoints[nextPatrolIndex];

                UpdateFacing(_currentPatrolTarget);
            }
        }

        void UpdateFacing(Vector3 referencePos)
        {
            if(transform.position.x - referencePos.x < 0.001f)
            {
                if(transform.position.y > referencePos.y)
                {
                    _currentFacing = EOrthogonalDirection.Down;
                }
                if(transform.position.y < referencePos.y)
                {
                    _currentFacing = EOrthogonalDirection.Up;
                }
            }

            if(transform.position.y - referencePos.y < 0.001f)
            {
                if(transform.position.x > referencePos.x)
                {
                    _currentFacing = EOrthogonalDirection.Left;
                }
                if(transform.position.x < referencePos.x)
                {
                    _currentFacing = EOrthogonalDirection.Right;
                }
            }
        }

        public void OnLevelPlacement()
        {
            _patrolPoints.Clear();

            foreach(Transform t in _patrolPointTransforms)
            {
                Vector3 patrolPos = t.position;
                _patrolPoints.Add(patrolPos);
            }

            transform.position = _patrolPoints[0];
            _currentPatrolTarget = _patrolPoints[1];

            UpdateFacing(_currentPatrolTarget);
        }
    }
}