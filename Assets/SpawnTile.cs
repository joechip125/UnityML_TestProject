using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTile : MonoBehaviour
{

    [SerializeField] private GameObject spawnPrototype;

    public void SpawnUnit()
    {
        var temp = Instantiate(spawnPrototype, transform.position, Quaternion.identity);
    }
    
    
    void Start()
    {
        SpawnUnit();
    }

    
    void Update()
    {
        
    }
}
