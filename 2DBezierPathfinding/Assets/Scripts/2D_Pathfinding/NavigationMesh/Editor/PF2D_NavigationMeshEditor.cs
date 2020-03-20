using System.Linq;
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
        private SerializedProperty m_meshHull = null;
        private SerializedProperty m_holes = null; 
        private Vector3 m_localPositionOffset = Vector3.zero;
        private Vector2 m_mousePos;

        private Mesh m_drawnMesh = null; 
        #endregion


        #region Methods

        #region Original Methods
        private void ApplyPositionChanges()
        {
            SerializedProperty _p; 
            if (m_localPositionOffset != ((PF2D_NavigationMesh)serializedObject.targetObject).transform.position)
            {
                Vector2 _offset = ((PF2D_NavigationMesh)serializedObject.targetObject).transform.position - m_localPositionOffset;
                for (int i = 0; i < m_meshHull.FindPropertyRelative("m_vertices").arraySize; i++)
                {
                    _p = m_meshHull.FindPropertyRelative("m_vertices").GetArrayElementAtIndex(i);
                    _p.vector2Value = _p.vector2Value + _offset;
                }
                for (int h = 0; h < m_holes.arraySize; h++)
                {
                    for (int i = 0; i < m_holes.GetArrayElementAtIndex(h).FindPropertyRelative("m_vertices").arraySize; i++)
                    {
                        _p = m_holes.GetArrayElementAtIndex(h).FindPropertyRelative("m_vertices").GetArrayElementAtIndex(i);
                        _p.vector2Value = _p.vector2Value + _offset;
                    }
                }
                m_localPositionOffset = ((PF2D_NavigationMesh)serializedObject.targetObject).transform.position;
            }
        }

        private void InitNavigationMesh()
        {
            m_meshHull.FindPropertyRelative("m_vertices").ClearArray();

            SerializedProperty _property;
            for (int i = 0; i < 3; i++)
            {
                m_meshHull.FindPropertyRelative("m_vertices").InsertArrayElementAtIndex(i);
                _property = m_meshHull.FindPropertyRelative("m_vertices").GetArrayElementAtIndex(i);
                _property.vector2Value = new Vector2(i%2, i>1 ? 1 : 0);
            }
            serializedObject.ApplyModifiedProperties(); 
        }

        private void DrawInspector(SerializedProperty _polygon, string _label, int _index = -1)
        {
            SerializedProperty _prop;
            GUILayout.BeginHorizontal();
            _polygon.FindPropertyRelative("m_displayPoints").boolValue = EditorGUILayout.Foldout(_polygon.FindPropertyRelative("m_displayPoints").boolValue, new GUIContent(_label), true);
            Color _originalColor = GUI.color;
            GUI.color = _polygon.FindPropertyRelative("m_isSelected").boolValue ? Color.grey : Color.white;
            if (GUILayout.Button("Select", GUILayout.MaxWidth((Screen.width / 4))))
            {
                SetSelected(_polygon);
            }
            GUI.color = _originalColor;
            GUILayout.EndHorizontal(); 
            if (_polygon.FindPropertyRelative("m_displayPoints").boolValue)
            {
                if(_index == -1)
                {
                    if (GUILayout.Button("Reverse"))
                    {
                        ReverseArray(_polygon.FindPropertyRelative("m_vertices"));
                    }
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Reverse"))
                    {
                        ReverseArray(_polygon.FindPropertyRelative("m_vertices"));
                    }
                    if (GUILayout.Button("Destroy"))
                    {
                        m_holes.DeleteArrayElementAtIndex(_index);
                        serializedObject.ApplyModifiedProperties(); 
                        UpdateDrawnMesh();
                        GUILayout.EndHorizontal();
                        return; 
                    }
                    GUILayout.EndHorizontal();
                }

                for (int i = 0; i < _polygon.FindPropertyRelative("m_vertices").arraySize; i++)
                {
                    _prop = _polygon.FindPropertyRelative("m_vertices").GetArrayElementAtIndex(i);
                    GUILayout.BeginHorizontal();
                    _prop.vector2Value = EditorGUILayout.Vector2Field($"Point {i}", _prop.vector2Value);
                    if (GUILayout.Button("x", GUILayout.MinWidth(Screen.width / 12), GUILayout.MaxWidth(Screen.width / 8)) && _polygon.FindPropertyRelative("m_vertices").arraySize > 3)
                    {
                        _polygon.FindPropertyRelative("m_vertices").DeleteArrayElementAtIndex(i);
                        serializedObject.ApplyModifiedProperties();
                        UpdateDrawnMesh();
                    }
                    GUILayout.EndHorizontal();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawPolygonHandles(SerializedProperty _polygon, Color _c)
        {
            SerializedProperty _vectorArray = _polygon.FindPropertyRelative("m_vertices"); 
            if (m_meshHull.FindPropertyRelative("m_vertices").arraySize < 3)
            {
                InitNavigationMesh();
            }
            SerializedProperty _p;
            Handles.color = _c;
            for (int i = 0; i < _vectorArray.arraySize; i++)
            {
                _p = _vectorArray.GetArrayElementAtIndex(i);
                _p.vector2Value = Handles.FreeMoveHandle(_p.vector2Value, Quaternion.identity, .05f, Vector2.zero, Handles.DotHandleCap);
                Handles.DrawLine(_p.vector2Value, _vectorArray.GetArrayElementAtIndex((i + 1) % _vectorArray.arraySize).vector2Value); 
                Handles.Label(_p.vector2Value - Vector2.one * .1f, i.ToString());
            }

            if(_polygon.FindPropertyRelative("m_isSelected").boolValue)
            {
                /// Get the two closest vertices to draw a button handle that add a triangle.
                m_mousePos = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;

                int _closestVertex = 0, _nextVertex = 1;
                float _minDist = Vector2.Distance(_vectorArray.GetArrayElementAtIndex(0).vector2Value, m_mousePos);

                float _currentDist = 0.0f;
                for (int i = 2; i < _vectorArray.arraySize; i++)
                {
                    _currentDist = Vector2.Distance(_vectorArray.GetArrayElementAtIndex(i).vector2Value, m_mousePos);
                    if (_currentDist <= .25f)
                    {
                        serializedObject.ApplyModifiedProperties();
                        return;
                    }
                    if (_currentDist < _minDist)
                    {
                        _closestVertex = i;
                        _minDist = _currentDist;
                    }
                }
                _minDist = Vector2.Distance(m_mousePos, _vectorArray.GetArrayElementAtIndex(_closestVertex == _vectorArray.arraySize - 1 ? 0 : _closestVertex + 1).vector2Value);
                _currentDist = Vector2.Distance(m_mousePos, _vectorArray.GetArrayElementAtIndex(_closestVertex == 0 ? _vectorArray.arraySize - 1 : _closestVertex - 1).vector2Value);
                if (_minDist > _currentDist)
                    _closestVertex = _closestVertex == 0 ? _vectorArray.arraySize - 1 : _closestVertex - 1;

                _nextVertex = _closestVertex == _vectorArray.arraySize - 1 ? 0 : _closestVertex + 1;


                Vector2 _normal = Geometry.GeometryHelper.GetNormalPoint(m_mousePos, _vectorArray.GetArrayElementAtIndex(_closestVertex).vector2Value, _vectorArray.GetArrayElementAtIndex(_nextVertex).vector2Value);
                PF2D_ExtendedHandlers.PolygonSigmentHandle(ref _normal, .05f, Quaternion.identity, () => AddVertex(_vectorArray, _closestVertex, m_mousePos));
            }
            HandleUtility.Repaint();
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
            Graphics.DrawMesh(m_drawnMesh, Vector3.zero, Quaternion.identity, AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat") as Material, 1, _cam); 
        }

        private void AddVertex(SerializedProperty _array, int _previousVertex, Vector2 _mousePosition)
        {
            _array.InsertArrayElementAtIndex(_previousVertex + 1);
            _array.GetArrayElementAtIndex(_previousVertex + 1).vector2Value = _mousePosition;
            serializedObject.ApplyModifiedProperties(); 
        }
        
        private void UpdateDrawnMesh()
        {
            serializedObject.ApplyModifiedProperties(); 
            (serializedObject.targetObject as PF2D_NavigationMesh).TriangulateMesh(); 
            m_drawnMesh = new Mesh();
            List<Vector3> _vertices = new List<Vector3>();
            for (int i = 0; i < m_meshHull.FindPropertyRelative("m_vertices").arraySize; i++)
            {
                _vertices.Add(m_meshHull.FindPropertyRelative("m_vertices").GetArrayElementAtIndex(i).vector2Value); 
            }
            for (int i = 0; i < m_holes.arraySize; i++)
            {
                for (int j = 0; j < m_holes.GetArrayElementAtIndex(i).FindPropertyRelative("m_vertices").arraySize; j++)
                {
                    _vertices.Add(m_holes.GetArrayElementAtIndex(i).FindPropertyRelative("m_vertices").GetArrayElementAtIndex(j).vector2Value); 
                }
            }
            SerializedProperty _trianglesProperty = serializedObject.FindProperty("m_triangles"); 
            int[] _triangles = new int[_trianglesProperty.arraySize];
            for (int i = 0; i < _trianglesProperty.arraySize; i++)
            {
                _triangles[i] = _trianglesProperty.GetArrayElementAtIndex(i).intValue;
            }
            m_drawnMesh.vertices = _vertices.ToArray();
            m_drawnMesh.triangles = _triangles;
        }

        private void HandleEvent()
        {
            Event _e = Event.current;
            if (_e.type == EventType.KeyDown && _e.keyCode == KeyCode.Space)
                UpdateDrawnMesh(); 
        }

        private void ReverseArray(SerializedProperty _vectorArray)
        {
            List<Vector2> _positions = new List<Vector2>();
            for (int i = 0; i < _vectorArray.arraySize; i++)
            {
                _positions.Add(_vectorArray.GetArrayElementAtIndex(i).vector2Value);
            }
            _positions.Reverse();
            for (int i = 0; i < _vectorArray.arraySize; i++)
            {
                _vectorArray.GetArrayElementAtIndex(i).vector2Value = _positions[i];
            }
            serializedObject.ApplyModifiedProperties(); 
        }

        private void SetSelected(SerializedProperty _selectedPolygon)
        {
            m_meshHull.FindPropertyRelative("m_isSelected").boolValue = false;
            for (int i = 0; i < m_holes.arraySize; i++)
            {
                m_holes.GetArrayElementAtIndex(i).FindPropertyRelative("m_isSelected").boolValue = false; 
            }
            _selectedPolygon.FindPropertyRelative("m_isSelected").boolValue = true;
            serializedObject.ApplyModifiedProperties(); 
        }
        #endregion

        #region Unity Methods
        private void OnEnable()
        {
            m_localPositionOffset = ((PF2D_NavigationMesh)serializedObject.targetObject).transform.position;
            m_meshHull = serializedObject.FindProperty("m_meshHull");
            m_holes = serializedObject.FindProperty("m_holes"); 
            if(m_meshHull.FindPropertyRelative("m_vertices").arraySize < 3)
            {
                InitNavigationMesh(); 
            }
            UpdateDrawnMesh();
            Camera.onPreCull -= DrawMesh;
            Camera.onPreCull += DrawMesh;
        }

        private void OnDisable()
        {
            Camera.onPreCull -= DrawMesh; 
        }

        private void OnSceneGUI()
        {
            DrawPolygonHandles(m_meshHull, Color.green);
            for (int i = 0; i < m_holes.arraySize; i++)
            {
                SerializedProperty _array = m_holes.GetArrayElementAtIndex(i);
                DrawPolygonHandles(_array, Color.red); 
            }
            HandleEvent();
            ApplyPositionChanges();
        }

        public override void OnInspectorGUI()
        {
            DrawInspector(m_meshHull, "Navigation Mesh Hull");
            GUILayout.Space(5);
            for (int i = 0; i < m_holes.arraySize; i++)
            {
                DrawInspector(m_holes.GetArrayElementAtIndex(i), $"Hole n° {i+1}", i); 
            }
            GUILayout.Space(5);
            if (GUILayout.Button("Add Exclusion Zone"))
            {
                m_holes.InsertArrayElementAtIndex(m_holes.arraySize);
                SerializedProperty _p = m_holes.GetArrayElementAtIndex(m_holes.arraySize - 1).FindPropertyRelative("m_vertices");
                _p.ClearArray();
                for (int i = 0; i < 3; i++)
                {
                    _p.InsertArrayElementAtIndex(i);
                    _p.GetArrayElementAtIndex(i).vector2Value = new Vector2(i % 2, i > 1 ? 1 : 0);
                }
            }
        }
        #endregion

        #endregion
    }
}

