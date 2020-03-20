using System.Collections;
using UnityEngine;


namespace Geometry
{
    [System.Serializable]
    public class Triangle
    {
        #region Fields and Properties
        [SerializeField] private int[] m_verticesIndex = new int[] { };
        public int[] VerticesIndex { get { return m_verticesIndex; } }

        [SerializeField] private Vector3[] m_vertices = new Vector3[] { }; 
        public Vector3[] Vertices { get { return m_vertices; } }

        public Vector3 CenterPosition
        {
            get { return (m_vertices[0] + m_vertices[1] + m_vertices[2]) / 3;}
        }
        #endregion

        #region Constructor
        public Triangle(int _a, int _b, int _c)
        {
            m_verticesIndex = new int[3] { _a, _b, _c }; 
        }

        public Triangle(Vertex _a, Vertex _b, Vertex _c)
        {
            m_verticesIndex = new int[3] { _a.Index, _b.Index, _c.Index };
            m_vertices = new Vector3[3] { _a.Position, _b.Position, _c.Position };
        }
        #endregion

        #region Methods

        #endregion

    }
}

