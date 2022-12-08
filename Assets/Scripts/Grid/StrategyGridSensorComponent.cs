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
    CustomGridSensor _mDebugSensor;
    
    List<CustomGridSensor> _mSensors;
    
    internal OverlapChecker MBoxOverlapChecker;
    
    public ColorGridBuffer GridBuffer
    {
        get { return _mGridBuffer; }
        set { _mGridBuffer = value; GridShape = value.GetShape(); }
    }
    private ColorGridBuffer _mGridBuffer;

    public ColorGridBuffer ExternalBuffer;
    
    public GridBuffer.Shape GridShape
    {
        get => mGridShape;
        set => mGridShape = value;
    }
    [SerializeField]
    private GridBuffer.Shape mGridShape;
    
   
    [SerializeField]
    protected internal string mSensorName = "GridSensor";
    public string SensorName
    {
        get { return mSensorName; }
        set { mSensorName = value; }
    }

    public List<ChannelLabel> ChannelLabels => mChannelLabels;

    [SerializeField]
    protected List<ChannelLabel> mChannelLabels;

    [SerializeField]
    internal Vector3 mCellScale = new Vector3(1f, 0.01f, 1f);
    
    public Vector3Int GridSize
    {
        get { return mGridSize; }
        set
        {
            if (value.y != 1)
            {
                mGridSize = new Vector3Int(value.x, 1, value.z);
            }
            else
            {
                mGridSize = value;
            }
        }
    }

    [SerializeField]
    internal Vector3Int mGridSize = new Vector3Int(20, 1, 20);
    
    [SerializeField]
    internal LayerMask mColliderMask;
    
    [SerializeField]
    internal int mMaxColliderBufferSize = 500;
    
    [SerializeField]
    internal int mInitialColliderBufferSize = 4;
    
    [SerializeField]
    internal bool mShowGizmos = false;

    [SerializeField]
    internal SensorCompressionType mCompressionType = SensorCompressionType.PNG;
    public SensorCompressionType CompressionType
    {
        get => mCompressionType;
        set { mCompressionType = value; UpdateSensor(); }
    }

    [SerializeField]
    [Range(1, 50)]
    [Tooltip("Number of frames of observations that will be stacked before being fed to the neural network.")]
    internal int mObservationStacks = 1;

    [SerializeField] private int totalNumberChannels = 3;

    public int ObservationStacks
    {
        get => mObservationStacks;
        set => mObservationStacks = value;
    }
    
    /// <inheritdoc/>
    public override ISensor[] CreateSensors()
    {
        MBoxOverlapChecker = new OverlapChecker(
            mCellScale,
            mGridSize,
            mColliderMask,
            gameObject,
            mInitialColliderBufferSize,
            mMaxColliderBufferSize,
            mChannelLabels
        );

        _mGridBuffer = new ColorGridBuffer(totalNumberChannels, mGridSize.x, mGridSize.z);
        mGridShape = new GridBuffer.Shape(totalNumberChannels, mGridSize.x, mGridSize.z);
        
        // debug data is positive int value and will trigger data validation exception if SensorCompressionType is not None.
        _mDebugSensor = new CustomGridSensor("DebugGridSensor", SensorCompressionType.None, _mGridBuffer, ExternalBuffer, ChannelLabels);
        MBoxOverlapChecker.RegisterDebugSensor(_mDebugSensor);
    
        _mSensors = GetGridSensors().ToList();
        if (_mSensors == null || _mSensors.Count < 1)
        {
            throw new UnityAgentsException("GridSensorComponent received no sensors. Specify at least one observation type (OneHot/Counting) to use grid sensors." +
                "If you're overriding GridSensorComponent.GetGridSensors(), return at least one grid sensor.");
        }
    
        // Only one sensor needs to reference the boxOverlapChecker, so that it gets updated exactly once
        _mSensors[0].m_BoxOverlapChecker = MBoxOverlapChecker;
        foreach (var sensor in _mSensors)
        {
            MBoxOverlapChecker.RegisterSensor(sensor);
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
    
    protected virtual CustomGridSensor[] GetGridSensors()
    {
        List<CustomGridSensor> sensorList = new List<CustomGridSensor>();
        var sensor = new CustomGridSensor(mSensorName, mCompressionType, _mGridBuffer, ExternalBuffer, ChannelLabels);
        sensorList.Add(sensor);
        return sensorList.ToArray();
    }

    /// <summary>
    /// Update fields that are safe to change on the Sensor at runtime.
    /// </summary>
    internal void UpdateSensor()
    {
        if (_mSensors == null) return;
        
        MBoxOverlapChecker.ColliderMask = mColliderMask;
        foreach (var sensor in _mSensors)
        {
            sensor.CompressionType = mCompressionType;
        }
    }
    
    void OnDrawGizmos()
    {
        if (!mShowGizmos) return;
        if (MBoxOverlapChecker == null || _mDebugSensor == null)
        {
            return;
        }
        
        _mDebugSensor.ResetGridBuffer();
        MBoxOverlapChecker.UpdateGizmo();
        var num = mGridSize.x * mGridSize.z;
        for (var i = 0; i < num; i++)
        {
            var cellPosition = MBoxOverlapChecker.GetCellGlobalPosition(i);
            var debugRayColor = Color.white;

            if (_mGridBuffer.ReadAll(i, out var channel, out var value))
            {
                debugRayColor =mChannelLabels[(int) channel].Color;
            }
            
            Gizmos.color = new Color(debugRayColor.r, debugRayColor.g, debugRayColor.b, .5f);
            Gizmos.DrawCube( cellPosition, Vector3.one);
        }
    }
    
}
