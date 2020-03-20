using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding2D
{
    public class PF2D_Agent : MonoBehaviour
    {
        [SerializeField] private PF2D_NavigationMesh m_navMesh = null;
        [SerializeField] private Transform m_destination = null;

        private Vector3[] m_path = null; 

        // Start is called before the first frame update
        void Start()
        {
            if(m_navMesh && m_destination)
            {
                m_path = m_navMesh.GetPathToDestination(transform.position, m_destination.transform.position); 
            }
        }

        private void OnDrawGizmos()
        {
            if (m_path == null) return;
            Gizmos.color = Color.magenta;
            for (int i = 0; i < m_path.Length; i++)
            {
                Gizmos.DrawSphere(m_path[i], .1f);
            }
        }
    }
}

