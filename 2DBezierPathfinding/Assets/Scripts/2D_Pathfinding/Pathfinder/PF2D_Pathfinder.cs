using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Geometry; 

namespace Pathfinding2D
{
    public static class PF2D_Pathfinder
    {
        public static bool CalculatePath(Vector2 _origin, Vector2 _destination, out Vector3[] _path, List<Triangle> _trianglesDatas)
        {
            _path = new Vector3[0];
            // GET TRIANGLES
            // Get the origin triangle and the destination triangle
            Triangle _originTriangle = GeometryHelper.GetTriangleContainingPosition(_origin, _trianglesDatas);
            Triangle _targetedTriangle = GeometryHelper.GetTriangleContainingPosition(_destination, _trianglesDatas);

            //Open list that contains all heuristically calculated triangles 
            List<Triangle> _openList = new List<Triangle>();
            //returned path
            Dictionary<Triangle, Triangle> _cameFrom = new Dictionary<Triangle, Triangle>();

            Triangle _currentTriangle = null;
            List<Triangle> _linkedTriangles; 

            /* ASTAR: Algorithm*/
            // Add the origin point to the open and close List
            // Set its heuristic cost and its selection state
            _openList.Add(_originTriangle);
            //_originTriangle.HeuristicCostFromStart = 0;
            _cameFrom.Add(_originTriangle, _originTriangle);
            //float _cost = 0;
            while (_openList.Count > 0)
            {
                //Get the point with the best heuristic cost
                _currentTriangle = GetBestTriangle(ref _openList);
                //If this point is in the targeted triangle, 
                if (_currentTriangle == _targetedTriangle)
                {
                    //_cost = _currentTriangle.HeuristicCostFromStart + HeuristicCost(_currentTriangle, _targetedTriangle);
                    //_targetedTriangle.HeuristicCostFromStart = _cost;

                    //add the destination point to the close list and set the previous point to the current point or to the parent of the current point if it is in Line of sight 

                    //_cameFrom.Add(_targetedTriangle, _currentTriangle);

                    //Build the path
                    _path = BuildPath(_cameFrom, _origin, _destination);
                    return true;
                }

                //Get all linked points from the current point
                _linkedTriangles = GeometryHelper.GetNeighbourTriangles(_currentTriangle, _trianglesDatas); 
                for (int i = 0; i < _linkedTriangles.Count; i++)
                {
                    Triangle _linkedTriangle = _linkedTriangles[i];
                    // If the linked points is not selected yet
                    if (!_cameFrom.ContainsKey(_linkedTriangle))
                    {
                        // Calculate the heuristic cost from start of the linked point
                        //_cost = _currentTriangle.HeuristicCostFromStart + HeuristicCost(_currentTriangle, _linkedTriangle);
                        //_linkedTriangle.HeuristicCostFromStart = _cost;
                        if (!_openList.Contains(_linkedTriangle) /*|| _cost < _linkedTriangle.HeuristicCostFromStart*/)
                        {
                            // Set the heuristic cost from start for the linked point
                            //_linkedTriangle.HeuristicCostFromStart = _cost;
                            //Its heuristic cost is equal to its cost from start plus the heuristic cost between the point and the destination
                            //_linkedTriangle.HeuristicPriority = HeuristicCost(_linkedTriangle, _targetedTriangle) + _cost;
                            //Set the point selected and add it to the open and closed list
                            _openList.Add(_linkedTriangle);
                            _cameFrom.Add(_linkedTriangle, _currentTriangle);
                        }
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// Get the triangle with the best heuristic cost from a list 
        /// Remove this point from the list and return it
        /// </summary>
        /// <param name="_triangles">list where the points are</param>
        /// <returns>point with the best heuristic cost</returns>
        static Triangle GetBestTriangle(ref List<Triangle> _triangles)
        {
            Triangle _bestNavTriangle = _triangles[0];
            _triangles.RemoveAt(0);
            return _bestNavTriangle;
            /*
            int _bestIndex = 0;
            for (int i = 0; i < _triangles.Count; i++)
            {
                if (_triangles[i].HeuristicPriority < _triangles[_bestIndex].HeuristicPriority)
                {
                    _bestIndex = i;
                }
            }

            Triangle _bestNavTriangle = _triangles[_bestIndex];
            _triangles.RemoveAt(_bestIndex);
            return _bestNavTriangle;*/
        }

        static Vector3[] BuildPath(Dictionary<Triangle, Triangle> _pathToBuild, Vector3 _origin, Vector3 _destination)
        {
            if (_pathToBuild.Count == 1)
            {
                List<Vector3> _pathPoints = new List<Vector3>();
                _pathPoints.Add(_origin);
                _pathPoints.Add(_destination);
                return _pathPoints.ToArray();
            }
            #region BuildingAbsolutePath
            // Building absolute path -> Link all triangle's CenterPosition together
            // Adding _origin and destination to the path
            Triangle _currentTriangle = _pathToBuild.Last().Key;
            List<Triangle> _absoluteTrianglePath = new List<Triangle>();
            while (_currentTriangle != _pathToBuild.First().Key)
            {
                _absoluteTrianglePath.Add(_currentTriangle);
                _currentTriangle = _pathToBuild[_currentTriangle];
            }
            _absoluteTrianglePath.Add(_currentTriangle);
            //Reverse the path to start at the origin 
            _absoluteTrianglePath.Reverse();

            //return _absoluteTrianglePath.Select(t => t.CenterPosition).ToArray(); 
            #endregion

            //Create the simplifiedPath
            List<Vector3> _simplifiedPath = new List<Vector3>() { _origin };

            //If there is only the origin and the destination, the path doesn't have to be simplified
            if (_absoluteTrianglePath.Count <= 1)
            {
                _simplifiedPath.Add(_destination);
                return _simplifiedPath.ToArray();
            }
            //Simplify the path with Funnel Algorithm

            //Create both portals vertices arrays
            Vector3[] _leftVertices = new Vector3[_absoluteTrianglePath.Count - 1];
            Vector3[] _rightVertices = new Vector3[_absoluteTrianglePath.Count - 1];

            //Create the apex
            Vector3 _apex = _origin;

            //Initialize portal vertices
            Vector3 _startLinePoint = Vector3.zero;
            Vector3 _endLinePoint = Vector3.zero;
            Vector3 _vertex1 = Vector3.zero;
            Vector3 _vertex2 = Vector3.zero;

            _currentTriangle = null;
            #region Initialise Portal Vertices
            /*
            //Initialize portal vertices between each triangles
            for (int i = 1; i < _absoluteTrianglePath.Count - 1; i++)
            {
                _currentTriangle = _absoluteTrianglePath[i];
                _startLinePoint = _currentTriangle.CenterPosition;
                _endLinePoint = _absoluteTrianglePath[i + 1].CenterPosition;
                for (int j = 0; j < _currentTriangle.Vertices.Length; j++)
                {
                    int k = j + 1 >= _currentTriangle.Vertices.Length ? 0 : j + 1;
                    _vertex1 = _currentTriangle.Vertices[j];
                    _vertex2 = _currentTriangle.Vertices[k]; ;
                    if (GeometryHelper.IsIntersecting(_startLinePoint, _endLinePoint, _vertex1, _vertex2))
                    {
                        //Debug.Log(_startLinePoint + "///" + _endLinePoint + " intersect with " + _vertex1 + "///" + _vertex2); 
                        if (GeometryHelper.AngleSign(_startLinePoint, _endLinePoint, _vertex1) > 0)
                        {
                            _leftVertices[i] = _vertex2;
                            _rightVertices[i] = _vertex1;
                        }
                        else
                        {
                            _leftVertices[i] = _vertex1;
                            _rightVertices[i] = _vertex2;
                        }
                        break;

                    }
                }
            }
            */
            //Initialize start portal vertices
            _startLinePoint = _origin;
            _startLinePoint.y = _absoluteTrianglePath[1].CenterPosition.y;
            _endLinePoint = _absoluteTrianglePath[1].CenterPosition;
            _currentTriangle = _absoluteTrianglePath[0];

            for (int j = 0; j < _currentTriangle.Vertices.Length; j++)
            {
                int k = j + 1 >= _currentTriangle.Vertices.Length ? 0 : j + 1;
                _vertex1 = _currentTriangle.Vertices[j];
                _vertex2 = _currentTriangle.Vertices[k]; ;
                if (GeometryHelper.IsIntersecting(_startLinePoint, _endLinePoint, _vertex1, _vertex2))
                {
                    if (GeometryHelper.AngleSign(_startLinePoint, _endLinePoint, _vertex1) > 0)
                    {
                        _leftVertices[0] = _vertex2;
                        _rightVertices[0] = _vertex1;
                    }
                    else
                    {
                        _leftVertices[0] = _vertex1;
                        _rightVertices[0] = _vertex2;
                    }
                    break;
                }
            }
            // Initialise end portal vertices -> Close the funnel

            _leftVertices[_leftVertices.Length - 1] = _destination;
            _rightVertices[_rightVertices.Length - 1] = _destination;

            return null; 
            #endregion

            //Step through the channel
            Vector3 _currentLeftVertex;
            Vector3 _nextLeftVertex;
            Vector3 _currentRightVertex;
            Vector3 _nextRightVertex;

            //Set left and right indexes
            int _leftIndex = 0;
            int _rightIndex = 0;


            for (int i = 1; i < _absoluteTrianglePath.Count - 1; i++)
            {
                _currentLeftVertex = _leftVertices[_leftIndex];
                _nextLeftVertex = _leftVertices[i];

                _currentRightVertex = _rightVertices[_rightIndex];
                _nextRightVertex = _rightVertices[i];

                //If the new left vertex is different process
                if (_nextLeftVertex != _currentLeftVertex && i > _leftIndex)
                {
                    //If the next point does not widden funnel, update 
                    if (GeometryHelper.AngleSign(_apex, _currentLeftVertex, _nextLeftVertex) >= 0)
                    {
                        //if next side cross the other side, place new apex
                        if (GeometryHelper.AngleSign(_apex, _currentRightVertex, _nextLeftVertex) > 0)
                        {
                            // Set the new Apex
                            _apex = _currentRightVertex;
                            _simplifiedPath.Add(_apex);

                            //Set i to the apex index to be at the good index on the next loop 
                            i = _rightIndex;
                            // Find new right vertex.
                            for (int j = _rightIndex; j < _rightVertices.Length; j++)
                            {
                                if (_rightVertices[j] != _apex)
                                {
                                    _rightIndex = j;
                                    break;
                                }
                            }
                            _leftIndex = i;
                            i--;
                            continue;

                        }
                        // else skip to the next vertex
                        else
                        {
                            _leftIndex = i;
                        }
                    }
                    //else skip
                }
                //else skip


                // If the right vertex is different process
                if (_nextRightVertex != _currentRightVertex && i > _rightIndex)
                {
                    //If the next point does not widden funnel, update 
                    if (GeometryHelper.AngleSign(_apex, _currentRightVertex, _nextRightVertex) <= 0)
                    {
                        //if next side cross the other side, place new apex
                        if (GeometryHelper.AngleSign(_apex, _currentLeftVertex, _nextRightVertex) < 0)
                        {
                            //Set the new Apex
                            _apex = _currentLeftVertex;
                            _simplifiedPath.Add(_apex);

                            //Set i to the apex index to be at the good index on the next loop 
                            i = _leftIndex;
                            // Find next Left Index
                            for (int j = _leftIndex; j < _leftVertices.Length; j++)
                            {
                                if (_leftVertices[j] != _apex)
                                {
                                    _leftIndex = j;
                                    break;
                                }
                            }
                            _rightIndex = i;
                            i--;
                            continue;


                        }
                        //else skip to the next vertex
                        else
                        {
                            _rightIndex = i;
                        }
                    }
                    //else skip
                }
                //else skip

            }

            _simplifiedPath.Add(_destination);

            //Set the simplifiedPath
            return _simplifiedPath.ToArray();
        }
    }
}

