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
    
    public Controller Controller;

    public UnitStore unitStore;
    private UnitValues _currentUnit;

    [SerializeField]
    [Tooltip("Select to enable action masking. Note that a model trained with action " +
             "masking turned on may not behave optimally when action masking is turned off.")]
    private bool m_MaskActions;

    private const int c_Stay = 0; 
    private const int c_Up = 1;
    private const int c_Down = 2;
    private const int c_Right = 3;
    private const int c_Left = 4;

    private ColorGridBuffer m_SensorBuffer;
    private ColorGridBuffer m_ReadBuffer;

    // Current agent position on grid.
    private Vector2Int m_GridPosition;
    private Vector3 m_LocalPosNext;
    private Vector3 m_LocalPosPrev;
    private List<int> m_ValidActions;
    private Vector2Int[] m_Directions;
        
    private bool m_IsTraining;
    // Whether the agent is currently requesting decisions.
    // Agent is inactive during animation at inference.
    private bool m_IsActive;
    private bool _taskComplete;
    private bool _taskAssigned;
    
    [SerializeField]
    private int m_LookDistance = 10;

    [SerializeField]
    [Tooltip("Amount by which rewards diminish for staying on, or repeat visits to grid " +
             "positions. Initial reward is 0.5 for every move onto a position the agent hasn't" +
             "visited before. Episodes end when rewards drop to -0.5 on any position.")]
    [Range(0, 1)] 
    private float m_RewardDecrement = 0.25f;

    [SerializeField]
    [Tooltip("The animation duration for every agent step at inference.")]
    [Range(0, 2f)] 
    private float m_StepDuration = 2f;
    private float m_StepTime;

    private StrategyGridSensorComponent sensorComp;
    private Vector3 m_CellCenterOffset;
    private Vector3Int gridSize = new Vector3Int(20, 1, 20);
    private int _currentIndex;

    private void Start()
    {
        EpisodeBegin?.Invoke();
    }

    public override void Initialize()
    {
        m_CellCenterOffset = new Vector3((gridSize.x - 1f) / 2, 0, (gridSize.z - 1f) / 2);
        _taskComplete = true;
        _taskAssigned = false;
        m_IsTraining = Academy.Instance.IsCommunicatorOn;
        m_ValidActions = new List<int>(5);

        m_Directions = new Vector2Int[]
        {
            Vector2Int.zero,
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };
        
        m_SensorBuffer = new ColorGridBuffer(3, 20, 20);

        sensorComp = GetComponent<StrategyGridSensorComponent>();
        sensorComp.ExternalBuffer = m_SensorBuffer;
    }
    
    public Vector2Int GetCellIndexFromPosition(Vector3 pos)
    {
        var comb = (transform.position - pos) - m_CellCenterOffset;
        return new Vector2Int(Mathf.RoundToInt(Mathf.Abs(comb.z)), Mathf.RoundToInt(Mathf.Abs(comb.x)));
    }
    
    public int GetIntIndexFromPosition(Vector3 pos)
    {
        var comb = (transform.position - pos) - m_CellCenterOffset;
        var perm = new Vector2Int(Mathf.RoundToInt(Mathf.Abs(comb.x)), Mathf.RoundToInt(Mathf.Abs(comb.z)));
        var result = perm.x * gridSize.z + perm.y;
        //Debug.Log($"{comb},{perm}, {result}");
        return result;
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
    }

    private int NextDirection(int direction)
    {
        switch (direction)
        {
            case 1:
                break;
            case 2:
                return _currentIndex + 1;
            case 3:
                return _currentIndex - 1;
            case 4:
                return _currentIndex + gridSize.z;
            case 5:
                return _currentIndex - gridSize.z;
        }

        return _currentIndex;
    }
    
    public override void OnEpisodeBegin()
    {
        unitStore ??= Controller._unitStore;
        _currentIndex = 0;
        
        m_SensorBuffer.Clear();
        if (_taskComplete && !_taskAssigned)
        {
            TryGetTask();
        }
        else if(_taskAssigned && !_taskComplete)
        {
            m_GridPosition = GetCellIndexFromPosition(_currentUnit.unitPos);
            _currentIndex = GetIntIndexFromPosition(_currentUnit.unitPos);
            m_LocalPosNext = _currentUnit.unitPos;
        }
        
        m_StepTime = 0;
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        m_ValidActions.Clear();
        m_ValidActions.Add(c_Stay);
        
        for (int action = 1; action < 5; action++)
        {
            bool isValid = m_SensorBuffer.Contains( 
                m_GridPosition.x + m_Directions[action].x,
                m_GridPosition.y + m_Directions[action].y);

            if (isValid)
            {
                m_ValidActions.Add(action);
            }
            else if (m_MaskActions)
            {
                actionMask.SetActionEnabled(0, action, false);
            }
        }
    }
    
    private bool ValidatePosition(bool rewardAgent)
    {
        // From 0 to +1. 
        float visitValue = m_SensorBuffer.Read(2, m_GridPosition);
        
        m_SensorBuffer.Write(2, m_GridPosition,
            Mathf.Min(1, visitValue + m_RewardDecrement));
        
        if (rewardAgent)
        {
            // From +0.5 to -0.5.
            AddReward(0.5f - visitValue);

            if (sensorComp.GridBuffer.Read(0, m_GridPosition) > 0f)
            {
                Debug.Log(m_GridPosition);
                Debug.Log(m_LocalPosNext);
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
        _currentUnit.CallBack.Invoke(m_LocalPosNext);
        _taskComplete = true;
        _taskAssigned = false;
        AddReward(1);
        EndEpisode();
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        bool isDone = false;
        var action = actions.DiscreteActions[0];
        m_LocalPosNext += new Vector3(m_Directions[action].x, 0,m_Directions[action].y);

        if (m_ValidActions.Contains(action))
        {
            m_GridPosition += m_Directions[action];
            //_currentIndex += NextDirection(action);
        
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
            _currentUnit.CallBack.Invoke(m_LocalPosNext);
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
        
        if (m_IsActive && _taskAssigned)
        {
            RequestDecision();
        }
        else if (m_StepDuration > 0)
        {
            m_StepTime += Time.fixedDeltaTime;
            m_IsActive = m_StepTime >= m_StepDuration;
        }
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = c_Stay;

        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = c_Right;
        }
        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = c_Up;
        }
        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = c_Left;
        }
        if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = c_Down;
        }
    }
}
