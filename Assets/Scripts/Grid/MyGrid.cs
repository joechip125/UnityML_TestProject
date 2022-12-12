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
[System.Flags]
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
    
    private Vector3 GetCellGlobalPosition(int cellIndex) 
    {
        return _mCellLocalPositions[cellIndex] + _gridAdjustment;
    }

    private void RollBackGrid()
    {
        if (adjustVvectors.Count > 0)
        {
            _gridAdjustment -= adjustVvectors.Pop();
        }
    }
    
    private void AdjustGridMemory(int index, AdjustActions actions, float division)
    {
        cellScale = new Vector3((gridSize.x / division) * minCellScale.x, 1, (gridSize.z / division) * minCellScale.z);
        
        if (actions.HasFlag(AdjustActions.Reset))
        {
            _gridAdjustment = transform.position;
        }
        else if (actions.HasFlag(AdjustActions.Rollback))
        {
            RollBackGrid();
        }
        
        else if (actions.HasFlag(AdjustActions.Add))
        {
           var add = Vector3.Scale(cellScale, adjustVecs[index]* 1f);
           _gridAdjustment += add;
           adjustVvectors.Push(add);
        }
    
        _mCellLocalPositions = new Vector3[_numCells];

        for (int i = 0; i < _numCells; i++)
        {
            _mCellLocalPositions[i] = GetCellLocalPosition(i);
        }
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

   

    private void ScanSomething(int divisions, int index)
    {
        ScanCells(divisions);
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

  

    private int ScanCell(int index, out Color hitColor)
    {
        var pos = GetCellGlobalPosition(index);
        var _mColliderBuffer = new Collider[4];
        hitColor = new Color(255, 255, 255, 255 * 0.5f);
        
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
                    hitColor = Color.green;
                }
            }
        }
        
        return hitTags;
    }

    
    private void AddWithMem(int index, int startDiv)
    {
        AdjustGridMemory(index, AdjustActions.Add, startDiv);
        for (int i = 0; i < 4; i++)
        {
            AdjustGridMemory(i, AdjustActions.Rollback, startDiv);
            AdjustGridMemory(i, AdjustActions.Add, startDiv);
            
            if (ScanCells(startDiv))
            {
                for (int j = 0; j < 4; j++)
                {
                    if(positions[j].z == 0)
                        Gizmos.DrawCube(GetCellGlobalPosition(j), cellScale);
                }
            }
        }
    }

    private bool ScanCells(int divisions)
    {
        var hitResult = false;
        positions.Clear();
        
        for (int i = 0; i < _numCells; i++)
        {
            if (ScanCell(i, out var color) > 0)
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

    private void ScanFirst()
    {
        for (int i = 0; i < _numCells; i++)
        {
            var hits = ScanCell(i, out var color);
            //Gizmos.color = color;
            
            if(hits < 1)
                Gizmos.DrawCube(GetCellGlobalPosition(i), cellScale);
        }
    }
    
    void OnDrawGizmos()
    {
        //if (!Application.IsPlaying(gameObject)) return;
       
        AdjustGridMemory(0, AdjustActions.Reset, 1);
        ScanFirst();
        
        AdjustGridMemory(0, AdjustActions.Reset, 2);
        AddWithMem(0, 2);

        for (int i = 0; i < 4; i++)
        {
            AdjustGridMemory(i, AdjustActions.Reset, 2);
            AdjustGridMemory(i, AdjustActions.Add, 2);
            AddWithMem(i, 4);
        }

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                AdjustGridMemory(j, AdjustActions.Reset, 2);
                AdjustGridMemory(i, AdjustActions.Add, 2);
                AdjustGridMemory(j, AdjustActions.Add, 4);
                AddWithMem(j, 8);
            }
        }
        //for (int i = 0; i < 4; i++)
        //{
        //    AdjustGridMemory(i, AdjustActions.Reset, 2);
        //    AdjustGridMemory(0, AdjustActions.Add, 2);
        //    AdjustGridMemory(i, AdjustActions.Add, 4);
        //    AddWithMem(i, 8);
        //}
        
        //AdjustGridMemory(0, AdjustActions.Reset, 2);
        //AdjustGridMemory(0, AdjustActions.Add, 2);
        //AdjustGridMemory(0, AdjustActions.Add, 4);
        //AddWithMem(0, 8);
    }
}
