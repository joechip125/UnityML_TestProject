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
    
    public Controller controller;
    
    public PositionStore positions;

    [SerializeField]
    private bool maskActions;
    
    private const int CUp = 0;
    private const int CDown = 1;
    private const int CRight = 2;
    private const int CLeft = 3;
    
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
    
    private SingleChannel _pathChannel;

    public Transform startPoint;

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
        
        _taskComplete = true;
        _taskAssigned = false;
        
        _mIsTraining = Academy.Instance.IsCommunicatorOn;
        _mValidActions = new List<int>(5);

        _mDirections = new Vector2Int[]
        {
            //Vector2Int.zero,
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
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

        if (_taskAssigned)
        {
            _mGridPosition = new Vector2Int(0,0);
            _mLocalPosNext = startPoint.localPosition;
        }

        _mStepTime = 0;
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        _mValidActions.Clear();

        for (int action = 0; action < 4; action++)
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
            AddReward(0.5f - visitValue);

            if (_sensorComp.GridBuffer.Read(0, _mGridPosition) > 0f)
            {
                //AddReward(0.5f - visitValue);
                TaskCompleted();
            }
        }
        
        return visitValue == 1;
    }

    private void TaskCompleted()
    {
        if (!positions.positions.Contains(_mLocalPosNext))
        {
            if (!positions.positions.Any(x => Vector3.Distance(x, _mLocalPosNext) < 1))
            {
                positions.positions.Enqueue(_mLocalPosNext);
            }
        }

        _taskComplete = true;
        _taskAssigned = false;
        AddReward(1.0f);
        EndEpisode();
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        bool isDone = false;
        var action = actions.DiscreteActions[0];
       // AddReward(-0.005f);

        if (_mValidActions.Contains(action))
        {
            if(!_pathChannel.GetNewGridShape(action, out var theIndex))
            {
                if (_sensorComp.GridBuffer.Read(0, theIndex) > 0)
                {
                    SetReward(1.0f);
                    var cellPos = _sensorComp.GetCellPosition(theIndex);
                    if (!positions.positions.Contains(cellPos))
                    {
                        if (!positions.positions.Any(x => Vector3.Distance(x, _mLocalPosNext) < 1))
                        {
                            positions.positions.Enqueue(_mLocalPosNext);
                        }
                    }
                    EndEpisode();
                }
            }
            _mGridPosition += _mDirections[action];
            _mLocalPosNext += new Vector3(_mDirections[action].x, 0,_mDirections[action].y);
            
            isDone = ValidatePosition(true);
            
        }
        else
        {
            AddReward(-1);
            
            isDone = ValidatePosition(false);
        }
        
        if (isDone)
        {
           
            EndEpisode();
        }
    }

    private bool TryGetTask()
    {
        if (positions.positions.Count > 7) return false;
        
        _mGridPosition = new Vector2Int(0,0);
        _mLocalPosNext = startPoint.localPosition;
        
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
