using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using EnhancedHandles;

namespace Pathfinding2D
{
    [CustomEditor(typeof(PF2D_NavigationMesh))]
    public class PF2D_NavigationMeshEditor : Editor
    {
        #region Fields and Properties
        private SerializedProperty m_vertices = null;
        private SerializedProperty m_triangles = null;
        private Vector3 m_localPositionOffset = Vector3.zero;
        private Vector2 m_mousePos;

        private int m_controlID = 0;
        #endregion


        #region Methods

        #region Original Methods
        private void InitNavigationMesh()
        {
            m_vertices.ClearArray();
            m_triangles.ClearArray();

            SerializedProperty _property;
            for (int i = 0; i < 3; i++)
            {
                m_vertices.InsertArrayElementAtIndex(i);
                _property = m_vertices.GetArrayElementAtIndex(i);
                _property.FindPropertyRelative("m_position").vector2Value = new Vector2(i%2, i>1 ? 1 : 0);
            }
            m_triangles.InsertArrayElementAtIndex(0);
            _property = m_triangles.GetArrayElementAtIndex(0).FindPropertyRelative("m_verticesIndex");
            for (int j = 0; j < 3; j++)
            {
                _property.InsertArrayElementAtIndex(j);
                _property.GetArrayElementAtIndex(j).intValue = j;
            }
            _property = m_triangles.GetArrayElementAtIndex(0).FindPropertyRelative("m_vertices");
            for (int j = 0; j < 3; j++)
            {
                _property.InsertArrayElementAtIndex(j);
                _property.GetArrayElementAtIndex(j).vector3Value = m_vertices.GetArrayElementAtIndex( m_triangles.GetArrayElementAtIndex(0).FindPropertyRelative("m_verticesIndex").GetArrayElementAtIndex(j).intValue).FindPropertyRelative("m_position").vector2Value ;
            }
            serializedObject.ApplyModifiedProperties(); 
        }

        private void DrawNavigationMeshHandles()
        {
            m_triangles = serializedObject.FindProperty("m_triangles");
            if (m_vertices.arraySize < 3 || m_triangles.arraySize < 1)
            {
                InitNavigationMesh();
            }
            SerializedProperty _p;
            if (m_localPositionOffset != ((PF2D_NavigationMesh)serializedObject.targetObject).transform.position)
            {
                Vector2 _offset = ((PF2D_NavigationMesh)serializedObject.targetObject).transform.position - m_localPositionOffset;
                for (int i = 0; i < m_vertices.arraySize; i++)
                {
                    _p = m_vertices.GetArrayElementAtIndex(i);
                    _p.FindPropertyRelative("m_position").vector2Value = _p.FindPropertyRelative("m_position").vector2Value + _offset;
                }
                m_localPositionOffset = ((PF2D_NavigationMesh)serializedObject.targetObject).transform.position;
            }
            Handles.color = Color.green; 
            for (int i = 0; i < m_vertices.arraySize; i++)
            {
                _p = m_vertices.GetArrayElementAtIndex(i);
                EditorGUI.BeginChangeCheck(); 
                _p.FindPropertyRelative("m_position").vector2Value = Handles.FreeMoveHandle(_p.FindPropertyRelative("m_position").vector2Value, Quaternion.identity, .05f, Vector2.zero, Handles.DotHandleCap);
                if(EditorGUI.EndChangeCheck())
                {
                    SerializedProperty _triangle;
                    for (int j = 0; j < m_triangles.arraySize; j++)
                    {
                        _triangle = m_triangles.GetArrayElementAtIndex(j);
                        for (int k = 0; k < _triangle.FindPropertyRelative("m_verticesIndex").arraySize; k++)
                        {
                            if (_triangle.FindPropertyRelative("m_verticesIndex").GetArrayElementAtIndex(k).intValue == i)
                            {
                                _triangle.FindPropertyRelative("m_vertices").GetArrayElementAtIndex(k).vector3Value = _p.FindPropertyRelative("m_position").vector2Value;
                                break; 
                            }
                        }
                    }
                }
                Handles.Label(_p.FindPropertyRelative("m_position").vector2Value, i.ToString());
            }
            for (int i = 0; i < m_triangles.arraySize; i++)
            {
                _p = m_triangles.GetArrayElementAtIndex(i);
                for (int j = 0; j < 3; j++)
                {
                    if (j == 2)
                        Handles.DrawLine(m_vertices.GetArrayElementAtIndex(_p.FindPropertyRelative("m_verticesIndex").GetArrayElementAtIndex(j).intValue).FindPropertyRelative("m_position").vector2Value,
                                         m_vertices.GetArrayElementAtIndex(_p.FindPropertyRelative("m_verticesIndex").GetArrayElementAtIndex(0).intValue).FindPropertyRelative("m_position").vector2Value);
                    else
                        Handles.DrawLine(m_vertices.GetArrayElementAtIndex(_p.FindPropertyRelative("m_verticesIndex").GetArrayElementAtIndex(j).intValue).FindPropertyRelative("m_position").vector2Value,
                                         m_vertices.GetArrayElementAtIndex(_p.FindPropertyRelative("m_verticesIndex").GetArrayElementAtIndex(j+1).intValue).FindPropertyRelative("m_position").vector2Value);

                }
            }

            /// Get the two closest vertices to draw a button handle that add a triangle.
            m_mousePos = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
            int _firstVertex = 0, _secondVertex = 1;
            float _minDist1 = Vector2.Distance(m_vertices.GetArrayElementAtIndex(0).FindPropertyRelative("m_position").vector2Value, m_mousePos); 
            float _minDist2 = Vector2.Distance(m_vertices.GetArrayElementAtIndex(1).FindPropertyRelative("m_position").vector2Value, m_mousePos);

            float _currentDist = 0.0f;
            for (int i = 2; i < m_vertices.arraySize; i++)
            {
                _currentDist = Vector2.Distance(m_vertices.GetArrayElementAtIndex(i).FindPropertyRelative("m_position").vector2Value, m_mousePos); 
                if(_currentDist <= .25f)
                {
                    serializedObject.ApplyModifiedProperties();
                    return; 
                }
                if (_minDist1 > _minDist2 && _currentDist < _minDist1)
                    _firstVertex = i;
                else if (_minDist1 < _minDist2 && _currentDist < _minDist1)
                    _secondVertex = i;
                else if (_minDist1 > _minDist2 && _currentDist < _minDist2)
                    _firstVertex = i;
                else if (_minDist1 < _minDist2 && _currentDist < _minDist2)
                    _secondVertex = i;  

            }
            if (_minDist1 <= .25f || _minDist2 <= .25f || _minDist1 > 5.0f || _minDist2 > 5.0f)
            {
                serializedObject.ApplyModifiedProperties();
                return;
            }
            HandleUtility.Repaint();
            Handles.color = Color.red;
            Handles.DrawLine(m_mousePos, m_vertices.GetArrayElementAtIndex(_firstVertex).FindPropertyRelative("m_position").vector2Value);
            Handles.DrawLine(m_mousePos, m_vertices.GetArrayElementAtIndex(_secondVertex).FindPropertyRelative("m_position").vector2Value);

            Vector2 _normal = Geometry.GeometryHelper.GetNormalPoint(m_mousePos, m_vertices.GetArrayElementAtIndex(_firstVertex).FindPropertyRelative("m_position").vector2Value, m_vertices.GetArrayElementAtIndex(_secondVertex).FindPropertyRelative("m_position").vector2Value);
            PF2D_ExtendedHandlers.PolygonSigmentHandle(ref _normal, .05f, Quaternion.identity, () => AddNewTriangle(_firstVertex, _secondVertex)); 

            serializedObject.ApplyModifiedProperties();
        }

        private void AddNewTriangle(int _firstVertex, int _secondVertex)
        {
            Debug.Log("Add a new triangle with the Vertices " + _firstVertex + " and " + _secondVertex); 
        }
        
        #endregion

        #region Unity Methods
        private void OnEnable()
        {
            m_localPositionOffset = ((PF2D_NavigationMesh)serializedObject.targetObject).transform.position;
            m_vertices = serializedObject.FindProperty("m_vertices");
            m_triangles = serializedObject.FindProperty("m_triangles"); 
            if(m_vertices.arraySize < 3 || m_triangles.arraySize < 1)
            {
                InitNavigationMesh(); 
            }
        }

        private void OnSceneGUI()
        {
            DrawNavigationMeshHandles();
        }
        #endregion

        #endregion
    }
}

