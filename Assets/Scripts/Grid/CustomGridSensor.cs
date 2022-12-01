using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class CustomGridSensor : GridSensorBase
{
    
    public CustomGridSensor(string name, Vector3 cellScale, Vector3Int gridSize, string[] detectableTags, SensorCompressionType compression) 
        : base(name, cellScale, gridSize, detectableTags, compression)
    {
    }
}
