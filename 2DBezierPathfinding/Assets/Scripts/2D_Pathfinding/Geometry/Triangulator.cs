using System.Collections;
using System.Collections.Generic;
using System.Linq; 
using UnityEngine;

namespace Geometry
{
    public static class Triangulator
    {
        public static int[] Triangulate(Polygon _polygon)
        {
            int[] _triangles = new int[((_polygon.NumHoles * 2) + _polygon.NumPoints - 2) * 3];
            int _triIndex = 0;
            LinkedList<Vertex> _polygonVertices = GenerateVerticesList(_polygon);
            //LinkedListNode<Vertex> _v = _polygonVertices.First; 
            //for (int i = 0; i < _polygonVertices.Count; i++)
            //{
            //    Debug.Log(_v.Value.Index + " -> " + _v.Value.Position);
            //    _v = _v.Next; 
            //}
            while (_polygonVertices.Count >= 3)
            {
                LinkedListNode<Vertex> _vertexNode = _polygonVertices.First;
                bool _hasRemovedVertex = false; 
                for (int i = 0; i < _polygonVertices.Count; i++)
                {
                    LinkedListNode<Vertex> _prevVertexNode = _vertexNode.Previous == null ? _polygonVertices.Last : _vertexNode.Previous;
                    LinkedListNode<Vertex> _nextVertexNode = _vertexNode.Next == null ? _polygonVertices.First : _vertexNode.Next;
                    if (_vertexNode.Value.IsConvex)
                    {
                        if (!TriangleContainsVertex(_prevVertexNode.Value, _vertexNode.Value, _nextVertexNode.Value, _polygonVertices))
                        {
                            if (!_prevVertexNode.Value.IsConvex)
                            {
                                LinkedListNode<Vertex> _previousOfPrevious = _prevVertexNode.Previous == null ? _polygonVertices.Last : _prevVertexNode.Previous;
                                if (_previousOfPrevious != null)
                                    _prevVertexNode.Value.IsConvex = IsConvex(_previousOfPrevious.Value.Position, _prevVertexNode.Value.Position, _nextVertexNode.Value.Position);
                            }
                            if (!_nextVertexNode.Value.IsConvex)
                            {
                                LinkedListNode<Vertex> _nextOfNext = _nextVertexNode.Next == null ? _polygonVertices.First : _nextVertexNode.Next;
                                if (_nextOfNext != null)
                                    _nextVertexNode.Value.IsConvex = IsConvex(_prevVertexNode.Value.Position, _nextVertexNode.Value.Position, _nextOfNext.Value.Position);
                            }

                            _triangles[_triIndex * 3 + 2] = _prevVertexNode.Value.Index;
                            _triangles[_triIndex * 3 + 1] = _vertexNode.Value.Index;
                            _triangles[_triIndex * 3] = _nextVertexNode.Value.Index;
                            _polygonVertices.Remove(_vertexNode);
                            _hasRemovedVertex = true;
                            _triIndex++;
                            break;
                        }
                    }
                    _vertexNode = _vertexNode.Next == null ? _polygonVertices.First : _vertexNode.Next;
                }
                if(!_hasRemovedVertex)
                {
                    Debug.Log("An Error has occured please try again");
                    return null; 
                }
            }
            return _triangles; 
        }

        // check if triangle contains any verts (note, only necessary to check reflex verts).
        private static bool TriangleContainsVertex(Vertex v0, Vertex v1, Vertex v2, LinkedList<Vertex> _polygonVertices)
        {
            LinkedListNode<Vertex> vertexNode = _polygonVertices.First;
            for (int i = 0; i < _polygonVertices.Count; i++)
            {
                if (!vertexNode.Value.IsConvex) // convex verts will never be inside triangle
                {
                    Vertex vertexToCheck = vertexNode.Value;
                    if (vertexToCheck.Index != v0.Index && vertexToCheck.Index != v1.Index && vertexToCheck.Index != v2.Index) // dont check verts that make up triangle
                    {
                        if (GeometryHelper.IsInTriangle(vertexToCheck.Position, v0.Position, v1.Position, v2.Position))
                        {
                            return true;
                        }
                    }
                }
                vertexNode = vertexNode.Next;
            }

            return false;
        }

        private static LinkedList<Vertex> GenerateVerticesList(Polygon _polygon)
        {
            LinkedList<Vertex> _verticesList = new LinkedList<Vertex>();
            LinkedListNode<Vertex> _currentVertexNode = null;

            for (int i = 0; i < _polygon.NumHullPoints; i++)
            {
                int _prevPointIndex = (i - 1 + _polygon.NumHullPoints) % _polygon.NumHullPoints;
                int _nextPointIndex = (i + 1) % _polygon.NumHullPoints;

                Vertex _v = new Vertex(i, _polygon.Points[i], IsConvex(_polygon.Points[_prevPointIndex], _polygon.Points[i], _polygon.Points[_nextPointIndex]));

                if (_currentVertexNode == null)
                    _currentVertexNode = _verticesList.AddFirst(_v);
                else
                    _currentVertexNode = _verticesList.AddAfter(_currentVertexNode, _v); 
            }

            List<HoleData> _sortedHoleData = new List<HoleData>();

            for (int holeIndex = 0; holeIndex < _polygon.NumHoles; holeIndex++)
            {
                Vector2 _holeBridgePoint = new Vector2(float.MinValue, 0);
                int _holeBridgeIndex = 0;
                for (int i = 0; i < _polygon.NumPointsPerHole[holeIndex]; i++)
                {
                    if(_polygon.GetHolePoint(i, holeIndex).x > _holeBridgePoint.x)
                    {
                        _holeBridgePoint = _polygon.GetHolePoint(i, holeIndex);
                        _holeBridgeIndex = i; 
                    }
                }
                _sortedHoleData.Add(new HoleData(holeIndex, _holeBridgeIndex, _holeBridgePoint));
            }

            _sortedHoleData.Sort((x, y) => (x.bridgePoint.x > y.bridgePoint.x) ? -1 : 1);

            foreach (HoleData holeData in _sortedHoleData)
            {

                // Find first edge which intersects with rightwards ray originating at the hole bridge point.
                Vector2 rayIntersectPoint = new Vector2(float.MaxValue, holeData.bridgePoint.y);
                List<LinkedListNode<Vertex>> hullNodesPotentiallyInBridgeTriangle = new List<LinkedListNode<Vertex>>();
                LinkedListNode<Vertex> initialBridgeNodeOnHull = null;
                _currentVertexNode = _verticesList.First;
                while (_currentVertexNode != null)
                {
                    LinkedListNode<Vertex> nextNode = (_currentVertexNode.Next == null) ? _verticesList.First : _currentVertexNode.Next;
                    Vector2 p0 = _currentVertexNode.Value.Position;
                    Vector2 p1 = nextNode.Value.Position;

                    // at least one point must be to right of holeData.bridgePoint for intersection with ray to be possible
                    if (p0.x > holeData.bridgePoint.x || p1.x > holeData.bridgePoint.x)
                    {
                        // one point is above, one point is below
                        if (p0.y > holeData.bridgePoint.y != p1.y > holeData.bridgePoint.y)
                        {
                            float rayIntersectX = p1.x; // only true if line p0,p1 is vertical
                            if (!Mathf.Approximately(p0.x, p1.x))
                            {
                                float intersectY = holeData.bridgePoint.y;
                                float gradient = (p0.y - p1.y) / (p0.x - p1.x);
                                float c = p1.y - gradient * p1.x;
                                rayIntersectX = (intersectY - c) / gradient;
                            }

                            // intersection must be to right of bridge point
                            if (rayIntersectX > holeData.bridgePoint.x)
                            {
                                LinkedListNode<Vertex> potentialNewBridgeNode = (p0.x > p1.x) ? _currentVertexNode : nextNode;
                                // if two intersections occur at same x position this means is duplicate edge
                                // duplicate edges occur where a hole has been joined to the outer polygon
                                bool isDuplicateEdge = Mathf.Approximately(rayIntersectX, rayIntersectPoint.x);

                                // connect to duplicate edge (the one that leads away from the other, already connected hole, and back to the original hull) if the
                                // current hole's bridge point is higher up than the bridge point of the other hole (so that the new bridge connection doesn't intersect).
                                bool connectToThisDuplicateEdge = holeData.bridgePoint.y > potentialNewBridgeNode.Previous.Value.Position.y;

                                if (!isDuplicateEdge || connectToThisDuplicateEdge)
                                {
                                    // if this is the closest ray intersection thus far, set bridge hull node to point in line having greater x pos (since def to right of hole).
                                    if (rayIntersectX < rayIntersectPoint.x || isDuplicateEdge)
                                    {
                                        rayIntersectPoint.x = rayIntersectX;
                                        initialBridgeNodeOnHull = potentialNewBridgeNode;
                                    }
                                }
                            }
                        }
                    }

                    // Determine if current node might lie inside the triangle formed by holeBridgePoint, rayIntersection, and bridgeNodeOnHull
                    // We only need consider those which are reflex, since only these will be candidates for visibility from holeBridgePoint.
                    // A list of these nodes is kept so that in next step it is not necessary to iterate over all nodes again.
                    if (_currentVertexNode != initialBridgeNodeOnHull)
                    {
                        if (!_currentVertexNode.Value.IsConvex && p0.x > holeData.bridgePoint.x)
                        {
                            hullNodesPotentiallyInBridgeTriangle.Add(_currentVertexNode);
                        }
                    }
                    _currentVertexNode = _currentVertexNode.Next;
                }

                // Check triangle formed by hullBridgePoint, rayIntersection, and bridgeNodeOnHull.
                // If this triangle contains any points, those points compete to become new bridgeNodeOnHull
                LinkedListNode<Vertex> validBridgeNodeOnHull = initialBridgeNodeOnHull;
                foreach (LinkedListNode<Vertex> nodePotentiallyInTriangle in hullNodesPotentiallyInBridgeTriangle)
                {
                    if (nodePotentiallyInTriangle.Value.Index == initialBridgeNodeOnHull.Value.Index)
                    {
                        continue;
                    }
                    // if there is a point inside triangle, this invalidates the current bridge node on hull.
                    if (GeometryHelper.IsInTriangle(nodePotentiallyInTriangle.Value.Position, holeData.bridgePoint, rayIntersectPoint, initialBridgeNodeOnHull.Value.Position))
                    {
                        // Duplicate points occur at hole and hull bridge points.
                        bool isDuplicatePoint = validBridgeNodeOnHull.Value.Position == nodePotentiallyInTriangle.Value.Position;

                        // if multiple nodes inside triangle, we want to choose the one with smallest angle from holeBridgeNode.
                        // if is a duplicate point, then use the one occurring later in the list
                        float currentDstFromHoleBridgeY = Mathf.Abs(holeData.bridgePoint.y - validBridgeNodeOnHull.Value.Position.y);
                        float pointInTriDstFromHoleBridgeY = Mathf.Abs(holeData.bridgePoint.y - nodePotentiallyInTriangle.Value.Position.y);

                        if (pointInTriDstFromHoleBridgeY < currentDstFromHoleBridgeY || isDuplicatePoint)
                        {
                            validBridgeNodeOnHull = nodePotentiallyInTriangle;

                        }
                    }
                }

                // Insert hole points (starting at holeBridgeNode) into vertex list at validBridgeNodeOnHull
                _currentVertexNode = validBridgeNodeOnHull;
                for (int i = holeData.bridgeIndex; i <= _polygon.NumPointsPerHole[holeData.holeIndex] + holeData.bridgeIndex; i++)
                {
                    int previousIndex = _currentVertexNode.Value.Index;
                    int currentIndex = _polygon.IndexOfPointInHole(i % _polygon.NumPointsPerHole[holeData.holeIndex], holeData.holeIndex);
                    int nextIndex = _polygon.IndexOfPointInHole((i + 1) % _polygon.NumPointsPerHole[holeData.holeIndex], holeData.holeIndex);

                    if (i == _polygon.NumPointsPerHole[holeData.holeIndex] + holeData.bridgeIndex) // have come back to starting point
                    {
                        nextIndex = validBridgeNodeOnHull.Value.Index; // next point is back to the point on the hull
                    }

                    bool vertexIsConvex = IsConvex(_polygon.Points[previousIndex], _polygon.Points[currentIndex], _polygon.Points[nextIndex]);
                    Vertex holeVertex = new Vertex(currentIndex, _polygon.Points[currentIndex], vertexIsConvex);
                    _currentVertexNode = _verticesList.AddAfter(_currentVertexNode, holeVertex);
                }

                // Add duplicate hull bridge vert now that we've come all the way around. Also set its concavity
                Vector2 nextVertexPos = (_currentVertexNode.Next == null) ? _verticesList.First.Value.Position : _currentVertexNode.Next.Value.Position;
                bool isConvex = IsConvex(holeData.bridgePoint, validBridgeNodeOnHull.Value.Position, nextVertexPos);
                Vertex repeatStartHullVert = new Vertex(validBridgeNodeOnHull.Value.Index, validBridgeNodeOnHull.Value.Position, isConvex);
                _verticesList.AddAfter(_currentVertexNode, repeatStartHullVert);

                //Set concavity of initial hull bridge vert, since it may have changed now that it leads to hole vert
                LinkedListNode<Vertex> nodeBeforeStartBridgeNodeOnHull = (validBridgeNodeOnHull.Previous == null) ? _verticesList.Last : validBridgeNodeOnHull.Previous;
                LinkedListNode<Vertex> nodeAfterStartBridgeNodeOnHull = (validBridgeNodeOnHull.Next == null) ? _verticesList.First : validBridgeNodeOnHull.Next;
                validBridgeNodeOnHull.Value.IsConvex = IsConvex(nodeBeforeStartBridgeNodeOnHull.Value.Position, validBridgeNodeOnHull.Value.Position, nodeAfterStartBridgeNodeOnHull.Value.Position);
            }

            return _verticesList; 
        }

        public static bool IsConvex(Vector2 v0, Vector2 v1, Vector2 v2)
        {
            return GeometryHelper.AngleSign(v0, v2, v1) == 1; 
        }

        public struct HoleData
        {
            public readonly int holeIndex;
            public readonly int bridgeIndex;
            public readonly Vector2 bridgePoint;

            public HoleData(int holeIndex, int bridgeIndex, Vector2 bridgePoint)
            {
                this.holeIndex = holeIndex;
                this.bridgeIndex = bridgeIndex;
                this.bridgePoint = bridgePoint;
            }
        }
    }

}
