using System.Collections;
using System.Collections.Generic;
using Interfaces;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class RTSAgent : Agent
{
    public UnitSpawner unitSpawner;
    public AssetSpawner assetSpawner;
    private int _unitScore;
    private IUnitControlInterface _controlInterface;
    private Vector3 _unitLocation;
    public Transform start;
    public SpawnArea spawnArea;
    public float rewardRange = 0.05f;
    public float currentReward;
    public float rewardIncrement = 0.005f;
    
    
    public override void CollectObservations(VectorSensor sensor)
    {
       // sensor.AddObservation(currentReward);
       // sensor.AddObservation(_unitLocation);
    }

    public override void OnEpisodeBegin()
    {
        currentReward = 0.00f;
        _unitLocation = transform.localPosition;
        spawnArea.RespawnCollect();
    }

    public float range = 1;
    private Rigidbody _mAgentRb;
    private float _agentRunSpeed;

    public override void OnActionReceived(ActionBuffers actions)
    {
        var choice = ChooseMove(actions.DiscreteActions);
        var result = spawnArea.CheckTile(choice);
        var localPos = transform.localPosition+ new Vector3(0, 1,0);
        var nextPos = new Vector3(choice.x, 1, choice.z) * 0.2f;

        _unitLocation += nextPos;
        
        Debug.DrawLine(_unitLocation+ new Vector3(0, 1,0), 
            nextPos, Color.green, 1f);

        if (result)
        {
            currentReward += rewardIncrement;
        }
        else
        {
            currentReward -= rewardIncrement;
        }

        currentReward = Mathf.Clamp(currentReward, -rewardRange, rewardRange);
        
        AddReward(currentReward);

        if (GetCumulativeReward() >= 1)
        {
            SetReward(1.0f);
            EndEpisode();
        }

        if (GetCumulativeReward() <= -1)
        {
            SetReward(-1.0f);
            EndEpisode();
        }
    }


    public IntVector2 ChooseMove(ActionSegment<int> act)
    {
        var action = act[0];
        var action2 = act[1];
        var moveDir = new IntVector2();
        
        switch (action)
        {
            case 1:
                moveDir.x = 0;
                break;
            case 2:
                moveDir.x = 1;
                break;
        }
        
        switch (action2)
        {
            case 1:
                moveDir.z = 0;
                break;
            case 2:
                moveDir.z = 1;
                break;
        }
        
        return moveDir;
    }
    
    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var action = act[0];
        switch (action)
        {
            case 1:
                dirToGo = transform.forward * 1f;
                break;
            case 2:
                dirToGo = transform.forward * -1f;
                break;
            case 3:
                rotateDir = transform.up * 1f;
                break;
            case 4:
                rotateDir = transform.up * -1f;
                break;
        }
        transform.Rotate(rotateDir, Time.deltaTime * 150f);
        _mAgentRb.AddForce(dirToGo * _agentRunSpeed, ForceMode.VelocityChange);
    }
    
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
