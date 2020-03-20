using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Geometry;

namespace Pathfinding2D
{
    public class PF2D_NavigationMesh : MonoBehaviour
    {
        #region Fields and properties
        [SerializeField] private PF2D_PolygoneVertices m_meshHull;
        [SerializeField] private int[] m_triangles = new int[] { };
        [SerializeField] private PF2D_PolygoneVertices[] m_holes = new PF2D_PolygoneVertices[] { };
        #endregion

        #region Methods

        #region Original Methods

        #endregion

        #region UnityMethods
        public void TriangulateMesh()
        {
            Vector2[][] _holes = new Vector2[m_holes.Length][];
            for (int i = 0; i < m_holes.Length; i++)
            {
                _holes[i] = m_holes[i].Vertices; 
            }
            Polygon _selfPolygon = new Polygon(m_meshHull.Vertices, _holes);
            m_triangles = Triangulator.Triangulate(_selfPolygon);
        }
        #endregion 

        #endregion
    }

    [System.Serializable]
    public class PF2D_PolygoneVertices
    {
        [SerializeField] private Vector2[] m_vertices = new Vector2[] { };
        public Vector2[] Vertices { get { return m_vertices; } }

        [SerializeField] private bool m_displayPoints = false;
        [SerializeField] private bool m_isSelected = false;

    }
}

