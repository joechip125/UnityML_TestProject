using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SpawnArea : MonoBehaviour
{
    public GameObject spawnPrototype;
    public int xTiles;
    public int zTiles;
    
    
    void Start()
    {
        SpawnAreas(xTiles, zTiles);
    }

    void SpawnAreas(int numX, int numZ)
    {
        Vector3 spawnLoc = transform.localPosition;
        for (int x = 0; x < numX; x++)
        {
            for (int z = 0; z < numX; z++)
            {
                Instantiate(spawnPrototype, spawnLoc + new Vector3(0,0, z * 10), 
                    quaternion.identity, transform);
            }

            spawnLoc += new Vector3(10,0,0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
