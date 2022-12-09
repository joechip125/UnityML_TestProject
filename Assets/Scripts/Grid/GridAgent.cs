using System;
using System.Collections;
using System.Collections.Generic;
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
    
    public Controller controller;

    public UnitStore unitStore;
    private UnitValues _currentUnit;

    [SerializeField]
    [Tooltip("Select to enable action masking. Note that a model trained with action " +
             "masking turned on may not behave optimally when action masking is turned off.")]
    private bool maskActions;

    private const int CStay = 0; 
    private const int CUp = 1;
    private const int CDown = 2;
    private const int CRight = 3;
    private const int CLeft = 4;

    //private ColorGridBuffer _mSensorBuffer;
    private ColorGridBuffer _mReadBuffer;


    private Vector2Int _mGridPosition;
    private Vector3 _mLocalPosNext;

    private List<int> _mValidActions;
    private Vector2Int[] _mDirections;
        
    private bool _mIsTraining;
    
    private bool _mIsActive;
    private bool _taskComplete;
    private bool _taskAssigned;
    
    [SerializeField]
    [Range(0, 1)] 
    private float rewardDecrement = 0.25f;

    [SerializeField]
    [Range(0, 2f)] 
    private float stepDuration = 2f;
    private float _mStepTime;

    private StrategyGridSensorComponent _sensorComp;
    private Vector3 _mCellCenterOffset;
    private Vector3Int _gridSize = new Vector3Int(20, 1, 20);

    private int _tasksCompleted;

    private TaskState _taskState;

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
        
        _mCellCenterOffset = new Vector3((_gridSize.x - 1f) / 2, 0, (_gridSize.z - 1f) / 2);

        _taskState |= TaskState.Completed;
        _taskComplete = true;
        _taskAssigned = false;
        
        _mIsTraining = Academy.Instance.IsCommunicatorOn;
        _mValidActions = new List<int>(5);

        _mDirections = new Vector2Int[]
        {
            Vector2Int.zero,
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };
    }
    
    public Vector2Int GetCellIndexFromPosition(Vector3 pos)
    {
        var comb = (transform.position - pos) - _mCellCenterOffset;
        return new Vector2Int(Mathf.RoundToInt(Mathf.Abs(comb.x)), Mathf.RoundToInt(Mathf.Abs(comb.z)));
    }
    
    public int GetIntIndexFromPosition(Vector3 pos)
    {
        var comb = (transform.position - pos) - _mCellCenterOffset;
        var perm = new Vector2Int(Mathf.RoundToInt(Mathf.Abs(comb.x)), Mathf.RoundToInt(Mathf.Abs(comb.z)));
        var result = perm.x * _gridSize.z + perm.y;
        return result;
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
        
        unitStore ??= controller._unitStore;

        _pathChannel.Clear();

        if (_taskAssigned)
        {
            _mGridPosition = GetCellIndexFromPosition(_currentUnit.unitPos);
            _mLocalPosNext = _currentUnit.unitPos;
        }

        _mStepTime = 0;
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        _mValidActions.Clear();
        _mValidActions.Add(CStay);
        
        for (int action = 1; action < 5; action++)
        {
            bool isValid = _pathChannel.Contains( 
                _mGridPosition.x + _mDirections[action].x,
                _mGridPosition.y + _mDirections[action].y);

            if (isValid)
            {
                _mValidActions.Add(action);
            }
            else if (maskActions)
            {
                actionMask.SetActionEnabled(0, action, false);
            }
        }
    }
    
    private bool ValidatePosition(bool rewardAgent)
    {
        // From 0 to +1. 
        var visitValue = _pathChannel.Read(_mGridPosition);
        
        _pathChannel.Write(_mGridPosition,
            Mathf.Min(1, visitValue + rewardDecrement));
        
        if (rewardAgent)
        {
            // From +0.5 to -0.5.
            AddReward(0.5f - visitValue);

            if (_sensorComp.GridBuffer.Read(0, _mGridPosition) > 0f)
            {
                Debug.Log(_mGridPosition);
                Debug.Log(_mLocalPosNext);
                TaskCompleted();
            }
        }

        if (rewardAgent)
        {
            // From +0.5 to -0.5.
            AddReward(0.5f - visitValue);
        }

        return visitValue == 1;
    }

    private void TaskCompleted()
    {
        _currentUnit.CallBack.Invoke(_mLocalPosNext);
        _taskComplete = true;
        _taskAssigned = false;
        AddReward(1);
        _tasksCompleted++;
        
        EndEpisode();
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        bool isDone = false;
        var action = actions.DiscreteActions[0];
        _mLocalPosNext += new Vector3(_mDirections[action].x, 0,_mDirections[action].y);

        if (_mValidActions.Contains(action))
        {
            _mGridPosition += _mDirections[action];
            
            isDone = ValidatePosition(true);

            if (isDone)
            {
                EndEpisode();
            }
        }
        
        else
        {
            AddReward(-1.0f);
        }
    }

    private bool TryGetTask()
    {
        if (unitStore.Unit.Count <= 0) return false;
        
        _currentUnit = unitStore.Unit.Dequeue();
        _taskState &= ~TaskState.Completed;
        _taskState |= TaskState.Assigned;
        
        _mGridPosition = GetCellIndexFromPosition(_currentUnit.unitPos);
        _mLocalPosNext = _currentUnit.unitPos;

        if (_tasksCompleted > 2)
        {
            _taskState |= TaskState.MaxCompleted;
        }
        _taskComplete = false;
        _taskAssigned = true;
        return true;
    }
    
    private void FixedUpdate()
    {
        if (_taskComplete && !_taskAssigned)
        {
            TryGetTask();
        }
        
        if (_mIsActive && _taskAssigned)
        {
            _mStepTime = 0;
            RequestDecision();
        }
        else if (stepDuration > 0)
        {
            _mStepTime += Time.fixedDeltaTime;
            _mIsActive = _mStepTime >= stepDuration;
        }
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = CStay;

        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = CRight;
        }
        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = CUp;
        }
        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = CLeft;
        }
        if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = CDown;
        }
    }
}
