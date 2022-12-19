using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DefaultNamespace.Grid;
using MBaske.Sensors.Grid;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class TensorVis : MonoBehaviour
{
    [SerializeField]private TensorTile tensorTileProto;
    
    [SerializeField] private Vector3Int gridSize= new Vector3Int(12, 1, 12);
    [SerializeField]private Vector3 cellScale = new(1f, 1, 1f);
    
    [SerializeField] List<ChannelLabel> labels;
    
    private Vector3 _cellCenterOffset = new Vector3(0.5f, 0, 0.5f);
    private Vector3[] _mCellLocalPositions;
    private int _numCells = 4;
    [SerializeField, HideInInspector]private List<TensorTile> tilesList = new();
    private int _smallGridSize;
    private Vector3Int _minorMin;
    private Collider[] _mColliderBuffer;
    private List<Vector3> _hitPositions = new();
    private Vector3 _currentCenter;
    private SingleChannel _channel = new (new Vector2Int(20, 20), 0);
    private List<Vector3> _tilePositions = new();
    private List<Vector3> _tileScales = new();
    private List<int> _colorIndexes = new();

    void InitCellLocalPositions()
    {
        _cellCenterOffset = new Vector3((gridSize.x - 1f) / 2, 0, (gridSize.z - 1f) / 2);
        _numCells = gridSize.x * gridSize.z;
        _mCellLocalPositions = new Vector3[_numCells];

        for (int i = 0; i < _numCells; i++)
        {
            _mCellLocalPositions[i] = GetCellLocalPosition(i);
        }
    }
    
    private Vector3 GetCellLocalPosition(int cellIndex)
    {
        float z = (cellIndex / gridSize.z - _cellCenterOffset.x) * cellScale.z;
        float x = (cellIndex % gridSize.z - _cellCenterOffset.z) * cellScale.x;
        return new Vector3(x, 0, z);
    }
    
    private Vector3 GetCellGlobalPosition(int cellIndex) 
    {
        return _mCellLocalPositions[cellIndex] + transform.position;
    }
    
    
    private void DrawToGrid(Vector3Int start, int size, float drawValue, bool setOrAdd = false)
    {
        var numX = size;
        var numZ = size;
        var xCount = start.x;
        var zCount = start.z;

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
                
                var singleIndex = zCount * gridSize.x + xCount;

                if (setOrAdd)
                {
                    tilesList[singleIndex].SetTileNum(drawValue);
                }
                else
                {
                    tilesList[singleIndex].AddTileNum(drawValue);
                }
                
                xCount++;
            }
            xCount = start.x;
            zCount++;
        }
    }
    
    private void DrawToGrid(Vector3Int start, int size)
    {
        _colorIndexes.Clear();
        var numX = size;
        var numZ = size;
        var xCount = start.x;
        var zCount = start.z;

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
                
                var singleIndex = zCount * gridSize.x + xCount;
                _colorIndexes.Add(singleIndex);
                
                xCount++;
            }
            xCount = start.x;
            zCount++;
        }
    }

    public void DrawGrid()
    {
        InitCellLocalPositions();
        
        for (int i = 0; i < _numCells; i++)
        {
            var temp = Instantiate(tensorTileProto, 
                GetCellGlobalPosition(i), Quaternion.identity, transform);
            temp.SetTileNum(0);
            temp.transform.localScale = cellScale / 10;
            temp.tileIndex = i;
            tilesList.Add(temp);
        }
    }
    
    private bool GetNewGridShape(int index)
    {
        var stepX = 0;
        var stepZ = 0;
        
        switch (index)
        {
            case 0:
                break;
            case 1:
                stepX = 1;
                break;
            case 2:
                stepZ = 1;
                break;
            case 3:
                stepX = 1;
                stepZ = 1;
                break;
        }

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

        var maxX = Mathf.Clamp(_minorMin.x + _smallGridSize, 0, gridSize.x - 1);
        var maxZ = Mathf.Clamp(_minorMin.z + _smallGridSize, 0, gridSize.z - 1);
        var newMax = new Vector3Int(maxX, 0, maxZ);
        GetGridSize(_minorMin, newMax, out var center, out var size);
   
        _currentCenter = center;
        _channel.DrawToGrid(_minorMin, _smallGridSize, 0.1f);
        DrawToGrid(_minorMin, _smallGridSize, 0.1f);
        return true;

    }
    
    private void GetNewShape(int index)
    {
        var stepX = 0;
        var stepZ = 0;
        
        switch (index)
        {
            case 0:
                break;
            case 1:
                stepX = 1;
                break;
            case 2:
                stepZ = 1;
                break;
            case 3:
                stepX = 1;
                stepZ = 1;
                break;
        }

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

        var maxX = Mathf.Clamp(_minorMin.x + _smallGridSize, 0, gridSize.x - 1);
        var maxZ = Mathf.Clamp(_minorMin.z + _smallGridSize, 0, gridSize.z - 1);
        var newMax = new Vector3Int(maxX, 0, maxZ);
        GetGridSize(_minorMin, newMax, out var center, out var size);
   
        _currentCenter = center;
        DrawToGrid(_minorMin, _smallGridSize);
    }

    private void AddVisTile(int indexX, int indexZ, int depth)
    {
        
    }
    
    private void GetGridSize(Vector3Int minIndex, Vector3Int maxIndex, out Vector3 center, out Vector3 size)
    {
        var min = minIndex.z * gridSize.x + minIndex.x;
        var max = maxIndex.z * gridSize.x + maxIndex.x;
        
        var minVec = GetCellGlobalPosition(min) - cellScale / 2;
        var maxVec = GetCellGlobalPosition(max) + cellScale / 2;
        size = new Vector3(maxVec.x - minVec.x, 1, maxVec.z - minVec.z);
       
        center = minVec + size / 2;
    }
    
    void Start()
    {
        _smallGridSize = gridSize.x;
        _currentCenter = transform.position;
        DrawGrid();
    }

    private void SortAll()
    {
        var mainCenter = transform.position;
        foreach (var h in _hitPositions)
        {
            SortThis(h);
            _currentCenter = mainCenter;
            _smallGridSize = gridSize.x;
            _minorMin = new Vector3Int(0, 0, 0);
        }
    }
    
    private void SortThis(Vector3 hit)
    {
        var norm = hit - _currentCenter;

        for (int i = 0; i < 5; i++)
        {
            if (norm.x < 0)
            {
                if (norm.z < 0)
                    GetNewGridShape(0);
                
                else if (norm.z > 0)
                    GetNewGridShape(2);
            }
            else if (norm.x > 0)
            {
                if (norm.z < 0)
                    GetNewGridShape(1);
                
                else if (norm.z > 0)
                    GetNewGridShape(3);
            }
            
            norm = hit - _currentCenter;
        }
    }
    
    private int ScanCell(Vector3 center, Vector3 size)
    {
        _mColliderBuffer = new Collider[800];
        _hitPositions.Clear();
        
        var numFound = Physics.OverlapBoxNonAlloc(center, size, _mColliderBuffer, Quaternion.identity);
        var hitTags = 0;

        for (int i = 0; i < numFound; i++)
        {
            var current = _mColliderBuffer[i].gameObject;
            
            for (int j = 0; j < labels.Count; j++)
            {
                if (current.CompareTag(labels[j].Name))
                {
                    hitTags++;
                    _hitPositions.Add(current.transform.position);
                }
            }
        }
        return hitTags;
    }

    private void ClearTensorVis()
    {
        foreach (var t in tilesList)
        {
            t.SetTileNum(0);
        }
    }
    
    private void UpdateTensorVis()
    {
        ClearTensorVis();   
        ScanCell(_currentCenter, gridSize / 2);
        SortAll();
    }

    private void FixedUpdate()
    {
        UpdateTensorVis();
        
        if (Input.GetKey(KeyCode.Alpha0))
        {
            GetNewShape(0);
        }
        if (Input.GetKey(KeyCode.Alpha1))
        {
            GetNewShape(1);
        }
        if (Input.GetKey(KeyCode.Alpha2))
        {
            GetNewShape(2);
        }
        if (Input.GetKey(KeyCode.Alpha3))
        {
            GetNewShape(3);
        }
    }

    private void OnDrawGizmos()
    {
        InitCellLocalPositions();

        for (int i = 0; i < _numCells; i++)
        {
            if (_colorIndexes.Contains(i))
            {
                Gizmos.color = new Color(0, 255, 255, 0.5f);
            }
            else
            {
                Gizmos.color = new Color(255, 255, 255, 0.5f);
            }
            Gizmos.DrawCube(GetCellGlobalPosition(i), cellScale);
        }
    }
}
