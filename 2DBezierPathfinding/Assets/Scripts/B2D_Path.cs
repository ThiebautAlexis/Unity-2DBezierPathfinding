using System.Linq;
using System.Collections.Generic;
using System.Collections; 
using UnityEngine;

public class B2D_Path : MonoBehaviour
{
    #region Fields and Properties
    [SerializeField] private B2D_PathPoint[] m_pathPoints = new B2D_PathPoint[] { };
    public B2D_PathPoint[] PathPoints { get { return m_pathPoints; } }

    [SerializeField] private B2D_Segment[] m_segments = new B2D_Segment[] { };
    #endregion

    public bool ComputePath(Vector2 _startPosition, Vector2 _endPosition, out List<int> _finalPathIndexes)
    {
        Dictionary<int, int> _cameFromIndex = new Dictionary<int, int>();
        _finalPathIndexes = new List<int>(); 
        List<int> _frontier = new List<int>();
        int _startIndex = GetClosestPathPointIndex(_startPosition);
        int _endIndex = GetClosestPathPointIndex(_endPosition);
        int _currentIndex; 
        _cameFromIndex.Add(_startIndex, -1); 
        _frontier.Add(_startIndex);
        while (_frontier.Count > 0)
        {
            _currentIndex = _frontier.First(); 
            if(_currentIndex == _endIndex)
            {
                _cameFromIndex.Add(-1, _endIndex);
                _finalPathIndexes = BuildPath(_cameFromIndex);
                return true; 
            }
            foreach (int i in m_pathPoints[_currentIndex].LinkedPointsIndexes)
            {
                if(!_cameFromIndex.ContainsKey(i))
                {
                    _frontier.Add(i);
                    _cameFromIndex.Add(i, _currentIndex); 
                }
            }
            _frontier.RemoveAt(0); 
        }
        return false;
    }

    public List<int> BuildPath(Dictionary<int, int> _camefromIndexes)
    {
        List<int> _path = new List<int>();
        int _currentIndex = _camefromIndexes[-1]; 
        while (_currentIndex != -1)
        {
            _path.Add(_currentIndex);
            _currentIndex = _camefromIndexes[_currentIndex]; 
        }
        _path.Reverse(); 
        return _path;
    }

    /// <summary>
    /// Get the segment with the path Point in and out at the selected indexes
    /// </summary>
    /// <param name="_pointAIndex">First Index</param>
    /// <param name="_pointBIndex">SecondIndex</param>
    /// <returns></returns>
    public B2D_Segment GetSegment(int _pointAIndex, int _pointBIndex, out bool _reverseSegment)
    {
        _reverseSegment = false;
        if(m_segments.Any(s => s.InAnchorIndex == _pointBIndex && s.OutAnchorIndex == _pointAIndex))
        {
            _reverseSegment = true; 
            return m_segments.Where(s => s.InAnchorIndex == _pointBIndex && s.OutAnchorIndex == _pointAIndex).First(); 
        }
        return m_segments.Where(s => s.InAnchorIndex == _pointAIndex && s.OutAnchorIndex == _pointBIndex).FirstOrDefault(); 
    }

    public int GetClosestPathPointIndex(Vector2 _position)
    {
        int _index = -1;
        float _minDist = 500; 
        for (int i = 0; i < m_pathPoints.Length; i++)
        {
            if (Vector2.Distance(_position, m_pathPoints[i].Position) < _minDist)
            {
                _index = i;
                _minDist = Vector2.Distance(_position, m_pathPoints[i].Position); 
            }
        }
        return _index; 
    }

}

[System.Serializable]
public class B2D_PathPoint
{
    [SerializeField] private Vector2 m_position;
    [SerializeField] private List<int> m_linkedPointsIndexes = new List<int>(); 

    public Vector2 Position { get { return m_position; } }
    public List<int> LinkedPointsIndexes { get { return m_linkedPointsIndexes; } }
}

[System.Serializable]
public class B2D_Segment
{
    [SerializeField] private int m_inAnchorIndex;
    [SerializeField] private int m_outAnchorIndex;

    [SerializeField] private Vector2 m_inControlOffset;
    [SerializeField] private Vector2 m_outControlOffset;


    public int InAnchorIndex { get { return m_inAnchorIndex; } }
    public int OutAnchorIndex { get { return m_outAnchorIndex; } }

    public Vector2 InControlOffset { get { return m_inControlOffset; } }
    public Vector2 OutControlOffset { get { return m_outControlOffset; } }
}

