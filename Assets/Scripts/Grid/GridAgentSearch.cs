using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using DefaultNamespace.Grid;
using MBaske.Sensors.Grid;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine;

public class GridAgentSearch : Agent
{
    public event Action ResetMap;

    public PositionStore positions;
    [SerializeField] public Transform owner;

    private const int Up = 0;
    private const int Down = 1;
    private const int Right = 2;
    private const int Left = 3;
    private const int Stay = 4;

    private bool _mIsTraining;

    private bool _mIsActive;
    private bool _taskComplete;
    private bool _taskAssigned;

    [SerializeField] [Range(0, 2f)] private float stepDuration = 2f;
    private float _mStepTime;

    private StrategyGridSensorComponent _sensorComp;
    private Vector3Int _gridSize = new Vector3Int(20, 1, 20);

    private int _tasksCompleted;

    private SingleChannel _pathChannel;
    
    private Vector2Int[] _directions;

    private Vector2Int _currentIndex;
    private List<int> _possibleDirections = new();

    [SerializeField] private TensorVis tensorVis;
    private float _rewardDecrement = 0.25f;

    public event Action<GridBuffer> UpdateTensorVis;

    private void Start()
    {
        ResetMap?.Invoke();
    }

    public override void Initialize()
    {
        _sensorComp = GetComponent<StrategyGridSensorComponent>();

        UpdateTensorVis += tensorVis.OnExternalUpdate;

        _gridSize = _sensorComp.gridSize;
        _pathChannel = new SingleChannel(_gridSize.x, _gridSize.z, 2);
        _sensorComp.ExternalChannel = _pathChannel;
        _currentIndex = Vector2Int.zero;
        _taskComplete = true;
        _taskAssigned = false;

        tensorVis.Buffer = _sensorComp.GridBuffer;
        tensorVis.displayChannel = 3;
        
        _mIsTraining = Academy.Instance.IsCommunicatorOn;

        _directions = new[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.right,
            Vector2Int.left,
        };
    }


    private void WriteMask()
    {
        var mask = _sensorComp.MaskChannel;
        var count = _gridSize.x * _gridSize.z;
        _sensorComp.GridBuffer.ClearChannel(3);

        for (int i = 0; i < count; i++)
        {
            var xVal = i % mask.SizeZ;
            var zVal = (i - xVal) / mask.SizeZ;
            
            if (mask.Read(i) > 0)
            {
                _sensorComp.GridBuffer.MaskSelection(3, new Vector2Int(xVal, zVal), 0.3f, 3);
            }
        }
        UpdateTensorVis?.Invoke(_sensorComp.GridBuffer);
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
        _currentIndex = Vector2Int.zero;

        if (_taskAssigned)
        {
           // _currentIndex = Vector2Int.zero;
        }

        _mStepTime = 0;
    }

    private bool CheckIndex(bool rewardAgent)
    {
        float visitValue = _pathChannel.Read( _currentIndex);

        _pathChannel.Write(_currentIndex,
            Mathf.Min(1, visitValue + _rewardDecrement));

        var buffer = _sensorComp.GridBuffer;
        var maskValue = buffer.Read(3, _currentIndex);
        var otherVal = buffer.Read(0, _currentIndex);
        buffer.Write(3, _currentIndex, 0);
        
        if (rewardAgent)
        {
            var total = Mathf.Clamp(maskValue  + (0.5f - visitValue), -1, 1);
            AddReward(total);
        }
        
        UpdateTensorVis?.Invoke(_sensorComp.GridBuffer);

        if (otherVal > 0.5f)
        {
            var index = _currentIndex.y * _gridSize.x + _currentIndex.x;
            _sensorComp.ClearMaskChannel();
            TaskComplete(index);
        }
        return visitValue == 1;
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
    
    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        _possibleDirections.Clear();
        for (int i = 0; i < 4; i++)
        {
            var nextIndex = _currentIndex + _directions[i];
            var possible = _pathChannel.Contains(nextIndex);
            if (possible)
            {
                _possibleDirections.Add(i);
            }
            
            //if (!_pathChannel.Contains(nextIndex))
            //{
            //    actionMask.SetActionEnabled(0, i, false);
            //}
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var done = false;
        if (_sensorComp.GridBuffer.CountLayer(0, 0) < 1)
        {
            ResetMap?.Invoke();
        }
        
        var action = actions.DiscreteActions[0];
        
        if (_possibleDirections.Contains(action))
        {
            _currentIndex += _directions[action];
            _pathChannel.Write(_currentIndex, 1.0f);
            done = CheckIndex(true);
        }
        else
        {
            AddReward(-1.0f);
            done = CheckIndex(false);
        }

        if (done)
        {
            EndEpisode();
        }
    }

    private bool TryGetTask()
    {
        if (positions.positions.Count > 3) return false;
        _taskComplete = false;
        _taskAssigned = true;
        WriteMask();
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

        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = Up;
        }

        if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = Down;
        }

        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = Right;
        }

        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = Left;
        }
    }
}
