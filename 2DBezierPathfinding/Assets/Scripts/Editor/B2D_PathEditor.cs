using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor; 

[CustomEditor(typeof(B2D_Path))]
public class B2D_PathEditor : Editor
{
    #region Field and Properties
    private SerializedProperty m_pathPoints = null;
    private SerializedProperty m_segments = null;

    private int m_selectedIndex = -1; 
    #endregion


    #region Methods
    #region Original Methods
    /// <summary>
    /// Draw the path in the SceneGUI
    /// </summary>
    private void DrawPath()
    {
        SerializedProperty _p;
        for (int i = 0; i < m_pathPoints.arraySize; i++)
        {
            _p = m_pathPoints.GetArrayElementAtIndex(i);
            Handles.color = m_selectedIndex == i ? Color.yellow : Color.red;
            EditorGUI.BeginChangeCheck();
            _p.FindPropertyRelative("m_position").vector2Value = Handles.FreeMoveHandle(_p.FindPropertyRelative("m_position").vector2Value, Quaternion.identity, .1f, Vector2.zero, Handles.CylinderHandleCap);
            if(EditorGUI.EndChangeCheck())
            {
                m_selectedIndex = i; 
            }
            Handles.Label(_p.FindPropertyRelative("m_position").vector2Value, "Point " + i);
            if (Handles.Button(_p.FindPropertyRelative("m_position").vector2Value + Vector2.one*.05f, Quaternion.identity, .03f, .03f, Handles.RectangleHandleCap))
            {
                AddNewSegment(i); 
            }

        }
        if (Event.current.control && m_selectedIndex >= 0)
        {
            Vector2 _currentPosition = m_pathPoints.GetArrayElementAtIndex(m_selectedIndex).FindPropertyRelative("m_position").vector2Value;
            Vector2 _otherPosition; 
            for (int i = 0; i < m_pathPoints.arraySize; i++)
            {
                if (i == m_selectedIndex) continue;
                _otherPosition = m_pathPoints.GetArrayElementAtIndex(i).FindPropertyRelative("m_position").vector2Value;
                if (Vector2.Distance(_currentPosition, _otherPosition) < .05f)
                {
                    LinkPoints(m_selectedIndex, i);
                    break; 
                }
            }
        }

        Handles.color = Color.blue;
        Vector2 _startPosition, _endPosition; 
        for (int i = 0; i < m_segments.arraySize; i++)
        {
            _p = m_segments.GetArrayElementAtIndex(i);
            _startPosition = m_pathPoints.GetArrayElementAtIndex(_p.FindPropertyRelative("m_inAnchorIndex").intValue).FindPropertyRelative("m_position").vector2Value;
            _endPosition = m_pathPoints.GetArrayElementAtIndex(_p.FindPropertyRelative("m_outAnchorIndex").intValue).FindPropertyRelative("m_position").vector2Value;
            _p.FindPropertyRelative("m_inControlOffset").vector2Value = (Vector2)Handles.FreeMoveHandle(_startPosition + _p.FindPropertyRelative("m_inControlOffset").vector2Value, Quaternion.identity, .05f, Vector2.zero, Handles.CylinderHandleCap) - _startPosition;
            Handles.DrawLine(_startPosition, _startPosition + _p.FindPropertyRelative("m_inControlOffset").vector2Value);
            _p.FindPropertyRelative("m_outControlOffset").vector2Value =(Vector2)Handles.FreeMoveHandle(_endPosition + _p.FindPropertyRelative("m_outControlOffset").vector2Value, Quaternion.identity, .05f, Vector2.zero, Handles.CylinderHandleCap) - _endPosition;
            Handles.DrawLine(_endPosition, _endPosition + _p.FindPropertyRelative("m_outControlOffset").vector2Value);

            Handles.DrawBezier(_startPosition, _endPosition, _startPosition + _p.FindPropertyRelative("m_inControlOffset").vector2Value, _endPosition + _p.FindPropertyRelative("m_outControlOffset").vector2Value, Color.green, null, 1.0f); 
            
            if(Handles.Button(B2D_BezierUtility.CubicCurve(_startPosition, _endPosition, _startPosition + _p.FindPropertyRelative("m_inControlOffset").vector2Value, _endPosition + _p.FindPropertyRelative("m_outControlOffset").vector2Value, .5f), Quaternion.identity, .05f, .05f, Handles.RectangleHandleCap))
            {
                RemoveSegment(i); 
            }
        }
        serializedObject.ApplyModifiedProperties(); 
    }

    /// <summary>
    /// Initialize the path by creating two path points and one segment between them
    /// </summary>
    private void InitPath()
    {
        m_pathPoints.InsertArrayElementAtIndex(0); // This will be the point at the index count - 2
        m_pathPoints.InsertArrayElementAtIndex(0); // This will be the point at the index count - 1

        SerializedProperty _prop = m_pathPoints.GetArrayElementAtIndex(0);
        _prop.FindPropertyRelative("m_position").vector2Value = Vector2.left;
        _prop.FindPropertyRelative("m_linkedPointsIndexes").InsertArrayElementAtIndex(0);
        _prop.FindPropertyRelative("m_linkedPointsIndexes").GetArrayElementAtIndex(0).intValue = m_pathPoints.arraySize - 1;

        _prop = m_pathPoints.GetArrayElementAtIndex(1);
        _prop.FindPropertyRelative("m_position").vector2Value = -Vector2.left;
        _prop.FindPropertyRelative("m_linkedPointsIndexes").InsertArrayElementAtIndex(0);
        _prop.FindPropertyRelative("m_linkedPointsIndexes").GetArrayElementAtIndex(0).intValue = m_pathPoints.arraySize - 2;

        m_segments.InsertArrayElementAtIndex(0);
        _prop = m_segments.GetArrayElementAtIndex(0);
        _prop.FindPropertyRelative("m_inAnchorIndex").intValue = 0;
        _prop.FindPropertyRelative("m_outAnchorIndex").intValue = 1;
        _prop.FindPropertyRelative("m_inControlOffset").vector2Value = Vector2.up;
        _prop.FindPropertyRelative("m_outControlOffset").vector2Value = Vector2.down;

        serializedObject.ApplyModifiedProperties(); 
    }

    /// <summary>
    /// Add a new segment after the point at the index <paramref name="_index"/> of the path points array
    /// </summary>
    /// <param name="_index">Index of the previous path point</param>
    private void AddNewSegment(int _index)
    {
        m_pathPoints.InsertArrayElementAtIndex(m_pathPoints.arraySize);
        SerializedProperty _prop = m_pathPoints.GetArrayElementAtIndex(m_pathPoints.arraySize - 1);
        _prop.FindPropertyRelative("m_linkedPointsIndexes").ClearArray();
        _prop.FindPropertyRelative("m_position").vector2Value = m_pathPoints.GetArrayElementAtIndex(_index).FindPropertyRelative("m_position").vector2Value + Vector2.right;
        _prop.FindPropertyRelative("m_linkedPointsIndexes").InsertArrayElementAtIndex(0);
        _prop.FindPropertyRelative("m_linkedPointsIndexes").GetArrayElementAtIndex(0).intValue = _index;

        _prop = m_pathPoints.GetArrayElementAtIndex(_index);
        _prop.FindPropertyRelative("m_linkedPointsIndexes").InsertArrayElementAtIndex(0);
        _prop.FindPropertyRelative("m_linkedPointsIndexes").GetArrayElementAtIndex(0).intValue = m_pathPoints.arraySize -1 ;


        m_segments.InsertArrayElementAtIndex(m_segments.arraySize);
        _prop = m_segments.GetArrayElementAtIndex(m_segments.arraySize - 1);
        _prop.FindPropertyRelative("m_inAnchorIndex").intValue = _index;
        _prop.FindPropertyRelative("m_outAnchorIndex").intValue = m_pathPoints.arraySize - 1;

        serializedObject.ApplyModifiedProperties(); 
    }

    /// <summary>
    /// Remove the segment at the <paramref name="_index"/> of the segment array.
    /// Reorganize the indexes to match with the removing of the point
    /// </summary>
    /// <param name="_index"></param>
    private void RemoveSegment(int _index)
    {
        SerializedProperty _property = m_segments.GetArrayElementAtIndex(_index);
        SerializedProperty _previousPoint = m_pathPoints.GetArrayElementAtIndex(_property.FindPropertyRelative("m_inAnchorIndex").intValue);
        SerializedProperty _nextPoint = m_pathPoints.GetArrayElementAtIndex(_property.FindPropertyRelative("m_outAnchorIndex").intValue);
        int _removedIndex = -1;
        int _leftIndex = -1; 
        SerializedProperty _prop;
        int _sameCount = 0; 
        // BEFORE DELETING MAKE SURE THERE IS NO LOOP LINK BETWEEN THE REMOVED POINTS
        for (int i = 0; i < _previousPoint.FindPropertyRelative("m_linkedPointsIndexes").arraySize; i++)
        {
            if (_previousPoint.FindPropertyRelative("m_linkedPointsIndexes").GetArrayElementAtIndex(i).intValue == _property.FindPropertyRelative("m_outAnchorIndex").intValue)
                _sameCount++; 
        }

        /* IF THERE IS MULTIPLE TIME THE SAME LINK --> loop*/
        if(_sameCount > 1)
        {
            for (int i = 0; i < _previousPoint.FindPropertyRelative("m_linkedPointsIndexes").arraySize; i++)
            {
                if (_previousPoint.FindPropertyRelative("m_linkedPointsIndexes").GetArrayElementAtIndex(i).intValue == _property.FindPropertyRelative("m_outAnchorIndex").intValue)
                {
                    if (_previousPoint.FindPropertyRelative("m_linkedPointsIndexes").arraySize == 1) _previousPoint.FindPropertyRelative("m_linkedPointsIndexes").ClearArray(); 
                    else _previousPoint.FindPropertyRelative("m_linkedPointsIndexes").DeleteArrayElementAtIndex(i);
                    break; 
                }
            }
            for (int i = 0; i < _nextPoint.FindPropertyRelative("m_linkedPointsIndexes").arraySize; i++)
            {
                if (_nextPoint.FindPropertyRelative("m_linkedPointsIndexes").GetArrayElementAtIndex(i).intValue == _property.FindPropertyRelative("m_inAnchorIndex").intValue)
                {
                    if (_nextPoint.FindPropertyRelative("m_linkedPointsIndexes").arraySize == 1) _nextPoint.FindPropertyRelative("m_linkedPointsIndexes").ClearArray();
                    else _nextPoint.FindPropertyRelative("m_linkedPointsIndexes").DeleteArrayElementAtIndex(i);
                    break;
                }
            }
            m_segments.DeleteArrayElementAtIndex(_index);
            return;
        }

        if (_nextPoint.FindPropertyRelative("m_linkedPointsIndexes").arraySize == 1)
        {
            _leftIndex = _property.FindPropertyRelative("m_inAnchorIndex").intValue;
            _removedIndex = _property.FindPropertyRelative("m_outAnchorIndex").intValue;
        }
        else
        {
            _leftIndex = _property.FindPropertyRelative("m_outAnchorIndex").intValue;
            _removedIndex = _property.FindPropertyRelative("m_inAnchorIndex").intValue;
            
        }
        LinkPoints(_removedIndex, _leftIndex);
        return;
    }

    private void LinkPoints(int _removedIndex, int _linkedIndex)
    {
        // BEFORE DELETING MAKE SURE: 
        // THE LINKED POINT AND THE REMOVED POINTS AREN'T LINKED TO EACH OTHER --> The linked point will be linked to itself
        // If they are linked, we have to delete the segment between them and remove the link between them
        SerializedProperty _prop;

        for (int i = 0; i < m_segments.arraySize; i++)
        {
            _prop = m_segments.GetArrayElementAtIndex(i); 
            if((_prop.FindPropertyRelative("m_inAnchorIndex").intValue == _removedIndex && _prop.FindPropertyRelative("m_outAnchorIndex").intValue == _linkedIndex)
               || (_prop.FindPropertyRelative("m_inAnchorIndex").intValue == _linkedIndex && _prop.FindPropertyRelative("m_outAnchorIndex").intValue == _removedIndex))
            {
                m_segments.DeleteArrayElementAtIndex(i);
                break; 
            }
        }

        _prop = m_pathPoints.GetArrayElementAtIndex(_removedIndex).FindPropertyRelative("m_linkedPointsIndexes");
        SerializedProperty _keptProperty = m_pathPoints.GetArrayElementAtIndex(_linkedIndex).FindPropertyRelative("m_linkedPointsIndexes");
        for (int i = 0; i < _prop.arraySize; i++)
        {
            _keptProperty.InsertArrayElementAtIndex(0);
            _keptProperty.GetArrayElementAtIndex(0).intValue = _prop.GetArrayElementAtIndex(i).intValue; 
        }
        for (int i = 0; i < _keptProperty.arraySize; i++)
        {
            if(_keptProperty.GetArrayElementAtIndex(i).intValue == _removedIndex || _keptProperty.GetArrayElementAtIndex(i).intValue == _linkedIndex)
            {
                _keptProperty.DeleteArrayElementAtIndex(i);
                i--; 
            }
        }

        m_pathPoints.DeleteArrayElementAtIndex(_removedIndex); 
        for (int i = 0; i < m_segments.arraySize; i++)
        {
            _prop = m_segments.GetArrayElementAtIndex(i);
            if (_prop.FindPropertyRelative("m_inAnchorIndex").intValue == _removedIndex)
                _prop.FindPropertyRelative("m_inAnchorIndex").intValue = _linkedIndex;
            else if(_prop.FindPropertyRelative("m_inAnchorIndex").intValue > _removedIndex)
                _prop.FindPropertyRelative("m_inAnchorIndex").intValue--;

            if (_prop.FindPropertyRelative("m_outAnchorIndex").intValue == _removedIndex)
                _prop.FindPropertyRelative("m_outAnchorIndex").intValue = _linkedIndex;
            else if (_prop.FindPropertyRelative("m_outAnchorIndex").intValue > _removedIndex)
                _prop.FindPropertyRelative("m_outAnchorIndex").intValue--;
        }

        for (int i = 0; i < m_pathPoints.arraySize; i++)
        {
            _prop = m_pathPoints.GetArrayElementAtIndex(i);
            for (int j = 0; j < _prop.FindPropertyRelative("m_linkedPointsIndexes").arraySize; j++)
            {
                if (_prop.FindPropertyRelative("m_linkedPointsIndexes").GetArrayElementAtIndex(j).intValue == _removedIndex)
                    _prop.FindPropertyRelative("m_linkedPointsIndexes").GetArrayElementAtIndex(j).intValue = _linkedIndex;
                else if (_prop.FindPropertyRelative("m_linkedPointsIndexes").GetArrayElementAtIndex(j).intValue > _removedIndex)
                    _prop.FindPropertyRelative("m_linkedPointsIndexes").GetArrayElementAtIndex(j).intValue --;
            }
        }
        m_selectedIndex = -1; 
    }
    #endregion

    #region Unity Methods
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI(); 
        if(m_selectedIndex != -1)
        {
            if(GUILayout.Button("Add new segment after the selected point"))
            {
                AddNewSegment(m_selectedIndex); 
            }
        }
    }

    private void OnEnable()
    {
        m_pathPoints = serializedObject.FindProperty("m_pathPoints");
        m_segments = serializedObject.FindProperty("m_segments"); 
        if(m_pathPoints.arraySize == 0)
        {
            InitPath();
        }
    }

    private void OnSceneGUI()
    {
        DrawPath();
    }
    #endregion
    #endregion
}
