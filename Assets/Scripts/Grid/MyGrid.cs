using System;
using System.Collections;
using System.Collections.Generic;
using MBaske.Sensors.Grid;
using UnityEngine;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using Random = UnityEngine.Random;


public class MyGrid : MonoBehaviour
{
    [SerializeField]private Vector3 cellScale = new(1f, 1, 1f);
    [SerializeField] private Vector3Int gridSize = new Vector3Int(12, 1, 12);
    [SerializeField] List<ChannelLabel> labels;

    private GridBuffer _gridBuffer;
    private Vector3 m_CellCenterOffset = new Vector3(0.5f, 0, 0.5f);
    private Vector3[] _mCellLocalPositions;
    
    Collider[] _mColliderBuffer;
    private Vector4[] _targets;
    
    private int _numCells = 4;

    private List<Vector4> _cubers = new();
    private Vector3Int _minorMin;
    private int _smallGridSize;
    
    void InitCellLocalPositions()
    {
        m_CellCenterOffset = new Vector3((gridSize.x - 1f) / 2, 0, (gridSize.z - 1f) / 2);
        _numCells = gridSize.x * gridSize.z;
        _mCellLocalPositions = new Vector3[_numCells];

        for (int i = 0; i < _numCells; i++)
        {
            _mCellLocalPositions[i] = GetCellLocalPosition(i);
        }
    }
    
    private Vector3 GetCellLocalPosition(int cellIndex)
    {
        float z = (cellIndex / gridSize.z - m_CellCenterOffset.x) * cellScale.z;
        float x = (cellIndex % gridSize.z - m_CellCenterOffset.z) * cellScale.x;
        return new Vector3(x, 0, z);
    }
    
    private Vector3 GetCellGlobalPosition(int cellIndex) 
    {
        return _mCellLocalPositions[cellIndex] + transform.position;;
    }
    
    private void StackGrids()
    {
        _cubers.Clear();
        _minorMin = new Vector3Int(0, 0,0);
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
        
        var minVec = GetCellGlobalPosition(min) - cellScale / 2;
        var maxVec = GetCellGlobalPosition(max) + cellScale / 2;
        var theSize = new Vector3(maxVec.x - minVec.x, 1, maxVec.z - minVec.z);
       
        var  theCenter = minVec + theSize / 2;

        _cubers.Add(new Vector4(theSize.x, theSize.z, theCenter.x, theCenter.z));
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
                Gizmos.DrawCube(GetCellGlobalPosition(start2), cellScale);
                
                xCount++;
            }
            xCount = start.x;
            zCount++;
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
    
    void OnDrawGizmos()
    {
        InitCellLocalPositions();
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
            Gizmos.DrawCube(GetCellGlobalPosition(i), cellScale);
        }
    }
}
