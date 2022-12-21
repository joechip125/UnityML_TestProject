using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using DefaultNamespace.Grid;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
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

        _directions = new[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.right,
            Vector2Int.left,
        };
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
        _pathChannel.ResetMinorGrid();
        
        

        if (_taskAssigned)
        {
            _currentIndex = Vector2Int.zero;
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
                Debug.Log(relative);
                positions.positions.Enqueue(relative);
            }
        }

        _taskComplete = true;
        _taskAssigned = false;
        AddReward(1.0f);
        EndEpisode();
    }

    private void CheckIndex()
    {
        
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        for (int i = 0; i < _directions.Length; i++)
        {
            var nextIndex = _currentIndex + _directions[i];
            if (!_pathChannel.Contains(nextIndex))
            {
                actionMask.SetActionEnabled(0, i, false);
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var action = actions.DiscreteActions[0];
        var nextIndex = _currentIndex + _directions[action];
        _pathChannel.Write(nextIndex, 1.0f);
        
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
