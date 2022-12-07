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
    private GridBuffer m_MazeBuffer;

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
    private bool taskComplete;
    
    public event Action<Vector2Int> FoundFoodEvent;

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
    [Range(0, 0.5f)] 
    private float m_StepDuration = 0.1f;
    private float m_StepTime;
    
    public override void Initialize()
    {
        taskComplete = true;
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
        
        m_SensorBuffer = new ColorGridBuffer(5, 20, 20);

        var sensorComp = GetComponent<StrategyGridSensorComponent>();
        sensorComp.ExternalBuffer = m_SensorBuffer;
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
    }
    
    public override void OnEpisodeBegin()
    {
        EpisodeBegin?.Invoke();

        unitStore ??= Controller._unitStore;

        m_SensorBuffer.Clear();
        if (taskComplete)
        {
            TryGetTask();
        }
        else
        {
            m_GridPosition = _currentUnit.unitPos;    
        }
        //m_GridPosition = unitStore.unitLocation;
        m_GridPosition = new Vector2Int();
        m_StepTime = 0;
    }

    private void OnGetMove(Vector2 pos, Action<Vector3> callBack)
    {
        //_moveQueue.Enqueue(new UnitStore(m_SensorBuffer.NormalizedToGridPos(pos), callBack));
    }
    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        m_ValidActions.Clear();
        m_ValidActions.Add(c_Stay);
        
        for (int action = 1; action < 5; action++)
        {
            bool isValid = m_SensorBuffer.TryRead(MyGrid.Wall, 
                m_GridPosition + m_Directions[action],
                out float value) && value == 0; // no wall

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

            if (m_SensorBuffer.Read(0, m_GridPosition) > 0.9f)
            {
                //m_LocalPosNext = m_SensorBuffer.Read()
                AddReward(1);    
            }
        }

        if (rewardAgent)
        {
            // From +0.5 to -0.5.
            AddReward(0.5f - visitValue);
        }

        return visitValue == 1;
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        bool isDone = false;
        var action = actions.DiscreteActions[0];
        if (m_ValidActions.Contains(action))
        {
            m_GridPosition += m_Directions[action];

            isDone = ValidatePosition(true);
        }
        
        else
        {
            AddReward(-1.0f);
        }
        
        if (taskComplete)
        {
            TryGetTask();
        }

        if (isDone)
        {
            //TODO Make this work
            //_currentUnit.callBackEvent.Invoke(new Vector3());
            
            m_IsActive = false;
            //EndEpisode();
        }
    }

    private bool TryGetTask()
    {
        if (unitStore.Unit.Count <= 0) return false;
        
        _currentUnit = unitStore.Unit.Dequeue();
        taskComplete = false;
        return true;
    }
    
    private void FixedUpdate()
    {
        if (m_IsActive)
        {
            RequestDecision();
        }
        else if (m_StepDuration > 0)
        {
            m_StepTime += Time.fixedDeltaTime;
            m_IsActive = m_StepTime >= m_StepDuration;
        }

        if (taskComplete)
        {
            if(TryGetTask())
                m_IsActive = true;
        }
        
        else
        {
            if(TryGetTask())
                m_IsActive = true;
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
