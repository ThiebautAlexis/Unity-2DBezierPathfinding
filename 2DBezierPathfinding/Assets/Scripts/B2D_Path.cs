using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class B2D_Path : MonoBehaviour
{
    #region Fields and Properties
    [SerializeField] private B2D_PathPoint[] m_pathPoints = new B2D_PathPoint[] { };
    [SerializeField] private B2D_Segment[] m_segments = new B2D_Segment[] { };
    #endregion

}

[System.Serializable]
public class B2D_PathPoint
{
    [SerializeField] private Vector2 m_position;
    [SerializeField] private List<int> m_linkedPointsIndexes = new List<int>(); 
}

[System.Serializable]
public class B2D_Segment
{
    [SerializeField] private int m_inAnchorIndex;
    [SerializeField] private int m_outAnchorIndex;

    [SerializeField] private Vector2 m_inControlOffset;
    [SerializeField] private Vector2 m_outControlOffset;

}

