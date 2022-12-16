using System;
using System.Collections;
using System.Collections.Generic;
using MBaske.Sensors.Grid;
using UnityEngine;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using Random = UnityEngine.Random;

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
    private bool m_ShowGizmos = false;
    [SerializeField] public Vector3 minCellScale = new Vector3(1f, 1, 1f);
    private Vector3 cellScale = new Vector3(1f, 0.01f, 1f);
    
    [SerializeField] private Vector3Int gridSize = new Vector3Int(12, 1, 12);
    
    [SerializeField] List<ChannelLabel> labels;

    private GridBuffer _gridBuffer;
    private Vector3 m_CellCenterOffset = new Vector3(0.5f, 0, 0.5f);
    private Vector3[] _mCellLocalPositions;
    
    Collider[] _mColliderBuffer;
    private Vector4[] _targets;
    private List<Vector4> _targetList = new();
    private List<Vector2Int> includeCells = new();
    private List<Vector3>includePositions = new();
    private List<Vector3> nextPositions = new();
    
    private List<Vector3> positions = new();

    private Vector3Int minorGridSize = new Vector3Int(2, 1, 2);
    private int currentDivide = 2;
    private int _numCells = 4;
    private Vector3 _gridAdjustment;
    private Vector3[][] posArray;
    private readonly Vector3Int[] _adjustVectors ={new (-1,0, -1), new (1,0, -1), new (-1,0, 1), new(1,0,1)};
    private List<int> extenders = new();
    private List<CellInfo> _cellInfos = new();
    LinkedList<int>[] linkedListArray;

    private List<int> _excludeIndex;

    private List<int> adjustIndexes = new();
    
    private Stack<Vector3> adjustVvectors = new();
    private int[] GridIndexes;

    private List<Vector4> _cubers = new();
    private Vector3Int _minorMin;
    private Vector3Int _minorMax;
    private int _smallGridSize;
    
    private void Awake()
    {
        m_ShowGizmos = true;
    }
    
    void InitCellLocalPositions()
    {
        m_CellCenterOffset = new Vector3((gridSize.x - 1f) / 2, 0, (gridSize.z - 1f) / 2);
        _numCells = gridSize.x * gridSize.z;
        _gridAdjustment = transform.position;
        minorGridSize = gridSize;
        cellScale = minCellScale;
        _mCellLocalPositions = new Vector3[_numCells];

        for (int i = 0; i < _numCells; i++)
        {
            _mCellLocalPositions[i] = GetCellLocalPosition(i);
        }
    }
    
    void InitCellLocalPositions2()
    {
        m_CellCenterOffset = new Vector3((gridSize.x - 1f) / 2, 0, (gridSize.z - 1f) / 2);
        _numCells = gridSize.x * gridSize.z;
        _gridAdjustment = transform.position;
        minorGridSize = gridSize;
        cellScale = minCellScale;
        _mCellLocalPositions = new Vector3[_numCells];
        var index = new Vector3Int();
        var xCount = 0;
        var zCount = 0;
        
        for (int i = 0; i < _numCells; i++)
        {
            var amount = (zCount * gridSize.x) + xCount++;
            _mCellLocalPositions[i] = GetCellLocalPosition(amount);
            if (xCount > gridSize.x)
            {
                xCount = 1;
                zCount++;
            }
        }
    }
    
    private Vector3 GetCellLocalPosition(int cellIndex)
    {
        float z = (cellIndex / minorGridSize.z - m_CellCenterOffset.x) * cellScale.z;
        float x = (cellIndex % minorGridSize.z - m_CellCenterOffset.z) * cellScale.x;
        return new Vector3(x, 0, z);
    }
    
    private Vector3 GetCellGlobalPosition(int cellIndex) 
    {
        return _mCellLocalPositions[cellIndex] + _gridAdjustment;
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

    private void StackGrids()
    {
        _cubers.Clear();
        _minorMin = new Vector3Int(0, 0,0);
        minorGridSize = gridSize;
        _smallGridSize = gridSize.x;
        List<Color> colors = new List<Color>()
        {
            Color.green,
            Color.blue,
            Color.red,
            Color.yellow,
            Color.magenta,
            Color.cyan
        };
        
        ChangeGridShape(1,1);
        ChangeGridShape(1,0);
        ChangeGridShape(1,0);
        ChangeGridShape(1,1);
        ChangeGridShape(1,1);
        
       for (int i = 0; i < _cubers.Count; i++)
       {
           var size = new Vector3(_cubers[i].x, 1, _cubers[i].y);
           var pos = new Vector3(_cubers[i].z, 1, _cubers[i].w);
           Gizmos.color = colors[i];
           Gizmos.DrawCube(pos, size);
       }
    
    }
    
    private void ChangeGridShape(int stepX, int stepZ, bool moveMin = true)
    {
        if (_smallGridSize % 2 != 0)
        {
            _smallGridSize -= 1;
            _minorMin += new Vector3Int(stepX, 0, stepZ);
        }
        else
        {
            _smallGridSize /= 2;
            _minorMin += new Vector3Int(stepX *  _smallGridSize, 0, stepZ * _smallGridSize);
        }
        
        var maxX = Mathf.Clamp(_minorMin.x + _smallGridSize - 1, 0, gridSize.x - 1);
        var maxZ = Mathf.Clamp(_minorMin.z + _smallGridSize -1, 0, gridSize.z - 1);
        var newMax = new Vector3Int(maxX, 0, maxZ);
        
        GetGridSize(_minorMin, newMax);    
    }
 
    private void GetGridSize(Vector3Int minIndex, Vector3Int maxIndex)
    {
        var min = minIndex.z * gridSize.x + minIndex.x;
        var max = maxIndex.z * gridSize.x + maxIndex.x;
        
        var minVec = GetCellGlobalPosition(min) - minCellScale / 2;
        var maxVec = GetCellGlobalPosition(max) + minCellScale / 2;
        var theSize = new Vector3(maxVec.x - minVec.x, 1, maxVec.z - minVec.z);
       
        var  theCenter = minVec + theSize / 2;

        _cubers.Add(new Vector4(theSize.x, theSize.z, theCenter.x, theCenter.z));
    }
    
    private void Get1DIndexes(Vector2Int index, int size)
    {
        var cosAngle = 1;
        var sinAngle = 1;
        GridIndexes = new int[8];
        var start = index - new Vector2Int(size, size);
        var end = index + new Vector2Int(size, size);
        var numX = end.x - start.x;
        var numZ = end.y - start.y;
        
        for (int i = 0; i < 4; i++)
        {
            var aSin = Mathf.Sin(Mathf.Deg2Rad * sinAngle) * size;
            var aCos = Mathf.Cos(Mathf.Deg2Rad * cosAngle) * size;
            var newIndex = index + new Vector2Int(Mathf.RoundToInt(aCos), Mathf.RoundToInt(aSin));
            cosAngle += 90;
            sinAngle += 90;
            if (newIndex.x < 0 || newIndex.x > gridSize.x -1) continue;
            
            var anIndex = newIndex.y * gridSize.x + newIndex.x;
            Gizmos.color = new Color(Random.Range(0,1), Random.Range(0,1), 0, 0.5f);
            if (anIndex > 0 && anIndex < _numCells)
            {
                Gizmos.DrawCube(GetCellGlobalPosition(anIndex), minCellScale);
            }
        }
    }
        
    private void GetSmallGrid(int size, Vector2Int index)
    {
        InitCellLocalPositions();
        var start = index - new Vector2Int(size, size);
        var end = index + new Vector2Int(size + 1, size + 1);
        var numX = end.x - start.x;
        var numZ = end.y - start.y;
        var xCount = start.x;
        var zCount = start.y;

        for (int z = 0; z < numZ; z++)
        {
            for (int x = 0; x < numX; x++)
            {
                if (xCount < 0 || xCount >= gridSize.x)
                {
                    xCount++;
                    continue;
                }

                if (zCount < 0 || zCount >= gridSize.z)
                {
                    break;
                }
                Gizmos.color = new Color(255, 0, 0, 0.5f);
                var start2 = zCount * gridSize.x + xCount;
                Gizmos.DrawCube(GetCellGlobalPosition(start2), minCellScale);
                
                xCount++;
            }
            xCount = start.x;
            zCount++;
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
    
    private int ScanCell(out Color hitColor, Vector2Int theIndex)
    {
        var index = theIndex.y * gridSize.x + theIndex.x;
        var pos = GetCellGlobalPosition(index);
        _mColliderBuffer = new Collider[4];
        hitColor = new Color(255, 255, 255, 255 * 0.5f);

        var numFound = Physics.OverlapBoxNonAlloc(pos, cellScale / 4, _mColliderBuffer, Quaternion.identity);
        var hitTags = 0;

        for (int i = 0; i < numFound; i++)
        {
            var current = _mColliderBuffer[i].gameObject;
            
            for (int j = 0; j < labels.Count; j++)
            {
                if (current.CompareTag(labels[j].Name))
                {
                    GetSmallGrid(1, theIndex);
                    hitTags++;
                    hitColor = Color.green;
                }
            }
        }
        
        return hitTags;
    }

    private void GatherColliders()
    {
        _mColliderBuffer = new Collider[50];
        _targetList.Clear();
        _targets = new Vector4[50];
        var pos = transform.position;
        var size = Vector3.Scale(cellScale, gridSize);
        var numFound = Physics.OverlapBoxNonAlloc(pos, 
            size / 2, _mColliderBuffer, Quaternion.identity);
        
        for (int i = 0; i < numFound; i++)
        {
            var current = _mColliderBuffer[i].gameObject;
            
            for (int j = 0; j < labels.Count; j++)
            {
                if (current.CompareTag(labels[j].Name))
                {
                    var loc = current.transform.position;
                }
            }
        }
    }
    
    void OnDrawGizmos()
    {
        InitCellLocalPositions2();
        StackGrids();
        
        for (int z = 0; z < gridSize.z; z++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                ScanCell( out var color, new Vector2Int(x,z));    
            }
        }
        
        for (int i = 0; i < _numCells; i++)
        {
            Gizmos.color = new Color(255, 255, 255, 0.5f);
            Gizmos.DrawCube(GetCellGlobalPosition(i), minCellScale);
        }
    }
}
