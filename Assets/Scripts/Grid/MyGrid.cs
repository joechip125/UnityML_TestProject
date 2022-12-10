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
    private List<Vector3> includePositions = new();
    private List<Vector3> nextPositions = new();
    
    private List<Vector2> positions = new();

    private Vector3Int currentGridSize = new Vector3Int(2, 1, 2);
    private int currentDivide = 2;
    private int _numCells = 4;
    private Vector3 _gridAdjustment;
    private Vector3[][] posArray;
    private Vector3[] adjustVecs ={new (-1,0, -1), new (1,0, -1), new (-1,0, 1), new(1,0,1)};
    private List<int> extenders = new();

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

    void InitCellLocalPositions(float division, int index)
    {
        cellScale = new Vector3((gridSize.x / division) * minCellScale.x, 1, (gridSize.z / division) * minCellScale.z);
        _mCellLocalPositions = new Vector3[_numCells];

        for (int i = 0; i < _numCells; i++)
        {
            _mCellLocalPositions[i] = GetCellLocalPosition(i);
        }

        if(division > 1)
            AdjustGrid(index, division -1);
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
        //InitCellLocalPositions(divisions);
        var hitResult = false;
        positions.Clear();
        includePositions.Clear();
        
        for (int i = 0; i < _numCells; i++)
        {
            includePositions.Add(GetCellGlobalPosition(i));
            if (ScanCell(i) > 0)
            {
                positions.Add(new Vector2(i,1));
                
                hitResult = true;
                extenders.Add(i);
            }
            else
            {
                positions.Add(new Vector2(i,0));
            }
        }

        return hitResult;
    }

    private void AdjustGrid(int index, float depth)
    {
        _gridAdjustment = transform.position + Vector3.Scale(cellScale, adjustVecs[index]* depth);
    }
    
    private void ScanAll()
    {
        includePositions.Clear();
        positions.Clear();
        var keep = true;
        var division = 1;
        keep = ScanCells((int)Mathf.Pow(2, division));
        int iterations = 0;
        
        var set = new Vector3();
        while (keep)
        {
            ScanCells((int)Mathf.Pow(2, division));
            if (iterations > 0 && positions.Count < 1) break;

            foreach (var p in positions)
            {
                if (p.x > 0)
                {
                    
                }
                else
                {
                    
                }
            }
            
            for (int i = 0; i < _numCells; i++)
            {
                if (positions[i].x > 0)
                {
                    
                }
                
            }

            iterations++;
            keep = false;
        }
    }
    
    void OnDrawGizmos()
    {
        //if (!Application.IsPlaying(gameObject)) return;
        var division = 1;
        _gridAdjustment = transform.position;
        
        var keep = true;
        
        //var set = new Vector3();
        //while (keep)
        //{
        //    keep = ScanCells((int)Mathf.Pow(2, division));
        //    
        //    
        //
        //    keep = false;
        //}
        var index = 0;
        var divider = 1;
        //InitCellLocalPositions(5);
        
        InitCellLocalPositions(1, 0);
        Gizmos.color = new Color(255, 255, 255, 255 * 0.5f);
        
        for (int i = 0; i < _numCells; i++)
        {
            if (i != 3)
            {
                Gizmos.DrawCube(GetCellGlobalPosition(i), cellScale);
            }
        }
        
        InitCellLocalPositions(2, 3);
        
        for (int i = 0; i < _numCells; i++)
        {
            if(i != 3)
                Gizmos.DrawCube(GetCellGlobalPosition(i), cellScale);
        }
        
        InitCellLocalPositions(4, 3);
        
        for (int i = 0; i < _numCells; i++)
        {
            if(i != 3)
                Gizmos.DrawCube(GetCellGlobalPosition(i), cellScale);
        }

        //foreach (var n in nextPositions)
        //{
        //    _gridAdjustment = n;
        //    ScanCells((int)Mathf.Pow(2, 2));
        //    for (int i = 0; i < _numCells; i++)
        //    {
        //        var pos = includePositions[i];
        //        var choice = positions[i];
        //    
        //        if (choice.y > 0)
        //        {
        //            //nextPositions.Add(pos - transform.position);
        //        }
        //        else
        //        {
        //            Gizmos.DrawCube(pos, cellScale);
        //        }
        //    }
        //}
    }
}
