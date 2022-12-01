using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class StrategyGridSensorComponent : GridSensorComponent
{
    
    [HideInInspector, SerializeField]
    internal SensorCompressionType m_CompressionType2 = SensorCompressionType.PNG;
    [HideInInspector, SerializeField]
    internal string[] m_DetectableTags2;
    [HideInInspector, SerializeField]
    internal Vector3Int m_GridSize2 = new Vector3Int(16, 1, 16);
    [HideInInspector, SerializeField]
    internal Vector3 m_CellScale2 = new Vector3(1f, 0.01f, 1f);
    
    protected override GridSensorBase[] GetGridSensors()
    {
        List<GridSensorBase> sensorList = new List<GridSensorBase>();
        var sensor = new OneHotGridSensor(m_SensorName + "-OneHot", m_CellScale2, m_GridSize2, m_DetectableTags2, m_CompressionType2);
        sensorList.Add(sensor);
        return sensorList.ToArray();
    }
    
}
