using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TensorVis : MonoBehaviour
{
    [SerializeField]private TensorTile tensorTileProto;
    
    [SerializeField] private Vector3Int gridSize= new Vector3Int(12, 1, 12);
    [SerializeField]private Vector3 cellScale = new(1f, 1, 1f);
    
    private Vector3 _cellCenterOffset = new Vector3(0.5f, 0, 0.5f);
    private Vector3[] _mCellLocalPositions;
    private int _numCells = 4;
    private Dictionary<int, TensorTile> _tiles = new();

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
        return _mCellLocalPositions[cellIndex] + transform.position;;
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
            _tiles.Add(i, temp);
        }
    }
    
    void Start()
    {
        DrawGrid();    
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
