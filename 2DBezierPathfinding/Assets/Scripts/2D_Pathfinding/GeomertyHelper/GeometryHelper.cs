using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Geometry
{

    public static class GeometryHelper
    {
        /* GeometryHelper :
         *
         *	#####################
         *	###### PURPOSE ######
         *	#####################
         *
         *	Contains methods that use geomtery
         *
         *	#####################
         *	### MODIFICATIONS ###
         *	#####################
         *
         *	Date :			[06/02/2019]
         *	Author :		[THIEBAUT Alexis]
         *
         *	Changes : 
         *
         *	Initialisation of the class
         *	    - Migrating all geometry methods from other scripts
         *
         *	-----------------------------------
         *	
         * Date :			[29/04/2019]
         *	Author :		[THIEBAUT Alexis]
         *
         *	Changes : 
         *
         *	Modification on the method Is Intersecting, now can return the intersection point
         *
         *	-----------------------------------
        */

        #region Methods

        #region bool 
        /// <summary>
        /// Check if two segement intersect
        /// </summary>
        /// <param name="L1_start">start of the first segment</param>
        /// <param name="L1_end">end of the first segment</param>
        /// <param name="L2_start">start of the second segment</param>
        /// <param name="L2_end">end of the second segment</param>
        /// <returns>return true if segements intersect</returns>
        public static bool IsIntersecting(Vector3 _a, Vector3 _b, Vector3 _c, Vector3 _d)
        {

            Vector3 _ab = _b - _a; // I
            float _anglesign = AngleSign(_a, _b, _c);
            Vector3 _cd = _anglesign < 0 ? _d - _c : _c - _d; // J 

            Vector3 _pointLeft = _anglesign < 0 ? _c : _d;

            float _denominator = Vector3.Cross(_ab, _cd).magnitude;

            if (_denominator != 0)
            {
                //  m =    (     -Ix*A.y      +      Ix*Cy     +      Iy*Ax     -      Iy*Cx )
                float _m = ((-_ab.x * _a.z) + (_ab.x * _pointLeft.z) + (_ab.z * _a.x) - (_ab.z * _pointLeft.x)) / _denominator;

                //  k =    (     Jy*Ax     -      Jy*Cx     -      Jx*Ay     +      Jx*Cy )
                float _k = ((_cd.z * _a.x) - (_cd.z * _pointLeft.x) - (_cd.x * _a.z) + (_cd.x * _pointLeft.z)) / _denominator;


                if ((_m >= 0 && _m <= 1 && _k >= 0 && _k <= 1))
                {

                    if (Vector3.Distance((_a + _k * _ab), (_pointLeft + _m * _cd)) > .1f)
                    {
                        return false;
                    }

                    return true;
                }
            }
            return false;
        }

        public static bool IsIntersecting(Vector3 _a, Vector3 _b, Vector3 _c, Vector3 _d, out Vector3 _intersection)
        {
            Vector3 _ab = _b - _a; // I
            float _anglesign = AngleSign(_a, _b, _c);
            Vector3 _cd = _anglesign < 0 ? _d - _c : _c - _d; // J

            Vector3 _pointLeft = _anglesign < 0 ? _c : _d;

            // P -> Intersection point 
            // P = _a + k * _ab = _c + m * _cd
            // A.x + k*_ab.x = _c.x + m *_cd.x
            // A.y + k*_ab.y = _c.y + m *_cd.y
            // A.z + k*_ab.z = _c.z + m *_cd.z

            float _denominator = Vector3.Cross(_ab, _cd).magnitude;

            if (_denominator != 0)
            {
                //  m =    (     -Ix*A.y      +      Ix*Cy     +      Iy*Ax     -      Iy*Cx )
                float _m = ((-_ab.x * _a.z) + (_ab.x * _pointLeft.z) + (_ab.z * _a.x) - (_ab.z * _pointLeft.x)) / _denominator;

                //  k =    (     Jy*Ax     -      Jy*Cx     -      Jx*Ay     +      Jx*Cy )
                float _k = ((_cd.z * _a.x) - (_cd.z * _pointLeft.x) - (_cd.x * _a.z) + (_cd.x * _pointLeft.z)) / _denominator;

                //Debug.Log(_m + " " + _k); 

                if ((_m >= 0 && _m <= 1 && _k >= 0 && _k <= 1))
                {

                    if (Vector3.Distance((_a + _k * _ab), (_pointLeft + _m * _cd)) > .1f)
                    {
                        _intersection = _a;
                        return false;
                    }

                    _intersection = _a + _k * _ab;
                    return true;
                }
            }
            _intersection = _a;
            return false;
        }

        /// <summary>
        /// Return if the position is inside of the triangle
        /// </summary>
        /// <param name="_position"></param>
        /// <returns></returns>
        public static bool IsInTriangle(Vector3 _position, Triangle _triangle)
        {
            Barycentric _barycentric = new Barycentric(_triangle.Vertices[0], _triangle.Vertices[1], _triangle.Vertices[2], _position);
            return _barycentric.IsInside;
        }

        /// <summary>
        /// Return if the _position is in the triangle composed by the 3 vertices
        /// </summary>
        /// <param name="_position"></param>
        /// <param name="_vertex1"></param>
        /// <param name="_vertex2"></param>
        /// <param name="_vertex3"></param>
        /// <returns></returns>
        public static bool IsInTriangle(Vector3 _position, Vector3 _vertex1, Vector3 _vertex2, Vector3 _vertex3)
        {
            Barycentric _barycentric = new Barycentric(_vertex1, _vertex2, _vertex3, _position);
            return _barycentric.IsInside; 
        }

        public static bool IsInAnyTriangle(Vector3 _position, List<Triangle> _triangles)
        {
            for (int i = 0; i < _triangles.Count; i++)
            {
                if (IsInTriangle(_position, _triangles[i]))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if a point is between two endpoints of a segment
        /// </summary>
        /// <param name="_firstSegmentPoint">First endpoint of the segment</param>
        /// <param name="_secondSegmentPoint">Second endpoint of the segment</param>
        /// <param name="_comparedPoint">Compared point</param>
        /// <returns></returns>
        public static bool PointContainedInSegment(Vector3 _firstSegmentPoint, Vector3 _secondSegmentPoint, Vector3 _comparedPoint)
        {
            float _segmentLength = Vector3.Distance(_firstSegmentPoint, _secondSegmentPoint);
            float _a = Vector3.Distance(_firstSegmentPoint, _comparedPoint);
            float _b = Vector3.Distance(_secondSegmentPoint, _comparedPoint);
            return _segmentLength > _a && _segmentLength > _b;
        }
        #endregion

        #region int
        /// <summary>
        /// Return the sign of the angle _a _b _c
        /// </summary>
        /// <param name="_start">Start point</param>
        /// <param name="_end">End Point</param>
        /// <param name="_point">Point</param>
        /// <param name="debug"></param>
        /// <returns>Sign of the angle between the three points (if 0 or 180 or -180, return 0)</returns>
        public static int AngleSign(Vector3 _a, Vector3 _b, Vector3 _c, bool debug = false)
        {
            return (int)Mathf.Sign((_c.x - _a.x) * (-_b.y + _a.y) + (_c.y - _a.y) * (_b.x - _a.x));
            /* LEGACY
            Vector3 _ref = _end - _start;
            Vector3 _angle = _point - _start;
            if (debug) Debug.Log($"{_start} --> {_end} // {_point} = {Vector3.SignedAngle(_ref, _angle, Vector3.up)}");
            float _alpha = Vector3.SignedAngle(_ref, _angle, Vector3.up);
            if (_alpha == 0 || _alpha == 180 || _alpha == -180) return 0;
            if (_alpha > 0) return 1;
            return -1;
            */
        }
        #endregion

        #region Triangle
        /// <summary>
        /// Get the triangle where the position is contained 
        /// If triangle can't be found, get the closest triangle
        /// </summary>
        /// <param name="_position">Position</param>
        /// <returns>Triangle where the position is contained</returns>
        public static Triangle GetTriangleContainingPosition(Vector3 _position, List<Triangle> triangles)
        {
            foreach (Triangle triangle in triangles)
            {
                if (IsInTriangle(_position, triangle))
                {
                    return triangle;
                }
            }
            return triangles.OrderBy(t => Vector3.Distance(t.CenterPosition, _position)).FirstOrDefault();
        }
        #endregion

        #region Vector3
        /// <summary>
        /// Get the transposed point of the predicted position on a segement between the previous and the next position
        /// Check if the targeted point is on the segment between the previous and the next points
        /// If it doesn't the normal point become the _nextPosition
        /// </summary>
        /// <param name="_predictedPosition">Predicted Position</param>
        /// <param name="_previousPosition">Previous Position</param>
        /// <param name="_nextPosition">Next Position</param>
        /// <returns></returns>
        public static Vector3 GetNormalPoint(Vector3 _predictedPosition, Vector3 _previousPosition, Vector3 _nextPosition)
        {
            Vector3 _ap = _predictedPosition - _previousPosition;
            Vector3 _ab = (_nextPosition - _previousPosition).normalized;
            Vector3 _ah = _ab * (Vector3.Dot(_ap, _ab));
            Vector3 _normal = (_previousPosition + _ah);
            Vector3 _min = Vector3.Min(_previousPosition, _nextPosition);
            Vector3 _max = Vector3.Max(_previousPosition, _nextPosition);
            if (_normal.x < _min.x || _normal.y < _min.y || _normal.x > _max.x || _normal.y > _max.y)
            {
                return _nextPosition;
            }
            return _normal;
        }

        #region Vector3
        public static Vector3 GetClosestPosition(Vector3 _position, List<Triangle> _triangles)
        {

            //Get the closest triangle
            Triangle _triangle = GetTriangleContainingPosition(_position, _triangles);
            //Get the closest point from position into the triangle 
            int _j = 0;
            Vector3 _intersectingPosition; 
            for (int i = 0; i < 2; i++)
            {
                _j = i + 1 >= _triangle.Vertices.Length ? 0 : i + 1;
                if (IsIntersecting(_triangle.Vertices[i], _triangle.Vertices[_j], _triangle.CenterPosition, _position, out _intersectingPosition))
                {
                    // Debug.Log("Return intersection -> " + _groundedPosition);
                    return _intersectingPosition;
                }
            }

            return _triangle.CenterPosition;
        }
        #endregion
        #endregion

        /// <summary>
        /// Compare triangles
        /// if the triangles have more than 1 vertices in common return true
        /// </summary>
        /// <param name="_triangle1">First triangle to compare</param>
        /// <param name="_triangle2">Second triangle to compare</param>
        /// <returns>If the triangles have more than 1 vertex.ices in common</returns>
        public static int[] GetVerticesIndexInCommon(Triangle _triangle1, Triangle _triangle2)
        {
            List<int> _vertices = new List<int>();
            for (int i = 0; i < _triangle1.VerticesIndex.Length; i++)
            {
                for (int j = 0; j < _triangle2.VerticesIndex.Length; j++)
                {
                    if (_triangle1.VerticesIndex[i] == _triangle2.VerticesIndex[j])
                        _vertices.Add(_triangle1.VerticesIndex[i]);
                }
            }
            return _vertices.ToArray();
        }

        /// <summary>
        /// Compare triangles
        /// if the triangles have more than 1 vertices in common return true
        /// </summary>
        /// <param name="_triangle1">First triangle to compare</param>
        /// <param name="_triangle2">Second triangle to compare</param>
        /// <returns>If the triangles have more than 1 vertex.ices in common</returns>
        public static Vector3[] GetVerticesPositionInCommon(Triangle _triangle1, Triangle _triangle2)
        {
            List<Vector3> _vertices = new List<Vector3>();
            for (int i = 0; i < _triangle1.VerticesIndex.Length; i++)
            {
                for (int j = 0; j < _triangle2.VerticesIndex.Length; j++)
                {
                    if (_triangle1.VerticesIndex[i] == _triangle2.VerticesIndex[j])
                        _vertices.Add(_triangle1.Vertices[i]);
                }
            }
            return _vertices.ToArray();
        }
        #endregion
    }

    public struct Barycentric
    {
        public float u;
        public float v;
        public float w;

        /// <summary>
        /// Return if u, v and w are greater or equal than 0 and less or equal than 1
        /// That means the point is inside of the triangle
        /// </summary>
        public bool IsInside
        {
            get
            {
                return (u >= 0.0f) && (u <= 1.0f) && (v >= 0.0f) && (v <= 1.0f) && (w >= 0.0f); //(w <= 1.0f)
            }
        }

        /// <summary>
        /// Calculate 3 value u, v, w as:
        /// aP = u*aV1 + v*aV2 + w*aV3
        /// if u, v and w are greater than 0, that means that the point is in the triangle aV1 aV2 aV3.
        /// </summary>
        /// <param name="aV1">First point of the triangle</param>
        /// <param name="aV2">Second point of the triangle</param>
        /// <param name="aV3">Third point of the triangle</param>
        /// <param name="aP">Point to get the Barycentric coordinates</param>
        public Barycentric(Vector3 aV1, Vector3 aV2, Vector3 aV3, Vector3 aP)
        {
            Vector3 a = aV2 - aV3;
            Vector3 b = aV1 - aV3;
            Vector3 c = aP - aV3;
            float aLen = a.x * a.x + a.y * a.y + a.z * a.z;
            float bLen = b.x * b.x + b.y * b.y + b.z * b.z;
            float ab = a.x * b.x + a.y * b.y + a.z * b.z;
            float ac = a.x * c.x + a.y * c.y + a.z * c.z;
            float bc = b.x * c.x + b.y * c.y + b.z * c.z;
            float d = aLen * bLen - ab * ab;
            u = (aLen * bc - ab * ac) / d;
            v = (bLen * ac - ab * bc) / d;
            w = 1.0f - u - v;
        }
    }
}