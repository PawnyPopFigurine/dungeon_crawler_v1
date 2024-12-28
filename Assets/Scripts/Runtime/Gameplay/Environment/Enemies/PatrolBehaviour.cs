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

        public void OnLevelPlacement()
        {
            _patrolPoints.Clear();

            foreach(Transform t in _patrolPointTransforms)
            {
                Vector3 patrolPos = t.position;
                _patrolPoints.Add(patrolPos);
            }

            _currentPatrolTarget = _patrolPoints[0];
        }

        public void Update()
        {
            // Move our position a step closer to the target.
            var step = _speed * Time.deltaTime; // calculate distance to move
            transform.position = Vector3.MoveTowards(transform.position, _currentPatrolTarget, step);

            if (Vector3.Distance(transform.position, _currentPatrolTarget) < 0.001f)
            {
                int nextPatrolIndex = _patrolPoints.IndexOf(_currentPatrolTarget) + 1;
                if(nextPatrolIndex >= _patrolPoints.Count)
                {
                    nextPatrolIndex = 0;
                }

                _currentPatrolTarget = _patrolPoints[nextPatrolIndex];
            }
        }
    }
}