using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MBaske.Sensors.Grid;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class StrategyGridSensorComponent : SensorComponent
{
    // dummy sensor only used for debug gizmo
    CustomGridSensor m_DebugSensor;
    
    List<CustomGridSensor> m_Sensors;
    
    internal OverlapChecker m_BoxOverlapChecker;
    
    public ColorGridBuffer GridBuffer
    {
        get { return m_GridBuffer; }
        set { m_GridBuffer = value; GridShape = value.GetShape(); }
    }
    private ColorGridBuffer m_GridBuffer;

    public ColorGridBuffer ExternalBuffer;
    
    public GridBuffer.Shape GridShape
    {
        get => m_GridShape;
        set => m_GridShape = value;
    }
    [SerializeField]
    private GridBuffer.Shape m_GridShape;
    
   
    [SerializeField]
    protected internal string m_SensorName = "GridSensor";
    public string SensorName
    {
        get { return m_SensorName; }
        set { m_SensorName = value; }
    }

    public List<ChannelLabel> ChannelLabels => m_ChannelLabels;

    [SerializeField]
    protected List<ChannelLabel> m_ChannelLabels;

    [SerializeField]
    internal Vector3 m_CellScale = new Vector3(1f, 0.01f, 1f);
    
    public Vector3Int GridSize
    {
        get { return m_GridSize; }
        set
        {
            if (value.y != 1)
            {
                m_GridSize = new Vector3Int(value.x, 1, value.z);
            }
            else
            {
                m_GridSize = value;
            }
        }
    }

    [SerializeField]
    internal Vector3Int m_GridSize = new Vector3Int(20, 1, 20);
    
    [SerializeField]
    internal LayerMask m_ColliderMask;
    
    [SerializeField]
    internal int m_MaxColliderBufferSize = 500;
    
    [SerializeField]
    internal int m_InitialColliderBufferSize = 4;
    
    [SerializeField]
    internal float m_GizmoYOffset = 0f;
    
    [SerializeField]
    internal bool m_ShowGizmos = false;

    [SerializeField]
    internal SensorCompressionType m_CompressionType = SensorCompressionType.PNG;
    public SensorCompressionType CompressionType
    {
        get { return m_CompressionType; }
        set { m_CompressionType = value; UpdateSensor(); }
    }

    [SerializeField]
    [Range(1, 50)]
    [Tooltip("Number of frames of observations that will be stacked before being fed to the neural network.")]
    internal int m_ObservationStacks = 1;

    [SerializeField] private int TotalNumberChannels = 3;

    public int ObservationStacks
    {
        get { return m_ObservationStacks; }
        set { m_ObservationStacks = value; }
    }
    
    /// <inheritdoc/>
    public override ISensor[] CreateSensors()
    {
        m_BoxOverlapChecker = new OverlapChecker(
            m_CellScale,
            m_GridSize,
            m_ColliderMask,
            gameObject,
            m_InitialColliderBufferSize,
            m_MaxColliderBufferSize,
            m_ChannelLabels
        );

        m_GridBuffer = new ColorGridBuffer(TotalNumberChannels, m_GridSize.x, m_GridSize.z);
        m_GridShape = new GridBuffer.Shape(TotalNumberChannels, m_GridSize.x, m_GridSize.z);
        
        // debug data is positive int value and will trigger data validation exception if SensorCompressionType is not None.
        m_DebugSensor = new CustomGridSensor("DebugGridSensor", SensorCompressionType.None, m_GridBuffer, ExternalBuffer, ChannelLabels);
        m_BoxOverlapChecker.RegisterDebugSensor(m_DebugSensor);
    
        m_Sensors = GetGridSensors().ToList();
        if (m_Sensors == null || m_Sensors.Count < 1)
        {
            throw new UnityAgentsException("GridSensorComponent received no sensors. Specify at least one observation type (OneHot/Counting) to use grid sensors." +
                "If you're overriding GridSensorComponent.GetGridSensors(), return at least one grid sensor.");
        }
    
        // Only one sensor needs to reference the boxOverlapChecker, so that it gets updated exactly once
        m_Sensors[0].m_BoxOverlapChecker = m_BoxOverlapChecker;
        foreach (var sensor in m_Sensors)
        {
            m_BoxOverlapChecker.RegisterSensor(sensor);
        }
    
        if (ObservationStacks != 1)
        {
            var sensors = new ISensor[m_Sensors.Count];
            for (var i = 0; i < m_Sensors.Count; i++)
            {
                sensors[i] = new StackingSensor(m_Sensors[i], ObservationStacks);
            }
            return sensors;
        }
        else
        {
            return m_Sensors.ToArray();
        }
    }
    
    protected virtual CustomGridSensor[] GetGridSensors()
    {
        List<CustomGridSensor> sensorList = new List<CustomGridSensor>();
        var sensor = new CustomGridSensor(m_SensorName, m_CompressionType, m_GridBuffer, ExternalBuffer, ChannelLabels);
        sensorList.Add(sensor);
        return sensorList.ToArray();
    }

    /// <summary>
    /// Update fields that are safe to change on the Sensor at runtime.
    /// </summary>
    internal void UpdateSensor()
    {
        if (m_Sensors == null) return;
        
        m_BoxOverlapChecker.ColliderMask = m_ColliderMask;
        foreach (var sensor in m_Sensors)
        {
            sensor.CompressionType = m_CompressionType;
        }
    }
    
    void OnDrawGizmos()
    {
        if (!m_ShowGizmos) return;
        if (m_BoxOverlapChecker == null || m_DebugSensor == null)
        {
            return;
        }
        
        m_DebugSensor.ResetGridBuffer();
        m_BoxOverlapChecker.UpdateGizmo();
        var num = m_GridSize.x * m_GridSize.z;
        var gizmoYOffset = new Vector3(0, m_GizmoYOffset, 0);
        for (var i = 0; i < num; i++)
        {
            var cellPosition = m_BoxOverlapChecker.GetCellGlobalPosition(i);
            var debugRayColor = Color.white;

            if (m_GridBuffer.ReadAll(i, out var channel, out var value))
            {
                debugRayColor =m_ChannelLabels[(int) channel].Color;
            }
            
            Gizmos.color = new Color(debugRayColor.r, debugRayColor.g, debugRayColor.b, .5f);
            Gizmos.DrawCube( cellPosition, Vector3.one);
        }
    }
    
}
