using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnController : MonoBehaviour
{
    [SerializeField] private SpawnTile spawnTile;
    [SerializeField] private TraceTest traceTest;
    void Start()
    {
        StartCoroutine(SpawnSome(5, 2));
    }

    private IEnumerator SpawnSome(int spawnAmount, float spawnDelay)
    {
        for (int i = 0; i < spawnAmount; i++)
        {
            yield return new WaitForSeconds(spawnDelay);
            traceTest.RegisterUnit(spawnTile.SpawnUnit());
        }
    }
    
}
