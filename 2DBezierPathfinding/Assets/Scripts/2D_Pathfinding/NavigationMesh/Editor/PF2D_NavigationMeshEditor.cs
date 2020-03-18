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
        private Vector3 m_localPositionOffset = Vector3.zero;
        private Vector2 m_mousePos;

        private Mesh m_drawnMesh = null; 
        #endregion


        #region Methods

        #region Original Methods
        private void InitNavigationMesh()
        {
            m_vertices.ClearArray();

            SerializedProperty _property;
            for (int i = 0; i < 3; i++)
            {
                m_vertices.InsertArrayElementAtIndex(i);
                _property = m_vertices.GetArrayElementAtIndex(i);
                _property.vector2Value = new Vector2(i%2, i>1 ? 1 : 0);
            }
            serializedObject.ApplyModifiedProperties(); 
        }

        private void DrawNavigationMeshHandles()
        {
            if (m_vertices.arraySize < 3)
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
                    _p.vector2Value = _p.vector2Value + _offset;
                }
                m_localPositionOffset = ((PF2D_NavigationMesh)serializedObject.targetObject).transform.position;
            }
            Handles.color = Color.green; 
            for (int i = 0; i < m_vertices.arraySize; i++)
            {
                _p = m_vertices.GetArrayElementAtIndex(i);
                _p.vector2Value = Handles.FreeMoveHandle(_p.vector2Value, Quaternion.identity, .05f, Vector2.zero, Handles.DotHandleCap);
                Handles.DrawLine(_p.vector2Value, m_vertices.GetArrayElementAtIndex((i + 1) % m_vertices.arraySize).vector2Value); 
                Handles.Label(_p.vector2Value, i.ToString());
            }
            /// Get the two closest vertices to draw a button handle that add a triangle.
            m_mousePos = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
            
            int _closestVertex = 0, _nextVertex = 1;
            float _minDist = Vector2.Distance(m_vertices.GetArrayElementAtIndex(0).vector2Value, m_mousePos); 

            float _currentDist = 0.0f;
            for (int i = 2; i < m_vertices.arraySize; i++)
            {
                _currentDist = Vector2.Distance(m_vertices.GetArrayElementAtIndex(i).vector2Value, m_mousePos); 
                if(_currentDist <= .25f)
                {
                    serializedObject.ApplyModifiedProperties();
                    return; 
                }
                if ( _currentDist < _minDist)
                {
                    _closestVertex = i;
                    _minDist = _currentDist; 
                }
            }
            _minDist = Vector2.Distance(m_mousePos, m_vertices.GetArrayElementAtIndex(_closestVertex == m_vertices.arraySize-1 ? 0 : _closestVertex+1).vector2Value); 
            _currentDist = Vector2.Distance(m_mousePos, m_vertices.GetArrayElementAtIndex(_closestVertex == 0 ? m_vertices.arraySize - 1 : _closestVertex - 1).vector2Value);
            if (_minDist > _currentDist)
                _closestVertex = _closestVertex == 0 ? m_vertices.arraySize - 1 : _closestVertex - 1; 

            _nextVertex = _closestVertex == m_vertices.arraySize - 1 ? 0 : _closestVertex + 1;

            HandleUtility.Repaint();
            Handles.color = Color.red;
            Handles.DrawLine(m_mousePos, m_vertices.GetArrayElementAtIndex(_closestVertex).vector2Value);
            Handles.DrawLine(m_mousePos, m_vertices.GetArrayElementAtIndex(_nextVertex).vector2Value);

            Vector2 _normal = Geometry.GeometryHelper.GetNormalPoint(m_mousePos, m_vertices.GetArrayElementAtIndex(_closestVertex).vector2Value, m_vertices.GetArrayElementAtIndex(_nextVertex).vector2Value);
            PF2D_ExtendedHandlers.PolygonSigmentHandle(ref _normal, .05f, Quaternion.identity, () => AddVertex(_closestVertex, m_mousePos));

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive)); 
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawMesh(Camera _cam)
        {
            if (m_drawnMesh == null)
            {
                Debug.Log("Mesh is null");
                return;
            }
            Graphics.DrawMesh(m_drawnMesh, (serializedObject.targetObject as PF2D_NavigationMesh).transform.position, Quaternion.identity, AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat") as Material, 1, _cam); 
        }

        private void AddVertex(int _previousVertex, Vector2 _mousePosition)
        {
            m_vertices.InsertArrayElementAtIndex(_previousVertex + 1);
            m_vertices.GetArrayElementAtIndex(_previousVertex + 1).vector2Value = _mousePosition;
        }
        
        private void UpdateDrawnMesh()
        {
            serializedObject.ApplyModifiedProperties(); 
            Debug.Log("Update Mesh"); 
            (serializedObject.targetObject as PF2D_NavigationMesh).TriangulateMesh(); 
            m_drawnMesh = new Mesh();
            Vector3[] _vertices = new Vector3[m_vertices.arraySize];
            for (int i = 0; i < m_vertices.arraySize; i++)
            {
                _vertices[i] = m_vertices.GetArrayElementAtIndex(i).vector2Value; 
            }
            SerializedProperty _trianglesProperty = serializedObject.FindProperty("m_triangles"); 
            int[] _triangles = new int[_trianglesProperty.arraySize];
            for (int i = 0; i < _trianglesProperty.arraySize; i++)
            {
                _triangles[i] = _trianglesProperty.GetArrayElementAtIndex(i).intValue;
            }
            m_drawnMesh.vertices = _vertices;
            m_drawnMesh.triangles = _triangles;
        }

        private void HandleEvent()
        {
            Event _e = Event.current;
            if (_e.type == EventType.KeyDown && _e.keyCode == KeyCode.Space)
                UpdateDrawnMesh(); 
        }
        #endregion

        #region Unity Methods
        private void OnEnable()
        {
            m_localPositionOffset = ((PF2D_NavigationMesh)serializedObject.targetObject).transform.position;
            m_vertices = serializedObject.FindProperty("m_vertices");
            UpdateDrawnMesh(); 
            if(m_vertices.arraySize < 3)
            {
                InitNavigationMesh(); 
            }
            Camera.onPreCull -= DrawMesh;
            Camera.onPreCull += DrawMesh;
        }

        private void OnDisable()
        {
            Camera.onPreCull -= DrawMesh; 
        }

        private void OnSceneGUI()
        {
            DrawNavigationMeshHandles();
            HandleEvent();
        }
        #endregion

        #endregion
    }
}

