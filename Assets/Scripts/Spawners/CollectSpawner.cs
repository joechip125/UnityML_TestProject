using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


public class CollectSpawner : MonoBehaviour
{
    public GameObject spawnPrototype;
    private List<GameObject> _spawnedObjeccts = new List<GameObject>(); 
    public int spawnAmount;
    public Transform owner;
    public Vector3 spawnLoc;
    public float xRange;
    public float zRange;
    
    public void SpawnSingle(int hitCount, int hitInterval)
    {
        DestroyAll();
        var local = owner.localPosition + new Vector3(Random.Range(-xRange, xRange), 
            0.2f,Random.Range(-zRange, zRange));
       
        _spawnedObjeccts.Add(Instantiate(spawnPrototype, local, Quaternion.identity, owner));
    }

    public void DestroyAll()
    {
        foreach (var o in _spawnedObjeccts)
        {
            if(o) Destroy(o);
        }
        _spawnedObjeccts.Clear();
    }

    public void Respawn(float rangeX, float rangeZ)
    {
        DestroyAll();

        for (int i = 0; i < spawnAmount; i++)
        {
            var local = owner.localPosition + new Vector3(Random.Range(-rangeX, rangeX), 
                0.2f,Random.Range(-rangeZ, rangeZ));
           
            _spawnedObjeccts.Add(Instantiate(spawnPrototype, local, Quaternion.identity, owner));
        }
    }
    
    public void DestroyThis(GameObject toDestroy)
    {
        var someObject = _spawnedObjeccts
            .SingleOrDefault(x => x == toDestroy);

        if (someObject != default)
        {
            Destroy(someObject);
        }
    }
}
