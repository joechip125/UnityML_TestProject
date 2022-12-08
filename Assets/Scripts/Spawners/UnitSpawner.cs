using System;
using System.Collections;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;


public enum UnitTypes
{
    Collector,
    Ranged,
    Melee
}

[Serializable]
public class Units
{
    public GameObject theUnit;
    public UnitTypes unitType;
}

public class UnitSpawner : MonoBehaviour
{
    public List<Units> unitsList;
    
    public IUnitControlInterface SpawnNewUnit(Vector3 spawnPos, Transform parent)
    {
        var gObj = Instantiate(unitsList[0].theUnit,
            parent.parent, false);
        gObj.transform.localScale = new Vector3(1, 1, 1);
        
           var inter = (IUnitControlInterface)gObj.GetComponent(typeof(IUnitControlInterface));
        
        return inter;
    }
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
