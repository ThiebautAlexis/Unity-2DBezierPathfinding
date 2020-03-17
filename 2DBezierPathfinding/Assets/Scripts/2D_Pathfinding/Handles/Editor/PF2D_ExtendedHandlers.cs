using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnhancedHandles
{
    public static class PF2D_ExtendedHandlers
    {
        private static PF2D_PolygonSigmentHandle m_polygonSigmentHandle = new PF2D_PolygonSigmentHandle(); 

        public static void PolygonSigmentHandle(ref Vector2 _position, float _size, Quaternion _rotation, System.Action _onHandleClicked)
        {
            m_polygonSigmentHandle.Matrix = Matrix4x4.TRS(Vector3.zero, _rotation, Vector3.one);
            m_polygonSigmentHandle.FaceCamera = true; 
            _position = m_polygonSigmentHandle.DrawHandle(_position, _size, _onHandleClicked); 
        }
    }
}

