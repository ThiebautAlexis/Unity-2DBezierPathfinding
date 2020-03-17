using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor; 

namespace EnhancedHandles
{
    public class PF2D_PolygonSigmentHandle
    {
        public Matrix4x4 Matrix;
        public bool FaceCamera = false; 

        public Color Color = Handles.color;
        public Color HoveredColor = Handles.preselectionColor;
        public Color SelectedColor = Handles.selectedColor;

        public float Distance { get; private set; }
        public int ControlId { get; private set; }

        private int m_polygonSigmentHandleHash = "PolygonSigmentHandle".GetHashCode();
        private bool m_selected = false;
        private bool m_hovered = false;

        public Vector2 DrawHandle(Vector2 _position, float _size, System.Action _onHandleClicked)
        {
            return DrawHandle(m_polygonSigmentHandleHash, _position, _size, _onHandleClicked);
        }

        public Vector2 DrawHandle(int _controlID, Vector2 _position, float _size, System.Action _onHandleClicked)
        {
            ControlId = _controlID;
            m_selected = GUIUtility.hotControl == ControlId || GUIUtility.keyboardControl == ControlId;
            m_hovered = HandleUtility.nearestControl == ControlId;
            Event _e = Event.current;
            switch (_e.type)
            {
                case EventType.MouseDown:
                    if (HandleUtility.nearestControl == ControlId && _e.button == 0)
                    {
                        GUIUtility.hotControl = ControlId;
                        GUIUtility.keyboardControl = ControlId;
                        _e.Use();
                        // Call the Action here
                        _onHandleClicked?.Invoke(); 
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == ControlId && _e.button == 0)
                    {
                        GUIUtility.hotControl = 0;
                        _e.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    break;
                case EventType.Repaint:
                    Vector3 worldPos = Matrix.MultiplyPoint3x4(_position);
                    Vector3 camRight = Camera.current.transform.right * _size;
                    Vector3 camUp = Camera.current.transform.up * _size;
                    GL.Begin(GL.QUADS);
                    {
                        GL.Vertex(worldPos + camRight + camUp);
                        GL.Vertex(worldPos + camRight - camUp);
                        GL.Vertex(worldPos - camRight - camUp);
                        GL.Vertex(worldPos - camRight + camUp);
                    }
                    GL.End();
                    break;
                case EventType.Layout:
                    Vector3 pointWorldPos = Matrix.MultiplyPoint3x4(_position);
                    float distance = HandleUtility.DistanceToRectangle(pointWorldPos, Camera.current.transform.rotation, _size);
                    HandleUtility.AddControl(ControlId, distance);
                    break;
            }
            return _position;
        }
    }
}
