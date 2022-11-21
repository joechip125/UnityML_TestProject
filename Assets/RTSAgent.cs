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
    
    
    public override void CollectObservations(VectorSensor sensor)
    {
      //  sensor.AddObservation(_unitLocation);
    }

    public override void OnEpisodeBegin()
    {
        spawnArea.RespawnCollect();
       // _controlInterface?.DestroyUnit();
       // assetSpawner.SpawnCollect();
       // _controlInterface = unitSpawner.SpawnNewUnit(transform.position, transform);
       // _unitScore = 0;
    }

    public float range = 1;
    private Rigidbody _mAgentRb;
    private float _agentRunSpeed;

    public override void OnActionReceived(ActionBuffers actions)
    {
        var choice = Mathf.Clamp(actions.DiscreteActions[0], 0, 3);

        var result = spawnArea.CheckTile(choice);

        if (result)
        {
            AddReward(0.05f);
        }
        else
        {
            AddReward(-0.05f);
        }

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
