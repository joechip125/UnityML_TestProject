using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class CollectSpawner : MonoBehaviour
{
    public GameObject spawnPrototype;
    private List<GameObject> _spawnedObjeccts = new List<GameObject>(); 
    public int spawnAmount;

    public void Respawn(float rangeX, float rangeZ)
    {
        foreach (var o in _spawnedObjeccts)
        {
            if(o) Destroy(o);
        }

        for (int i = 0; i < spawnAmount; i++)
        {
            var spawnPoint =new Vector3(Random.Range(-rangeX, rangeX), 
                0.2f,Random.Range(-rangeZ, rangeZ));
            _spawnedObjeccts.Add(Instantiate(spawnPrototype, spawnPoint, Quaternion.identity));
        }
    }
    
}
