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
    public List<Transform> Locations = new List<Transform>();
    public Vector3 spawnLoc;

    public void Init()
    {
        GetNewLocation();
    }
    public void SpawnSingle(int hitCount, int hitInterval)
    {
        DestroyAll();
        if (hitCount % hitInterval == 0)
        {
         //   GetNewLocation();
        }
        _spawnedObjeccts.Add(Instantiate(spawnPrototype, Locations[hitCount].localPosition, Quaternion.identity, owner));
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

    public Vector3 GetNewLocation()
    {
        var vec = Locations[0].localPosition;
        spawnLoc = vec;
        Locations.RemoveAt(0);
        return vec;
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
