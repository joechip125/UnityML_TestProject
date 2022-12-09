using System;
using System.Collections;
using System.Collections.Generic;
using MBaske.Sensors.Grid;
using UnityEngine;
using Unity.MLAgents.Sensors;

public enum CellContents
{
    None,
    Collector,
    Collectable,
    Poison
}


[Serializable]
public class CellInfo
{
    public IntVector2 index;
    public Vector3 location;
    public CellContents contents;
}
[ExecuteInEditMode]
public class MyGrid : MonoBehaviour
{
    
    public const int NumChannels = 4;
    // Buffer channels.
    public const int Collectable = 1;
    public const int Poison = 2;
    public const int Visit = 3;
    public const int Wall = 4;
    
    private bool m_ShowGizmos = false;
    [SerializeField] public Vector3 minCellScale = new Vector3(1f, 0.01f, 1f);
    private Vector3 cellScale = new Vector3(1f, 0.01f, 1f);
    
    [SerializeField] private Vector3Int gridSize = new Vector3Int(12, 1, 12);
    [HideInInspector, SerializeField]
    internal float m_GizmoYOffset = 0f;

    public List<ChannelLabel> labels;

    private GridBuffer _gridBuffer;
    private Vector3 m_CellCenterOffset;
    private Vector3[] _mCellLocalPositions;
    
    Collider[] _mColliderBuffer;
    private List<Vector2Int> includeCells = new();
    private List<Vector4> includePositions = new();

    private Vector3Int currentGridSize;
    private int currentDivide = 2;
    private int _numCells = 2;

    // Start is called before the first frame update
    void Start()
    {
       
    }

    private void Awake()
    {
        m_ShowGizmos = true;
    }

    // Update is called once per frame

    void Update()
    {
        
    }

    private void Something()
    {
        
    }

    void InitCellLocalPositions()
    {
        _mCellLocalPositions = new Vector3[_numCells];

        for (int i = 0; i < _numCells; i++)
        {
            _mCellLocalPositions[i] = GetCellLocalPosition(i);
        }
    }

    private Vector3 GetCellLocalPosition(int cellIndex)
    {
        float z = (cellIndex / currentGridSize.z - m_CellCenterOffset.x) * cellScale.z;
        float x = (cellIndex % currentGridSize.z - m_CellCenterOffset.z) * cellScale.x;
        return new Vector3(x, 0, z);
    }


    private Vector3 GetCellGlobalPosition(int cellIndex)
    {
        return _mCellLocalPositions[cellIndex] + transform.position;
    }

    private Color ScanCell(int indexX, int indexZ)
    {
        cellScale = new Vector3(minCellScale.x * 6 , 1, minCellScale.z * 6);
        var pos = transform.position + 
                  new Vector3(indexX * cellScale.x, 0, indexZ * cellScale.z);
        var _mColliderBuffer = new Collider[4];
        
        var numFound = Physics.OverlapBoxNonAlloc(pos, cellScale / 2, _mColliderBuffer, Quaternion.identity);


        for (int i = 0; i < numFound; i++)
        {
            var current = _mColliderBuffer[i].gameObject;
            
            for (int j = 0; j < labels.Count; j++)
            {
                if (current.CompareTag(labels[j].Name))
                {
                    return labels[j].Color;
                }
            }
        }
        
        return Color.white;
    }
    
    private int ScanCell(int index)
    {
        var pos = GetCellGlobalPosition(index);
        var _mColliderBuffer = new Collider[4];
        
        
        var numFound = Physics.OverlapBoxNonAlloc(pos, cellScale / 2, _mColliderBuffer, Quaternion.identity);
        var hitTags = 0;

        for (int i = 0; i < numFound; i++)
        {
            var current = _mColliderBuffer[i].gameObject;
            
            for (int j = 0; j < labels.Count; j++)
            {
                if (current.CompareTag(labels[j].Name))
                {
                    hitTags++;
                }
            }
        }
        
        return hitTags;
    }

    public void DivideAnd()
    {
        InitCellLocalPositions();

        for (int i = 0; i < _mCellLocalPositions.Length; i++)
        {
            
        }
    }

    private void SetCellPositions(int division)
    {
        var somethiong = 0;
        currentDivide = 2;
        _numCells = division * 2;
        currentGridSize = new Vector3Int(division, 1, division);
        m_CellCenterOffset = new Vector3((division - 1f) / 2, 0, (division - 1f) / 2);
        cellScale = new Vector3((gridSize.x / division) * minCellScale.x, 1, (gridSize.z / division) * minCellScale.z);
        InitCellLocalPositions();
    }
    
    private void GetCellTransform(int division, int index)
    {
        currentDivide = 2;
        _numCells = division * 2;
        m_CellCenterOffset = new Vector3((division - 1f) / 2, 0, (division - 1f) / 2);
        cellScale = new Vector3((gridSize.x / division) * minCellScale.x, 1, (gridSize.z / division) * minCellScale.z);
        InitCellLocalPositions();
    }
    
    void OnDrawGizmos()
    {
        //if (!Application.IsPlaying(gameObject)) return;
        SetCellPositions(2);

        for (int i = 0; i < _numCells; i++)
        {
            var pos = GetCellGlobalPosition(i);
            if(ScanCell(i) < 1)
                Gizmos.DrawCube(pos, cellScale);
            else
            {
                var adjust = pos - transform.position;
                
                includePositions.Add(new Vector4(pos.x, pos.z, cellScale.x, cellScale.z));
            }
        }
    }
}
