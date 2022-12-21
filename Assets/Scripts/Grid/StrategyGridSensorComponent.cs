using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace.Grid;
using MBaske.Sensors.Grid;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class StrategyGridSensorComponent : SensorComponent
{
    CustomGridSensor _mDebugSensor;
    
    List<CustomGridSensor> _mSensors;
    
    internal OverlapChecker BoxOverlapChecker;
    
    public ColorGridBuffer GridBuffer
    {
        get { return _mGridBuffer; }
        set { _mGridBuffer = value; GridShape = value.GetShape(); }
    }
    private ColorGridBuffer _mGridBuffer;
    
    public SingleChannel ExternalChannel;

    public SingleChannel MaskChannel;
    
    public GridBuffer.Shape GridShape
    {
        get => gridShape;
        set => gridShape = value;
    }
    [SerializeField]
    private GridBuffer.Shape gridShape;
    
   
    [SerializeField]
    protected internal string sensorName = "GridSensor";
    public string SensorName
    {
        get { return sensorName; }
        set { sensorName = value; }
    }

    public List<ChannelLabel> ChannelLabels => channelLabels;

    [SerializeField]
    protected List<ChannelLabel> channelLabels;

    [SerializeField]
    internal Vector3 cellScale = new Vector3(1f, 0.01f, 1f);
    
    public Vector3Int GridSize
    {
        get { return gridSize; }
        set
        {
            if (value.y != 1)
            {
                gridSize = new Vector3Int(value.x, 1, value.z);
            }
            else
            {
                gridSize = value;
            }
        }
    }

    [SerializeField]
    internal Vector3Int gridSize = new Vector3Int(20, 1, 20);
    
    [SerializeField]
    internal LayerMask colliderMask;
    
    [SerializeField]
    internal int maxColliderBufferSize = 500;
    
    [SerializeField]
    internal int initialColliderBufferSize = 4;
    
    [SerializeField]
    internal bool showGizmos = false;

    [SerializeField]
    internal SensorCompressionType compressionType = SensorCompressionType.PNG;
    public SensorCompressionType CompressionType
    {
        get => compressionType;
        set { compressionType = value; UpdateSensor(); }
    }

    [SerializeField]
    [Range(1, 50)]
    [Tooltip("Number of frames of observations that will be stacked before being fed to the neural network.")]
    internal int observationStacks = 1;

    [SerializeField] private int totalNumberChannels = 3;

    public int ObservationStacks
    {
        get => observationStacks;
        set => observationStacks = value;
    }
    
    /// <inheritdoc/>
    public override ISensor[] CreateSensors()
    {
        BoxOverlapChecker = new OverlapChecker(
            cellScale,
            gridSize,
            colliderMask,
            gameObject,
            initialColliderBufferSize,
            maxColliderBufferSize,
            channelLabels
        );

        _mGridBuffer = new ColorGridBuffer(totalNumberChannels, gridSize.x, gridSize.z);
        gridShape = new GridBuffer.Shape(totalNumberChannels, gridSize.x, gridSize.z);
        MaskChannel = new SingleChannel(gridSize.x, gridSize.z, 3);
        
        // debug data is positive int value and will trigger data validation exception if SensorCompressionType is not None.
        _mDebugSensor = new CustomGridSensor("DebugGridSensor", SensorCompressionType.None, _mGridBuffer, ChannelLabels, ExternalChannel);
        BoxOverlapChecker.RegisterDebugSensor(_mDebugSensor);
    
        _mSensors = GetGridSensors().ToList();
        if (_mSensors == null || _mSensors.Count < 1)
        {
            throw new UnityAgentsException("GridSensorComponent received no sensors. Specify at least one observation type (OneHot/Counting) to use grid sensors." +
                "If you're overriding GridSensorComponent.GetGridSensors(), return at least one grid sensor.");
        }
    
        // Only one sensor needs to reference the boxOverlapChecker, so that it gets updated exactly once
        _mSensors[0].m_BoxOverlapChecker = BoxOverlapChecker;
        _mSensors[0].MaskObjectDetected += OnMaskedObjectDetected;
        foreach (var sensor in _mSensors)
        {
            BoxOverlapChecker.RegisterSensor(sensor);
        }
    
        if (ObservationStacks != 1)
        {
            var sensors = new ISensor[_mSensors.Count];
            for (var i = 0; i < _mSensors.Count; i++)
            {
                sensors[i] = new StackingSensor(_mSensors[i], ObservationStacks);
            }
            return sensors;
        }
        else
        {
            return _mSensors.ToArray();
        }
    }

    private void OnMaskedObjectDetected(int cellIndex, int channel)
    {
        var xVal = cellIndex % gridSize.z;
        var zVal = (cellIndex - xVal) / gridSize.z;
        
        if (MaskChannel.Read(xVal, zVal) < 1)
        {
            MaskChannel.Write(xVal, zVal, 1);
        }
    }
    
    protected virtual CustomGridSensor[] GetGridSensors()
    {
        List<CustomGridSensor> sensorList = new List<CustomGridSensor>();
        var sensor = new CustomGridSensor(sensorName, compressionType, _mGridBuffer, ChannelLabels, ExternalChannel);
        sensorList.Add(sensor);
        return sensorList.ToArray();
    }

    internal void UpdateSensor()
    {
        if (_mSensors == null) return;
        
        BoxOverlapChecker.ColliderMask = colliderMask;
        foreach (var sensor in _mSensors)
        {
            sensor.CompressionType = compressionType;
        }
    }

    public Vector3 GetCellPosition(int cellIndex)
    {
        return BoxOverlapChecker.GetCellGlobalPosition(cellIndex);
    }
    
    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        if (BoxOverlapChecker == null || _mDebugSensor == null)
        {
            return;
        }
        
        _mDebugSensor.ResetGridBuffer();
        BoxOverlapChecker.UpdateGizmo();
        var num = gridSize.x * gridSize.z;
        for (var i = 0; i < num; i++)
        {
            var cellPosition = BoxOverlapChecker.GetCellGlobalPosition(i);
            var debugRayColor = Color.white;

            if (_mGridBuffer.ReadAllChannelsAtIndex(i, out var channel, out var value))
            {
                debugRayColor = channelLabels[channel].Color;
            }
            
            Gizmos.color = new Color(debugRayColor.r, debugRayColor.g, debugRayColor.b, .5f);
            Gizmos.DrawCube( cellPosition, Vector3.one);
        }
    }
    
}
