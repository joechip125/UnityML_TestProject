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
    
    public IUnitControlInterface SpawnNewUnit()
    {
        var inter = (IUnitControlInterface)Instantiate(unitsList[0].theUnit, 
            transform.localPosition + new Vector3(0, 0.5f,0), Quaternion.identity).
            GetComponent(typeof(IUnitControlInterface));
        
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
