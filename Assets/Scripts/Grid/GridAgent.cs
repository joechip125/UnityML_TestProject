using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using MBaske.Sensors.Grid;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;


public class GridAgent : Agent
{
    public event Action EpisodeBegin;
    
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

    private ColorGridBuffer _mSensorBuffer;
    private ColorGridBuffer _mReadBuffer;

    // Current agent position on grid.
    private Vector2Int _mGridPosition;
    private Vector3 _mLocalPosNext;
    private Vector3 _mLocalPosPrev;
    private List<int> _mValidActions;
    private Vector2Int[] _mDirections;
        
    private bool _mIsTraining;
    // Whether the agent is currently requesting decisions.
    // Agent is inactive during animation at inference.
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

    private void Start()
    {
        EpisodeBegin?.Invoke();
    }

    public override void Initialize()
    {
        _sensorComp = GetComponent<StrategyGridSensorComponent>();
        
        _mSensorBuffer = new ColorGridBuffer(3, 20, 20);

        _sensorComp.ExternalBuffer = _mSensorBuffer;
        
        _gridSize = _sensorComp.gridSize;
        _mCellCenterOffset = new Vector3((_gridSize.x - 1f) / 2, 0, (_gridSize.z - 1f) / 2);
        
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
        unitStore ??= controller._unitStore;

        _mSensorBuffer.Clear();
        if (_taskComplete && !_taskAssigned)
        {
            TryGetTask();
        }
        else if(_taskAssigned && !_taskComplete)
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
            bool isValid = _mSensorBuffer.Contains( 
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
        float visitValue = _mSensorBuffer.Read(2, _mGridPosition);
        
        _mSensorBuffer.Write(2, _mGridPosition,
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
        
        
        if (_taskComplete && _taskAssigned)
        {
            _currentUnit.CallBack.Invoke(_mLocalPosNext);
            EndEpisode();
        }
    }

    private bool TryGetTask()
    {
        if (unitStore.Unit.Count <= 0) return false;

        _currentUnit = unitStore.Unit.Dequeue();
        _taskComplete = false;
        _taskAssigned = true;
        return true;
    }
    
    private void FixedUpdate()
    {
        if (_taskComplete && !_taskAssigned)
        {
            TryGetTask();
            EndEpisode();
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
