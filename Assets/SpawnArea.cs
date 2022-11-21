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
    
    void Start()
    {
        SpawnAreas(xTiles, zTiles);
    }

    public void RespawnCollect()
    {
        foreach (var t in tiles)
        {
            t.ClearAllCollect();
            t.SpawnRandomAmount();
        }
    }

    public bool CheckTile(int tileChoice)
    {
        var num = tiles[tileChoice].numberCollect;

        var high = tiles.OrderBy(x => x.numberCollect)
            .ToList()[^1].collectValue;

        Debug.Log($"Choice val:{num}, High val: {high}");
        if (num > high)
            return true;

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
