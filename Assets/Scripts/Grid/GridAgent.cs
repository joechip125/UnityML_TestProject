using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using DefaultNamespace.Grid;
using Grid;
using MBaske.Sensors.Grid;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;


public class GridAgent : Agent
{ 
    public event Action ResetMap;
    
    public PositionStore positions;
    [SerializeField] public Transform owner;
    
    private const int Zero = 0;
    private const int One = 1;
    private const int Two = 2;
    private const int Three = 3;
    private const int Revert = 4;

    private bool _mIsTraining;
    
    private bool _mIsActive;
    private bool _taskComplete;
    private bool _taskAssigned;
    
    [SerializeField]
    [Range(0, 2f)] 
    private float stepDuration = 2f;
    private float _mStepTime;

    private StrategyGridSensorComponent _sensorComp;
    private Vector3Int _gridSize = new Vector3Int(20, 1, 20);

    private int _tasksCompleted;
    
    private SingleChannel _pathChannel;
    
    private void Start()
    {
        ResetMap?.Invoke();
    }

    public override void Initialize()
    {
        _sensorComp = GetComponent<StrategyGridSensorComponent>();

        _gridSize = _sensorComp.gridSize;
        _pathChannel = new SingleChannel(_gridSize.x, _gridSize.z, 2);
        _sensorComp.ExternalChannel = _pathChannel;
        
        _taskComplete = true;
        _taskAssigned = false;
        
        _mIsTraining = Academy.Instance.IsCommunicatorOn;
    }
    
    
    public override void CollectObservations(VectorSensor sensor)
    {
    }
    
    public override void OnEpisodeBegin()
    {
        if (_sensorComp.GridBuffer.CountLayer(0, 0) < 1)
        {
            ResetMap?.Invoke();
        }
        
        _pathChannel.Clear();
        //_pathChannel.ResetMinorGrid();

        if (_taskAssigned)
        {
            _pathChannel.ResetMinorGrid();
        }

        _mStepTime = 0;
    }

    
    private void TaskComplete(int hitIndex)
    {
        var cellPos = _sensorComp.GetCellPosition(hitIndex);
        if (!positions.positions.Contains(cellPos))
        {
            if (!positions.positions.Any(x => Vector3.Distance(x, cellPos) < 1.5f))
            {
                var relative = owner.InverseTransformPoint(cellPos);
                positions.positions.Enqueue(relative);
            }
        }
        _taskComplete = true;
        _taskAssigned = false;
        AddReward(1.0f);
        EndEpisode();
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        var action = actions.DiscreteActions[0];
        
        var notLast = _pathChannel.GetNewGridShape(action);
        var size = _pathChannel.SmallGridSize;
        var theIndex = _pathChannel.MinorMin;
        var startIndex = theIndex.z * size + theIndex.x;
        var hits = _sensorComp.GridBuffer.ReadFromGrid(_pathChannel.MinorMin, _pathChannel.SmallGridSize, 0);
        
        if (!notLast)
        {
            if (_sensorComp.GridBuffer.Read(0, startIndex) > 0)
            {
                TaskComplete(startIndex);
            }
            else
            {
                AddReward(-1.0f);
                EndEpisode();
            }
        }
        else
        {
            if (hits > 0 && size < 20)
            {
                AddReward(1.0f);
            }
            else if(hits <= 0)
            {
                AddReward(-1.0f);
            }
        }


    }

    private bool TryGetTask()
    {
        if (positions.positions.Count > 3) return false;
        _pathChannel.ResetMinorGrid();
        _taskComplete = false;
        _taskAssigned = true;
        return true;
    }
    
    private void FixedUpdate()
    {
        if (stepDuration > 0)
        {
            _mStepTime += Time.fixedDeltaTime;
            _mIsActive = _mStepTime >= stepDuration;
        }

        if (_taskComplete && !_taskAssigned)
        {
            TryGetTask();
        }

        if (_mIsActive && _taskAssigned)
        {
            _mStepTime = 0;
            RequestDecision();
        }
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        if (Input.GetKey(KeyCode.Alpha0))
        {
            discreteActionsOut[0] = Zero;
        }
        if (Input.GetKey(KeyCode.Alpha1))
        {
            discreteActionsOut[0] = One;
        }
        if (Input.GetKey(KeyCode.Alpha2))
        {
            discreteActionsOut[0] = Two;
        }
        if (Input.GetKey(KeyCode.Alpha3))
        {
            discreteActionsOut[0] = Three;
        }
        if (Input.GetKey(KeyCode.Alpha4))
        {
            discreteActionsOut[0] = Revert;
        }
    }
}
