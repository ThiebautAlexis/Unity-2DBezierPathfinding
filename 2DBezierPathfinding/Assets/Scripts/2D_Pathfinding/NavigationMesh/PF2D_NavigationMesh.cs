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

        [SerializeField] private Triangle[] m_navigationMeshTriangles = new Triangle[] { };
         #endregion

        #region Methods

        #region Original Methods
        public void TriangulateMesh()
        {
            Vector2[][] _holes = new Vector2[m_holes.Length][];
            for (int i = 0; i < m_holes.Length; i++)
            {
                _holes[i] = m_holes[i].Vertices;
            }
            Polygon _selfPolygon = new Polygon(m_meshHull.Vertices, _holes);
            m_triangles = Triangulator.Triangulate(_selfPolygon);
            Vertex[] _meshVertices = new Vertex[_selfPolygon.NumPoints];
            for (int i = 0; i < _meshVertices.Length; i++)
            {
                _meshVertices[i] = new Vertex(i, _selfPolygon.Points[i]);
            }
            m_navigationMeshTriangles = new Triangle[m_triangles.Length / 3];
            for (int i = 0; i < m_triangles.Length - 1; i += 3)
            {
                m_navigationMeshTriangles[i / 3] = new Triangle(_meshVertices[m_triangles[i]], _meshVertices[m_triangles[i + 1]], _meshVertices[m_triangles[i + 2]]);
            }
        }

        public Vector3[] GetPathToDestination(Vector2 _origin, Vector2 _destination)
        {
            Vector3[] _path = null;
            PF2D_Pathfinder.CalculatePath(_origin, _destination, out _path, m_navigationMeshTriangles.ToList());
            return _path;
        }
        #endregion

        #region UnityMethods
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

