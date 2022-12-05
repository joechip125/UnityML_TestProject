using System;
using System.Collections;
using System.Collections.Generic;
using MBaske.Sensors.Grid;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class OverlapChecker
{
    Vector3 m_CellScale;
    
    Vector3Int m_GridSize;
    
    LayerMask m_ColliderMask;
    
    Transform m_CenterObject;

    private ColorGridBuffer m_GridBuffer;

    string[] m_DetectableTags;
    
    int m_InitialColliderBufferSize;
    
    int m_MaxColliderBufferSize;

    int m_NumCells;
    
    Vector3 m_HalfCellScale;
    
    Vector3 m_CellCenterOffset;
    
    Vector3[] m_CellLocalPositions;
    

        Collider[] m_ColliderBuffer;

        public event Action<GameObject, int> GridOverlapDetectedAll;
        public event Action<GameObject, int> GridOverlapDetectedClosest;
        public event Action<GameObject, int> GridOverlapDetectedDebug;

        public OverlapChecker(
        Vector3 cellScale,
        Vector3Int gridSize,
        LayerMask colliderMask,
        Transform centerObject,
        string[] detectableTags,
        int initialColliderBufferSize,
        int maxColliderBufferSize)
    {
        m_CellScale = cellScale;
        m_GridSize = gridSize;
        m_ColliderMask = colliderMask;
        m_CenterObject = centerObject;
        m_DetectableTags = detectableTags;
        m_InitialColliderBufferSize = initialColliderBufferSize;
        m_MaxColliderBufferSize = maxColliderBufferSize;

        m_NumCells = gridSize.x * gridSize.z;
        m_HalfCellScale = new Vector3(cellScale.x / 2f, cellScale.y, cellScale.z / 2f);
        m_CellCenterOffset = new Vector3((gridSize.x - 1f) / 2, 0, (gridSize.z - 1f) / 2);

        m_ColliderBuffer = new Collider[Math.Min(m_MaxColliderBufferSize, m_InitialColliderBufferSize)];
        
        InitCellLocalPositions();
    }
    
    public LayerMask ColliderMask
    {
        get { return m_ColliderMask; }
        set { m_ColliderMask = value; }
    }

    /// <summary>
    /// Initializes the local location of the cells
    /// </summary>
    void InitCellLocalPositions()
    {
        m_CellLocalPositions = new Vector3[m_NumCells];

        for (int i = 0; i < m_NumCells; i++)
        {
            m_CellLocalPositions[i] = GetCellLocalPosition(i);
        }
    }
    
    /// <summary>Converts the index of the cell to the 3D point (y is zero) relative to grid center</summary>
    /// <returns>Vector3 of the position of the center of the cell relative to grid center</returns>
    /// <param name="cellIndex">The index of the cell</param>
    public Vector3 GetCellLocalPosition(int cellIndex)
    {
        float x = (cellIndex / m_GridSize.z - m_CellCenterOffset.x) * m_CellScale.x;
        float z = (cellIndex % m_GridSize.z - m_CellCenterOffset.z) * m_CellScale.z;
        return new Vector3(x, 0, z);
    }
    
    internal Vector3 GetCellGlobalPosition(int cellIndex)
    {
        return m_CellLocalPositions[cellIndex] + m_CenterObject.position;
    }
    
    /// <summary>
    /// Perceive the latest grid status. Call OverlapBoxNonAlloc once to detect colliders.
    /// Then parse the collider arrays according to all available gridSensor delegates.
    /// </summary>
    internal void Update()
    {
        for (var cellIndex = 0; cellIndex < m_NumCells; cellIndex++)
        {
            var cellCenter = GetCellGlobalPosition(cellIndex);
            var numFound = BufferResizingOverlapBoxNonAlloc(cellCenter, m_HalfCellScale);

            if (GridOverlapDetectedAll != null)
            {
                ParseCollidersAll(m_ColliderBuffer, numFound, cellIndex, cellCenter, GridOverlapDetectedAll);
            }
            if (GridOverlapDetectedClosest != null)
            {
                ParseCollidersClosest(m_ColliderBuffer, numFound, cellIndex, cellCenter, GridOverlapDetectedClosest);
            }
        }
    }
    /// <summary>
    /// Same as Update(), but only load data for debug gizmo.
    /// </summary>
    internal void UpdateGizmo()
    {
        for (var cellIndex = 0; cellIndex < m_NumCells; cellIndex++)
        {
            var cellCenter = GetCellGlobalPosition(cellIndex);
            var numFound = BufferResizingOverlapBoxNonAlloc(cellCenter, m_HalfCellScale);
            //Debug.Log($"num found {numFound}, cell index {cellIndex}, cell center {cellCenter}");

            ParseCollidersClosest(m_ColliderBuffer, numFound, cellIndex, cellCenter, GridOverlapDetectedDebug);
        }

    }
    
    
    
        int BufferResizingOverlapBoxNonAlloc(Vector3 cellCenter, Vector3 halfCellScale)
        {
            int numFound;
            // Since we can only get a fixed number of results, requery
            // until we're sure we can hold them all (or until we hit the max size).
            while (true)
            {
                numFound = Physics.OverlapBoxNonAlloc(cellCenter, halfCellScale, m_ColliderBuffer, Quaternion.identity, m_ColliderMask);
                if (numFound == m_ColliderBuffer.Length && m_ColliderBuffer.Length < m_MaxColliderBufferSize)
                {
                    m_ColliderBuffer = new Collider[Math.Min(m_MaxColliderBufferSize, m_ColliderBuffer.Length * 2)];
                    m_InitialColliderBufferSize = m_ColliderBuffer.Length;
                }
                else
                {
                    break;
                }
            }
            return numFound;
        }

        /// <summary>
        /// Parses the array of colliders found within a cell. Finds the closest gameobject to the agent root reference within the cell
        /// </summary>
        void ParseCollidersClosest(Collider[] foundColliders, int numFound, int cellIndex, Vector3 cellCenter, Action<GameObject, int> detectedAction)
        {
            GameObject closestColliderGo = null;
            var minDistanceSquared = float.MaxValue;

            for (var i = 0; i < numFound; i++)
            {
                var currentColliderGo = foundColliders[i].gameObject;
                
                var closestColliderPoint = foundColliders[i].ClosestPointOnBounds(cellCenter);
                var currentDistanceSquared = (closestColliderPoint - m_CenterObject.position).sqrMagnitude;

                if (currentDistanceSquared >= minDistanceSquared)
                {
                    continue;
                }

                // Checks if our colliders contain a detectable object
                var index = -1;
                for (var ii = 0; ii < m_DetectableTags.Length; ii++)
                {
                    if (currentColliderGo.CompareTag(m_DetectableTags[ii]))
                    {
                        //Debug.Log(currentColliderGo.tag);
                        index = ii;
                        break;
                    }
                }
                if (index > -1 && currentDistanceSquared < minDistanceSquared)
                {
                    minDistanceSquared = currentDistanceSquared;
                    closestColliderGo = currentColliderGo;
                }
            }

            if (!ReferenceEquals(closestColliderGo, null))
            {
                detectedAction.Invoke(closestColliderGo, cellIndex);
            }
        }

        /// <summary>
        /// Parses all colliders in the array of colliders found within a cell.
        /// </summary>
        void ParseCollidersAll(Collider[] foundColliders, int numFound, int cellIndex, Vector3 cellCenter, Action<GameObject, int> detectedAction)
        {
            for (int i = 0; i < numFound; i++)
            {
                var currentColliderGo = foundColliders[i].gameObject;
             
                detectedAction.Invoke(currentColliderGo, cellIndex);
                
            }
        }
        
        internal void RegisterSensor(CustomGridSensor sensor)
        {

            if (sensor.GetProcessCollidersMethod() == ProcessCollidersMethod.ProcessAllColliders)
            {
                GridOverlapDetectedAll += sensor.ProcessDetectedObject;
            }
            else
            {
                GridOverlapDetectedClosest += sensor.ProcessDetectedObject;
            }
        }

        internal void RegisterDebugSensor(CustomGridSensor debugSensor)
        {

            GridOverlapDetectedDebug += debugSensor.ProcessDetectedObject;

        }
}


