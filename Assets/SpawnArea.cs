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
    private IntVector2 _theRightChoice;
    private int _rightChoice;
    private int _totalScore;

    private int _numWrong;
    private int _numRight;
    
    void Start()
    {
        SpawnAreas(xTiles, zTiles);
    }

    public void RespawnCollect()
    {
        var randChoice = Random.Range(1, 4);
        var counter = 0;

        foreach (var t in tiles)
        {
            t.ClearAllCollect();

            if (counter == randChoice)
            {
                _rightChoice = counter;
                _theRightChoice = t.coordinates;
                t.SpawnSetAmount(1);
                //t.UpdateTileStatus(TileStatus.Right);
            }
            
            else if(counter != randChoice)
            {
               // t.UpdateTileStatus(TileStatus.Wrong);
            }
            counter++;
        }
    }

    public bool CheckTile(int tileChoice)
    {
        if (tileChoice == _rightChoice)
        {
            tiles[tileChoice].CollectValue++;
            return true;
        }

        tiles[tileChoice].CollectValue--;
        return false;
    }

    public Vector3 GetLocationAtTile(IntVector2 tileCoords)
    {
        return tiles.Single(x => x.coordinates == tileCoords)
            .transform.localPosition;
    }
    
    public bool CheckTile(IntVector2 tileChoice)
    {
        if (tileChoice == _theRightChoice)
        {
            Debug.Log($"Right{tileChoice} {_numRight++}");
            //tiles.Single(x => x.coordinates == tileChoice).CollectValue++;
            return true;
        }
        Debug.Log($"Wrong{tileChoice} {_numWrong++}");
        //tiles.Single(x => x.coordinates == tileChoice).CollectValue--;
        return false;
    }
    

    void SpawnAreas(int numX, int numZ)
    {
        Vector3 spawnLoc = transform.localPosition;
        for (int x = 0; x < numX; x++)
        {
            for (int z = 0; z < numZ; z++)
            {
                var temp = Instantiate(spawnPrototype, spawnLoc + new Vector3(0,0, z * 10), 
                    quaternion.identity, transform).GetComponent<FieldTile>();
                tiles.Add(temp);
                temp.coordinates = new IntVector2(x, z);
            }

            spawnLoc += new Vector3(10,0,0);
        }
    }
    
 
}
