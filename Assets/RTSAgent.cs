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
    
    
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(_unitLocation);
    }

    public override void OnEpisodeBegin()
    {
        _controlInterface?.DestroyUnit();
        assetSpawner.SpawnCollect();
        _controlInterface = unitSpawner.SpawnNewUnit(transform.position, transform);
        _unitScore = 0;
    }

    public float range = 1;
    public override void OnActionReceived(ActionBuffers actions)
    {
        AddReward(-0.005f);
        Vector3 controlSignal = Vector3.zero;
        
        controlSignal.x =  actions.ContinuousActions[0] * range;
        controlSignal.z = actions.ContinuousActions[1] * range;
        controlSignal.y = 0.5f;
        _unitLocation = _controlInterface.GetLocation();
        
        _controlInterface.MoveToLocation(controlSignal);
        var tempScore = _controlInterface.GetUnitScore();

        if (_unitLocation.y < -1)
        {
            _controlInterface.DestroyUnit();
            SetReward(-1f);
            EndEpisode();
        }
        
        if (tempScore > _unitScore)
        {
            SetReward(1f);
            _controlInterface.DestroyUnit();
            EndEpisode();
        }
        _unitScore = tempScore;
    }
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
