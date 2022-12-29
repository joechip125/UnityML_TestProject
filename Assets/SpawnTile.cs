using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTile : MonoBehaviour
{

    [SerializeField] private GameObject spawnPrototype;

    public void SpawnUnit(Vector3 moveLocation)
    {
        var temp = Instantiate(spawnPrototype, transform.position, Quaternion.identity);
        temp.GetComponent<UnitMovement>().Goal = moveLocation;
    }
    
    
    void Start()
    {
        
    }

    
    void Update()
    {
        
    }
}
