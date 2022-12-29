using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnController : MonoBehaviour
{
    [SerializeField] private SpawnTile spawnTile;
    [SerializeField] private TraceTest traceTest;
    void Start()
    {
        traceTest.GetFreeLocation(out var location);
        spawnTile.SpawnUnit(location);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
