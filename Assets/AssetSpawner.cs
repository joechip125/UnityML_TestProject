using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class AssetSpawner : MonoBehaviour
{
    public GameObject spawnObject;
    private GameObject _keepObject;

    public void SpawnCollect()
    {
        if (_keepObject) Destroy(_keepObject);
        
        var random = transform.localPosition   
                     + new Vector3(Random.Range(0, 12), 0.15f, Random.Range(0, 12));
        _keepObject  = Instantiate(spawnObject,
            transform, false);
        _keepObject.transform.localPosition = random;
    }
    
    
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
