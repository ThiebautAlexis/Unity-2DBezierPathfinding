using UnityEngine;

namespace Geometry
{
    [System.Serializable]
    public class Vertex
    {
        [SerializeField] private int m_index = 0; 
        [SerializeField] private Vector2 m_position = Vector2.zero;

        public Vector2 Position { get { return m_position; } }
    }
}

