using UnityEngine;

namespace Geometry
{
    [System.Serializable]
    public class Vertex
    {
        #region Fields and Properties
        [SerializeField] private int m_index = 0; 
        [SerializeField] private Vector2 m_position = Vector2.zero;
        public bool IsConvex { get; set;  }


        public Vector2 Position { get { return m_position; } }
        public int Index { get { return m_index; } }
        #endregion

        #region Constructor
        public Vertex(int _index, Vector2 _position, bool _isConvex)
        {
            m_index = _index;
            m_position = _position;
            IsConvex = _isConvex; 
        }
        public Vertex(int _index, Vector2 _position)
        {
            m_index = _index;
            m_position = _position;
        }
        #endregion

        #region Methods
        #endregion
    }
}

