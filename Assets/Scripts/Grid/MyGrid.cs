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
    private Vector3 m_CellCenterOffset = new Vector3(0.5f, 0, 0.5f);
    private Vector3[] _mCellLocalPositions;
    
    Collider[] _mColliderBuffer;
    private List<Vector2Int> includeCells = new();
    private List<Vector4> includePositions = new();
    private List<Vector4> nextPositions = new();
    
    private List<Vector2> positions = new();

    private Vector3Int currentGridSize = new Vector3Int(2, 1, 2);
    private int currentDivide = 2;
    private int _numCells = 4;
    private Vector3 _gridAdjustment;

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
        
        for (int i = 0; i < _mCellLocalPositions.Length; i++)
        {
            
        }
    }

    void InitCellLocalPositions(float division)
    {
        cellScale = new Vector3((gridSize.x / division) * minCellScale.x, 1, (gridSize.z / division) * minCellScale.z);
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

    private void SetNumberCells()
    {
        _numCells = 4;
        
        
    }

    private Vector3 GetCellGlobalPosition(int cellIndex) 
    {
        return _mCellLocalPositions[cellIndex] + _gridAdjustment;
    }

 

    private bool ScanCells(int divisions)
    {
        InitCellLocalPositions(divisions);
        includePositions.Clear();
        positions.Clear();
        var hitResult = false;
        
        for (int i = 0; i < _numCells; i++)
        {
            var pos = GetCellGlobalPosition(i);
            if (ScanCell(i) > 0)
            {
                positions.Add(new Vector2(i,1));
                nextPositions.Add(new Vector4(pos.x, pos.z, cellScale.x, cellScale.z));
                hitResult = true;
            }
            else
            {
                positions.Add(new Vector2(i,0));
                includePositions.Add(new Vector4(pos.x, pos.z, cellScale.x, cellScale.z));
            }
        }

        return hitResult;
    }
    
    void OnDrawGizmos()
    {
        //if (!Application.IsPlaying(gameObject)) return;
        var division = 1;
        InitCellLocalPositions(division);
        _gridAdjustment = transform.position;
        
        nextPositions.Clear();
        var keep = ScanCells((int)Mathf.Pow(2, division));
        

        var set = new Vector3();

        foreach (var p in positions)
        {
            var pos = GetCellGlobalPosition((int)p.x);
            if (p.y == 0)
            {
                Gizmos.DrawCube(pos, cellScale);
            }
            else
            {
                set = (pos - transform.position);
            }
        }

        if (!keep) return;
        division = 2;
        _gridAdjustment = transform.position + set;
        InitCellLocalPositions((int)Mathf.Pow(2, division));
        ScanCells((int)Mathf.Pow(2, division));
        //if (!ScanCells(4)) return;


        foreach (var p in positions)
        {
            var pos = GetCellGlobalPosition((int)p.x);
            if (p.y == 0)
            {
                //Gizmos.DrawCube(pos, cellScale);
                //Gizmos.color = Color.magenta;
            }
            else if(p.y == 1)
            {
                Gizmos.DrawCube(pos, cellScale);
                Gizmos.color = Color.magenta;
            }
        }

    }
}
