using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnArea : MonoBehaviour
{
    public GameObject spawnPrototype;
    public int xTiles;
    public int zTiles;
    public List<FieldTile> tiles;
    public IntVector2 coordinates;
    private int _rightChoice;
    private int _totalScore;
    
    void Start()
    {
        SpawnAreas(xTiles, zTiles);
    }

    public void RespawnCollect()
    {
        var randChoice = Random.Range(0, 4);
        var counter = 0;
        
        foreach (var t in tiles)
        {
            t.ClearAllCollect();

            if (counter == randChoice)
            {
                _rightChoice = counter;
                t.SpawnSetAmount(1);
            }
            counter++;
        }
    }

    public bool CheckTile(int tileChoice)
    {
        if (tileChoice == _rightChoice)
        {
            tiles[tileChoice].CollectValue++;
            tiles[tileChoice].UpdateTileStatus(TileStatus.Right);
            return true;
        }

        tiles[tileChoice].CollectValue = 0;
        tiles[tileChoice].UpdateTileStatus(TileStatus.Wrong);
        return false;
    }
    

    void SpawnAreas(int numX, int numZ)
    {
        Vector3 spawnLoc = transform.localPosition;
        for (int x = 0; x < numX; x++)
        {
            for (int z = 0; z < numX; z++)
            {
                var temp = Instantiate(spawnPrototype, spawnLoc + new Vector3(0,0, z * 10), 
                    quaternion.identity, transform);
                tiles.Add(temp.GetComponent<FieldTile>());
            }

            spawnLoc += new Vector3(10,0,0);
        }
    }
    
 
}
