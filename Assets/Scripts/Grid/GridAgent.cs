using System;
using System.Collections;
using System.Collections.Generic;
using MBaske.Sensors.Grid;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class GridAgent : Agent
{
    public event Action EpisodeBegin;
    public SpawnArea spawnArea;

    [SerializeField]
    [Tooltip("Select to enable action masking. Note that a model trained with action " +
             "masking turned on may not behave optimally when action masking is turned off.")]
    private bool m_MaskActions;

    private const int c_Stay = 0; 
    private const int c_Up = 1;
    private const int c_Down = 2;
    private const int c_Left = 3;
    private const int c_Right = 4;

    private GridBuffer m_SensorBuffer;
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
        m_SensorBuffer = new ColorGridBuffer(3, length, length);

        //var sensorComp = GetComponent<GridSensorComponentNew>();
        //sensorComp.GridBuffer = m_SensorBuffer;
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
        EpisodeBegin?.Invoke();
        //RequestDecision();
        spawnArea.RespawnCollection();
        m_StepTime = 0;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {   
        //AddReward(-0.005f);
        
        
        var continuousActions = actions.ContinuousActions;
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var forward = Mathf.Clamp(continuousActions[0], -1f, 1f);
        var right = Mathf.Clamp(continuousActions[1], -1f, 1f);
        var rotate = Mathf.Clamp(continuousActions[2], -1f, 1f);
         
       // dirToGo = transform.forward * forward;
       // dirToGo += transform.right * right;
       // rotateDir = -transform.up * rotate;

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
            // Wait one step before activating.
            m_IsActive = true;
        }
    }
    
    
}
