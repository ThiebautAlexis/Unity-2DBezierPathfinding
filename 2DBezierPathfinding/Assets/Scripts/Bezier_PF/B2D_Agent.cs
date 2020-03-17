using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class B2D_Agent : MonoBehaviour
{
    #region Fields and Properties
    [SerializeField] private B2D_Path m_currentNavigationPath = null;
    [SerializeField] private Vector2 m_destination = Vector2.zero;
    [SerializeField] private float m_speed = 2.0f; 
    private Coroutine m_movementCoroutine = null; 
    #endregion

    #region Methods

    #region Original Methods 
    private IEnumerator MoveAlongPath(List<int> _pathIndexes, Vector2 _destination)
    {
        float _delta = 0;
        float _distance = 0; 
        Vector2 _startPosition = transform.position;
        Vector2 _targetPosition = m_currentNavigationPath.PathPoints[_pathIndexes[0]].Position;
        Vector2 _startTangent, _endTangent;
        B2D_Segment _currentSegment;
        bool _reverseSegment = false;
        _distance = Vector2.Distance(_startPosition, _targetPosition); 
        while (_delta <= 1)
        {
            transform.position = Vector2.Lerp(_startPosition, _targetPosition, _delta);
            yield return null;
            _delta += Time.deltaTime / _distance * m_speed; 
        }
        for (int i = 0; i < _pathIndexes.Count -1; i++)
        {
            _delta = 0;
            _startPosition = transform.position;
            _targetPosition = m_currentNavigationPath.PathPoints[_pathIndexes[i + 1]].Position;
            _currentSegment = m_currentNavigationPath.GetSegment(_pathIndexes[i], _pathIndexes[i + 1], out _reverseSegment);
            _startTangent = _reverseSegment ? (Vector2)transform.position + _currentSegment.OutControlOffset : (Vector2)transform.position + _currentSegment.InControlOffset;
            _endTangent = _reverseSegment ? _targetPosition + _currentSegment.InControlOffset : _targetPosition + _currentSegment.OutControlOffset;
            _distance = B2D_BezierUtility.GetBezierLength(_startPosition, _targetPosition, _startTangent, _endTangent); 
            while (_delta <= 1)
            {
                transform.position = B2D_BezierUtility.EvaluateCubicCurve(_startPosition, _targetPosition, _startTangent, _endTangent, _delta);
                yield return null;
                _delta += Time.deltaTime / _distance * m_speed;
            }
        }
        _startPosition = transform.position;
        _targetPosition = _destination;
        _delta = 0;
        _distance = Vector2.Distance(_startPosition, _targetPosition); 
        while (_delta <= 1)
        {
            transform.position = Vector2.Lerp(_startPosition, _targetPosition, _delta);
            yield return null;
            _delta += Time.deltaTime / _distance * m_speed;
        }
        m_movementCoroutine = null; 
    }

    private void SetPath(Vector2 _destination)
    {
        if (m_currentNavigationPath == null) return;
        List<int> _pathIndexes; 
        if (m_currentNavigationPath.ComputePath(transform.position, _destination, out _pathIndexes))
        {
            if(m_movementCoroutine != null)
            {
                StopCoroutine(m_movementCoroutine);
                m_movementCoroutine = null; 
            }
            m_movementCoroutine = StartCoroutine(MoveAlongPath(_pathIndexes, _destination));
        }
    }
    #endregion

    #region Unity Methods
    private void Start()
    {
        SetPath(m_destination); 
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, .1f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(m_destination, .1f); 
    }
    #endregion

    #endregion


}
