using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry
{
    public class Polygon
    {
        #region Fields and Properties
        public readonly Vector2[] Points;
        public readonly int NumPoints;

        public readonly int NumHullPoints;

        public readonly int[] NumPointsPerHole;
        public readonly int NumHoles;

        readonly int[] m_holeStartIndices;
        #endregion

        #region Constructors
        public Polygon(Vector2[] hull, Vector2[][] holes)
        {
            NumHullPoints = hull.Length;
            NumHoles = holes.GetLength(0);

            NumPointsPerHole = new int[NumHoles];
            m_holeStartIndices = new int[NumHoles];
            int numHolePointsSum = 0;

            for (int i = 0; i < holes.GetLength(0); i++)
            {
                NumPointsPerHole[i] = holes[i].Length;

                m_holeStartIndices[i] = NumHullPoints + numHolePointsSum;
                numHolePointsSum += NumPointsPerHole[i];
            }

            NumPoints = NumHullPoints + numHolePointsSum;
            Points = new Vector2[NumPoints];


            // add hull points, ensuring they wind in counterclockwise order
            bool reverseHullPointsOrder = !PointsAreCounterClockwise(hull);
            for (int i = 0; i < NumHullPoints; i++)
            {
                Points[i] = hull[(reverseHullPointsOrder) ? NumHullPoints - 1 - i : i];
            }

            // add hole points, ensuring they wind in clockwise order
            for (int i = 0; i < NumHoles; i++)
            {
                bool reverseHolePointsOrder = PointsAreCounterClockwise(holes[i]);
                for (int j = 0; j < holes[i].Length; j++)
                {
                    Points[IndexOfPointInHole(j, i)] = holes[i][(reverseHolePointsOrder) ? holes[i].Length - j - 1 : j];
                }
            }

        }

        public Polygon(Vector2[] hull) : this(hull, new Vector2[0][])
        {
        }
        #endregion

        #region Methods
        bool PointsAreCounterClockwise(Vector2[] _testPoints)
        {
            float signedArea = 0;
            for (int i = 0; i < _testPoints.Length; i++)
            {
                int nextIndex = (i + 1) % _testPoints.Length;
                signedArea += (_testPoints[nextIndex].x - _testPoints[i].x) * (_testPoints[nextIndex].y + _testPoints[i].y);
            }

            return signedArea < 0;
        }

        public int IndexOfFirstPointInHole(int _holeIndex)
        {
            return m_holeStartIndices[_holeIndex];
        }

        public int IndexOfPointInHole(int _index, int _holeIndex)
        {
            return m_holeStartIndices[_holeIndex] + _index;
        }

        public Vector2 GetHolePoint(int _index, int _holeIndex)
        {
            return Points[m_holeStartIndices[_holeIndex] + _index];
        }
        #endregion
    }

}
