using System;
using System.Collections;
using System.Collections.Generic;
using MBaske.Sensors.Grid;
using UnityEngine;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;

public enum CellContents
{
    None,
    Collector,
    Collectable,
    Poison
}

public enum AdjustActions
{
    None,
    Add,
    Rollback,
    Reset,
}


[Serializable]
public class CellInfo
{
    public Vector3 scale;
    public Vector3 location;
    public Vector3 coordinates;
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
    private List<Vector3>includePositions = new();
    private List<Vector3> nextPositions = new();
    
    private List<Vector3> positions = new();

    private Vector3Int currentGridSize = new Vector3Int(2, 1, 2);
    private int currentDivide = 2;
    private int _numCells = 4;
    private Vector3 _gridAdjustment;
    private Vector3[][] posArray;
    private Vector3[] adjustVecs ={new (-1,0, -1), new (1,0, -1), new (-1,0, 1), new(1,0,1)};
    private List<int> extenders = new();
    private List<CellInfo> _cellInfos = new();
    LinkedList<int>[] linkedListArray;

    private List<int> _excludeIndex;

    private List<int> adjustIndexes = new();
    
    private Stack<Vector3> adjustVvectors = new();

    private int[][] scanThese;

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


    private void AdjustGrid(int index, float depth, bool reset = false)
    {
        if (reset)
        {
            _gridAdjustment = transform.position;
        }
        
        _gridAdjustment += Vector3.Scale(cellScale, adjustVecs[index]* depth);
    }
    
    private void AdjustGridMemory(int index, float depth, AdjustActions actions, float division)
    {
        cellScale = new Vector3((gridSize.x / division) * minCellScale.x, 1, (gridSize.z / division) * minCellScale.z);
        
        if (actions.HasFlag(AdjustActions.Reset))
        {
            _gridAdjustment = transform.position;
        }
        else if (actions.HasFlag(AdjustActions.Rollback))
        {
            if (adjustVvectors.Count > 0)
            {
                _gridAdjustment -= adjustVvectors.Pop();
            }
        }
        
        else if (actions.HasFlag(AdjustActions.Add))
        {
           var add = Vector3.Scale(cellScale, adjustVecs[index]* depth);
           _gridAdjustment += add;
           
        }
    
        InitCellLocalPositions(division, index);
    }
    
    private void AdjustGrid()
    {
        var addVector = new Vector3();
        var depth = 2;
        _gridAdjustment = transform.position;
        foreach (var a in adjustIndexes)
        {
            _gridAdjustment += Vector3.Scale(cellScale, adjustVecs[a]* depth);
            depth *= 2;
        }
    }

    private void ScanQuadrant(int startIndex)
    {
        ScanCells(2, startIndex);
        List<Vector2> dontScan = new();

        var keepGoing = true;

        while (keepGoing)
        {
            for (int i = 0; i < positions.Count; i++)
            {
                if (positions[i].z != 0)
                {
                    
                }
                else
                {
                    dontScan.Add(new Vector2());
                    Gizmos.DrawCube(GetCellGlobalPosition(i), cellScale);
                }
            }
            keepGoing = false;
        }
    }

    private void Depth(int v)
    {
        linkedListArray = new LinkedList<int>[v];
        var allFound = false;
        var index = 0;
        var forom = Mathf.Pow(4,2) + index;
        
        var depth = 1;
        var sum = index * 64 + 2;
        while (!allFound)
        {
            for (int i = 0; i < _numCells; i++)
            {
                var amount = ScanCell(i);
                AddEdge(depth,i + 2);
            }

            allFound = true;
        }
        DFS();
    }

    internal void DFS()
    {
        Debug.Log("DFS");
        //Console.WriteLine("DFS");
        bool[] visited = new bool[linkedListArray.Length + 1];
        DFSHelper(1, visited);

    }

    internal void DFSHelper(int src, bool[] visited)
    {
        visited[src] = true;
        //Console.Write(src + "->");
        Debug.Log(src + "->");
        if (linkedListArray[src] != null)
        {
            foreach (var item in linkedListArray[src])
            {
                if (!visited[item] == true)
                {
                    DFSHelper(item, visited);
                }
            }
        }
    }

    public void AddEdge(int u, int v, bool blnBiDir = false)
    {
        if (linkedListArray[u] == null)
        {
            linkedListArray[u] = new LinkedList<int>();
            linkedListArray[u].AddFirst(v);
        }
        else
        {
            var last = linkedListArray[u].Last;
            linkedListArray[u].AddAfter(last, v);
        }

        if (blnBiDir)
        {
            if (linkedListArray[v] == null)
            {
                linkedListArray[v] = new LinkedList<int>();
                linkedListArray[v].AddFirst(u);
            }
            else
            {
                var last = linkedListArray[v].Last;
                linkedListArray[v].AddAfter(last, u);
            }
        }
    }

    private void ScanAll()
    {
        includePositions.Clear();
        positions.Clear();
        _cellInfos.Clear();
        var keep = true;
        var division = 1;
        InitCellLocalPositions(1, 0);
        
        int iterations = 0;
        var set = new Vector3();
        while (keep)
        {
            if (iterations > 0 && positions.Count < 1) break;
            
            for (int i = 0; i < _numCells; i++)
            {
                if (positions[i].x > 0)
                {
                    
                }
            }
            
            division *= 2;
            ScanCells(division, 3);
            
            iterations++;
            keep = false;
            
        }
    }

    private void ScanSomething(int divisions, int index)
    {
        ScanCells(divisions, index);
        for (int i = 0; i < positions.Count; i++)
        {
            if (positions[i].z == 0)
            {
                Gizmos.DrawCube(GetCellGlobalPosition(i), cellScale);
                _excludeIndex.Add(index);
            }
            else
            {
            
            }
        }
    }

    private void ScanMany(int divisions)
    {
        
        for (int i = 0; i < 4; i++)
        {
            if (!ScanCells(divisions, i))
            {
                
            }
        }
    }

    private bool ScanCells(int divisions, int index)
    {
        InitCellLocalPositions(divisions, index);
        var hitResult = false;
        positions.Clear();
        
        for (int i = 0; i < _numCells; i++)
        {
            if (ScanCell(i) > 0)
            {
                positions.Add(new Vector3(i,divisions,1));
                
                hitResult = true;
            }
            else
            {
                positions.Add(new Vector3(i,divisions,0));
            }
        }
        
        return hitResult;
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

    private void UntilEnd(int index)
    {
        InitCellLocalPositions(2, index);
        AdjustGrid(index, 0.5f);
        var hits = ScanCell(index);
        var div = 4;

        while (hits < 0)
        {
            InitCellLocalPositions(div, index);
            AdjustGrid(index, 0.5f);
            hits = ScanCell(index);
            
        }
    }
    
    void OnDrawGizmos()
    {
        //if (!Application.IsPlaying(gameObject)) return;
        var division = 1f;
        _gridAdjustment = transform.position;
        var keep = true;
        
        ScanSomething(1,1);
        AdjustGrid(0, 0.5f);
     
        ScanSomething(2,0);
        AdjustGrid(0, 0.5f);
        ScanSomething(4,0);
        AdjustGrid(0, 0.5f);
        ScanSomething(8,0);
        ////ScanSomething(2,3);
        //ScanSomething(4,3);
        //ScanSomething(8,3);
        //
    }
}
