using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class AssetSpawner : MonoBehaviour
{
    public GameObject spawnObject;


    public void SpawnCollect()
    {
        var random = transform.localPosition   
                     + new Vector3(Random.Range(0, 12), 0.15f, Random.Range(0, 12));
        Instantiate(spawnObject, random, Quaternion.identity);
    }
    
    void Start()
    {
        SpawnCollect();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
