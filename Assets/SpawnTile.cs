using System.Collections;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;

public class SpawnTile : MonoBehaviour
{
    [SerializeField] private GameObject spawnPrototype;

    public IUnitControlInterface SpawnUnit()
    {
        var temp = Instantiate(spawnPrototype, transform.position, Quaternion.identity);
        var inter = temp.GetComponent<IUnitControlInterface>();
        
        return inter;
    }
}
