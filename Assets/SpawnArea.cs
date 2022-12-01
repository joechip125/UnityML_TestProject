using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnArea : MonoBehaviour
{
    public GameObject spawnPrototype;
    public GameObject wallPrototype;
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

    public void RespawnCollection()
    {
        var counter = 0;
        foreach (var t in tiles)
        {
            t.ClearAllCollect();

            if(counter != 0)
                t.SpawnSetAmount(Random.Range(1, 4));
            counter++;
        }
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
        SpawnWalls(numX, numZ);
    }

    private void SpawnWalls(int numX, int numZ)
    {
        var placeX = (numX - 1) * 5;
        
        var temp2 = Instantiate(wallPrototype, 
            transform.localPosition + new Vector3(placeX, 0, -5.5f), 
            Quaternion.identity, transform);
        temp2.transform.localScale = new Vector3(numX * 10, 5, 1);
        
        temp2 = Instantiate(wallPrototype, 
            transform.localPosition + new Vector3(placeX, 0, numZ * 10 - 4.5f), 
            Quaternion.identity, transform);
        temp2.transform.localScale = new Vector3(numX * 10, 5, 1);
        
        temp2 = Instantiate(wallPrototype, 
            transform.localPosition + new Vector3(-5.5f, 0, numZ * 5 - 5f), 
            Quaternion.identity, transform);
        temp2.transform.localScale = new Vector3(1, 5, numZ * 10);
        
        temp2 = Instantiate(wallPrototype, 
            transform.localPosition + new Vector3(numX * 10 -4.5f, 0, numZ * 5 - 5f), 
            Quaternion.identity, transform);
        temp2.transform.localScale = new Vector3(1, 5, numZ * 10);
    }
    
 
}
