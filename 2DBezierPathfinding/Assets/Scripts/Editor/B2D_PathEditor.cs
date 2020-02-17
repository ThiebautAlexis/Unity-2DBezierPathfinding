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
    
    private void DrawPath()
    {
        SerializedProperty _p;
        for (int i = 0; i < m_pathPoints.arraySize; i++)
        {
            _p = m_pathPoints.GetArrayElementAtIndex(i);
            Handles.color = m_selectedIndex == i ? Color.yellow : Color.red;
            EditorGUI.BeginChangeCheck(); 
            _p.FindPropertyRelative("m_position").vector2Value = Handles.FreeMoveHandle(_p.FindPropertyRelative("m_position").vector2Value, Quaternion.identity, .05f, Vector2.zero, Handles.CylinderHandleCap); 
            if(EditorGUI.EndChangeCheck())
            {
                m_selectedIndex = i; 
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
                if(m_pathPoints.arraySize > 2)
                    RemoveSegment(i); 
            }
        }
        serializedObject.ApplyModifiedProperties(); 
    }

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

    private void ProcessEvents(Event _e)
    {
        switch (_e.type)
        {
            case EventType.MouseDown:
                if (_e.control )
                {
                    Vector2 _mousePosition = Camera.current.ScreenPointToRay(_e.mousePosition).origin;
                    int _closestIndex = -1;
                    float _closestDist = .5f;
                    for (int i = 0; i < m_pathPoints.arraySize; i++)
                    {
                        if (Vector2.Distance(m_pathPoints.GetArrayElementAtIndex(i).FindPropertyRelative("m_position").vector2Value, _mousePosition) < _closestDist)
                        {
                            _closestIndex = i;
                            _closestDist = Vector2.Distance(m_pathPoints.GetArrayElementAtIndex(i).FindPropertyRelative("m_position").vector2Value, _mousePosition);
                        }
                    }
                    if (_closestIndex != -1)
                    {
                        AddNewSegment(_closestIndex); 
                    }
                }
                break;
            default:
                break;
        }
    }


    private void RemoveSegment(int _index)
    {
        
        SerializedProperty _property = m_segments.GetArrayElementAtIndex(_index);
        SerializedProperty _previousPoint = m_pathPoints.GetArrayElementAtIndex(_property.FindPropertyRelative("m_inAnchorIndex").intValue);
        SerializedProperty _nextPoint = m_pathPoints.GetArrayElementAtIndex(_property.FindPropertyRelative("m_outAnchorIndex").intValue);

        if(_previousPoint.FindPropertyRelative("m_linkedPointsIndexes").arraySize == 1 || _nextPoint.FindPropertyRelative("m_linkedPointsIndexes").arraySize == 1)
        {
            Debug.Log("Just Destroy"); 
        }
        else
        {
            Debug.Log("Set indexes again");
        }

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
        ProcessEvents(Event.current);
        DrawPath();
    }
    #endregion
    #endregion
}
