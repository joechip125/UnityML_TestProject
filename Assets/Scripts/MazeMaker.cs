using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class MazeMaker : MonoBehaviour
{
    public GameObject floorTile;
    public int numberTiles;
    
    public int sizeX, sizeZ;
	
    public MazeCell cellPrefab;
    public Pellet pelletPrefab;

    private MazeCell[,] cells;
    
    public MazeMaker mazePrefab;

    private MazeMaker mazeInstance;
    
    void Start()
    {
    }
    
    public void Generate () 
    {
        cells = new MazeCell[sizeX, sizeZ];
        for (int x = 0; x < sizeX; x++) 
        {
            for (int z = 0; z < sizeZ; z++) 
            {
                CreateCell(x, z);
            }
        }
    }

    private void CreateCell (int x, int z) 
    {
        MazeCell newCell = Instantiate(cellPrefab) as MazeCell;
        var pellet = Instantiate(pelletPrefab);
        var pos = new 
            Vector3(x - sizeX * 0.5f + 0.5f, 0f, z - sizeZ * 0.5f + 0.5f);
        cells[x, z] = newCell;
        newCell.name = "Maze Cell " + x + ", " + z;
        pellet.transform.parent = transform;
        newCell.transform.parent = transform;
        pellet.transform.localPosition = pos + new Vector3(0,0.5f,0);
        newCell.transform.localPosition = pos;
    }
    
    void SpawnTiles()
    {
        Vector3 start = transform.position;
        for (int i = 0; i < numberTiles; i++)
        {
            Instantiate(floorTile, start, Quaternion.identity, transform);
            start += new Vector3(0,0,1);
        }
    }

    // Update is called once per frame
    void Update()
    {
   
    }
}
