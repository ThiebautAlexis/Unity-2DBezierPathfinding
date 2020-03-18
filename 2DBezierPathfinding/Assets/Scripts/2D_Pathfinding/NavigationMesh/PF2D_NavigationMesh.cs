using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Geometry;

namespace Pathfinding2D
{
    public class PF2D_NavigationMesh : MonoBehaviour
    {
        #region Fields and properties
        [SerializeField] private List<Vector2> m_vertices = new List<Vector2>();
        [SerializeField, HideInInspector] private int[] m_triangles = new int[] { }; 
        #endregion

        #region Methods

        #region Original Methods

        #endregion

        #region UnityMethods
        public void TriangulateMesh()
        {
            Polygon _selfPolygon = new Polygon(m_vertices.ToArray());
            m_triangles = Triangulator.Triangulate(_selfPolygon);
        }
        #endregion 

        #endregion
    }
}
