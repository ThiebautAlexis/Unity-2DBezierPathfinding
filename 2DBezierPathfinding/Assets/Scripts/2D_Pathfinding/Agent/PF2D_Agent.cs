using System; 
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Geometry;

namespace Pathfinding2D
{
    public class PF2D_Agent : MonoBehaviour
    {
        #region Events
        public Action OnMovementStarted; 
        public Action OnDestinationReached;
        public Action OnAgentStopped; 
        #endregion 


        #region Fields and Properties
        [SerializeField] private PF2D_NavigationMesh m_navMesh = null;
        [SerializeField] private Transform m_destination = null;

        private Vector3[] m_path = null;

        [Header("Navigation Settings")]
        [SerializeField] private float m_radius = 1.0f;
        [SerializeField] private float m_pathRadius = 1.0f;
        [SerializeField] private float m_speed = 1.0f;
        [SerializeField] private float m_steerForce = 90.0f;

        [SerializeField] private Vector3 m_velocity = Vector3.zero;
        #endregion


        #region Methods

        #region Original Methods
        /// <summary>
        /// Make the agent follows the path
        /// </summary>
        /// <returns></returns>
        IEnumerator FollowPath()
        {
            OnMovementStarted?.Invoke();
            //isMoving = true;
            int _pathIndex = 1;

            /*STEERING*/
            //Predicted position is the position of the agent at the next frame
            Vector3 _predictedPosition;


            // Previous Position
            Vector3 _previousPosition = transform.position;
            //Next Position
            Vector3 _nextPosition = m_path[1];


            Vector3 _dir;
            Vector3 _targetPosition;
            Vector3 _normalPoint;


            float _distance = 0;
            // Angle theta is the angle between forward and velocity direction
            float _theta;
            float _scalarProduct;
            // List of directions to apply on the avoidance 
            List<Vector3> _obstaclesPos = new List<Vector3>();


            /* First the velocity is equal to the normalized direction from the agent position to the next position */
            /* LEGACY 
            if (velocity == Vector3.zero)
                velocity = (_nextPosition - OffsetPosition) * speed;
            Seek(_nextPosition);
            */

            while (Vector3.Distance(transform.position, m_path.Last()) > m_radius)
            {
                /* Apply the velocity to the transform position multiply by the speed and by Time.deltaTime to move*/
                m_velocity = m_velocity.normalized * m_speed;
                //m_velocity = Vector3.ClampMagnitude(m_velocity, m_speed);
                transform.position += m_velocity * Time.deltaTime;

                /* If the agent is close to the next position
                 * Update the previous and the next point
                 * Also update the pathIndex
                 * if the pathindex is greater than the pathcount break the loop
                 * else continue in the loop
                 */
                if (Vector3.Distance(transform.position, _nextPosition) <= m_radius)
                {
                    //set the new previous position
                    _previousPosition = m_path[_pathIndex];
                    //Increasing path index
                    _pathIndex++;
                    if (_pathIndex > m_path.Length - 1) break;
                    //Set the new next Position
                    _nextPosition = m_path[_pathIndex];
                    Seek(_nextPosition);
                    continue;
                }
                // Theta is equal to the angle between the velocity and the forward vector
                _theta = Vector3.SignedAngle(Vector3.forward, m_velocity, Vector3.up);


                /* Get the predicted Velocity and the Predicted position*/
                _predictedPosition = transform.position + m_velocity;

                /*Get the transposed Position of the predicted position on the segment between the previous and the next point
                * The agent has to get closer while it's to far away from the path 
                */
                _normalPoint = GeometryHelper.GetNormalPoint(_predictedPosition, _previousPosition, _nextPosition);


                /* Direction of the segment between the previous and the next position normalized in order to go further on the path
                 * Targeted position is the normal point + an offset defined by the direction of the segment to go a little further on the path
                 * If the target is out of the segment between the previous and the next position, the target position is the next position
                 */
                _dir = (_nextPosition - _previousPosition).normalized;
                _targetPosition = _normalPoint + _dir;

                /* Distance between the predicted position and the normal point on the segment 
                * If the distance is greater than the radius, it has to steer to get closer
                */
                _distance = Vector3.Distance(_predictedPosition, _normalPoint);
                _scalarProduct = Vector3.Dot(m_velocity.normalized, _dir.normalized);
                if (_distance > m_pathRadius / 2 || _scalarProduct == -1 || m_velocity == Vector3.zero)
                {
                    Seek(_targetPosition);
                }
                Debug.DrawRay(transform.position, m_velocity); 
                yield return null;
            }
            StopAgent();
            OnDestinationReached?.Invoke();
        }


        private void Seek(Vector3 _target)
        {
            Vector3 _desiredVelocity = (_target - transform.position).normalized * m_speed;

            Vector3 _steer = (_desiredVelocity - m_velocity) * Mathf.Tan(Mathf.Deg2Rad * m_steerForce) * Time.fixedDeltaTime;
            m_velocity += _steer;
        }

        private void StopAgent()
        {
            StopCoroutine(FollowPath());
            OnAgentStopped?.Invoke(); 
        }
        #endregion

        #region UnityMethods
        // Start is called before the first frame update
        void Start()
        {
            if(m_navMesh && m_destination)
            {
                m_path = m_navMesh.GetPathToDestination(transform.position, m_destination.transform.position);
                StartCoroutine(FollowPath()); 
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, m_radius);
            Gizmos.color = Color.blue; 
            Gizmos.DrawWireSphere(transform.position, m_pathRadius); 

            if (m_path == null) return;
            Gizmos.color = Color.magenta;
            for (int i = 0; i < m_path.Length; i++)
            {
                Gizmos.DrawSphere(m_path[i], .1f);
            }

            for (int i = 0; i < m_path.Length - 2; i++)
            {
                Gizmos.DrawLine(m_path[i], m_path[i + 1]); 
            }

        }
        #endregion

        #endregion
    }
}

