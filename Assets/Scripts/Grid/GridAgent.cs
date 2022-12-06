using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using MBaske.Sensors.Grid;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

[Serializable]
public class UnitStore
{
    public UnitStore(Vector2Int index, Guid unitGuid)
    {
        unitLocation = index;
        Guid = unitGuid;
    }
    
    public Vector2Int unitLocation;
    public Guid Guid;
}

public class GridAgent : Agent
{
    public event Action EpisodeBegin;
    public event Action<Vector3, Guid> NeedDirectionEvent;
    public Controller Controller;
    public SpawnArea spawnArea;

    private UnitStore _currentUnit;
    private Guid _currentGuid;
    private Vector2Int _currentStartIndex;

    [SerializeField]
    [Tooltip("Select to enable action masking. Note that a model trained with action " +
             "masking turned on may not behave optimally when action masking is turned off.")]
    private bool m_MaskActions;

    private const int c_Stay = 0; 
    private const int c_Up = 1;
    private const int c_Down = 2;
    private const int c_Left = 3;
    private const int c_Right = 4;

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

    private Queue<UnitStore> _moveQueue = new();


    public event Action<Vector2Int> FoundFoodEvent;

    [SerializeField]
    [Tooltip("The number of grid cells the agent can observe in any cardinal direction. " +
             "The resulting grid observation will always have odd dimensions, as the agent " +
             "is located at its center position, e.g. radius = 10 results in grid size 21 x 21.")]
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
        m_IsTraining = Academy.Instance.IsCommunicatorOn;
        m_ValidActions = new List<int>(5);
        this.Controller.NeedDirectionEvent += OnGetMove;

        m_Directions = new Vector2Int[]
        {
            Vector2Int.zero,
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        int length = m_LookDistance * 2 + 1;
        // The ColorGridBuffer supports PNG compression.
        m_SensorBuffer = new ColorGridBuffer(5, 20, 20);

        var sensorComp = GetComponent<StrategyGridSensorComponent>();
        sensorComp.ExternalBuffer = m_SensorBuffer;
        //// Labels for sensor debugging.
        //sensorComp.ChannelLabels = new List<ChannelLabel>()
        //{
        //    new ChannelLabel("Wall", new Color32(0, 128, 255, 255)),
        //    new ChannelLabel("Food", new Color32(64, 255, 64, 255)),
        //    new ChannelLabel("Visited", new Color32(255, 64, 64, 255)),
        //};
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
    }
    
    public override void OnEpisodeBegin()
    {
        spawnArea.RespawnCollection();
        m_SensorBuffer.Clear();
        m_GridPosition = Vector2Int.zero;
        m_StepTime = 0;
    }

    private void OnGetMove(Vector2 pos, Guid guid)
    {
        _moveQueue.Enqueue(new UnitStore(m_SensorBuffer.NormalizedToGridPos(pos), guid));
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
        float visitValue = m_SensorBuffer.Read(MyGrid.Visit, m_GridPosition);

        m_SensorBuffer.Write(2, m_GridPosition,
            1);

        //(1, visitValue + m_RewardDecrement)
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
            m_LocalPosNext = new Vector3(m_GridPosition.x, 0, m_GridPosition.y);
            
            isDone = ValidatePosition(true);
        }
        
        else
        {
            AddReward(-1.0f);
        }

        if (isDone)
        {
            m_IsActive = false;
            EndEpisode();
        }
    }

    private bool TryGetTask()
    {
        if (_moveQueue.Count > 0)
        {
            _currentUnit = _moveQueue.Dequeue();
            return true;
        }

        return false;
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
    
    private void UpdateSensorBuffer()
    {
        m_SensorBuffer.Clear();

        // Current FOV.
        int xMin = m_GridPosition.x - m_LookDistance;
        int xMax = m_GridPosition.x + m_LookDistance;
        int yMin = m_GridPosition.y - m_LookDistance;
        int yMax = m_GridPosition.y + m_LookDistance;

        for (int mx = xMin; mx <= xMax; mx++)
        {
            int sx = mx - xMin;
            for (int my = yMin; my <= yMax; my++)
            {
                int sy = my - yMin;
                // TryRead -> FOV might extend beyond maze bounds.
                if (m_MazeBuffer.TryRead(MyGrid.Collectable, mx, my, out float wall))
                {
                    // Copy maze -> sensor.
                    m_SensorBuffer.Write(MyGrid.Wall, sx, sy, wall);
                    m_SensorBuffer.Write(MyGrid.Collectable, sx, sy, m_MazeBuffer.Read(MyGrid.Collectable, mx, my));
                    m_SensorBuffer.Write(MyGrid.Visit, sx, sy, m_MazeBuffer.Read(MyGrid.Visit, mx, my));
                }
            }
        }
    }
    
}
