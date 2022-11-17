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
    private int _unitScore;
    private IUnitControlInterface _controlInterface;
    
    
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
    }

    public override void OnEpisodeBegin()
    {
        _controlInterface = unitSpawner.SpawnNewUnit();
        _unitScore = 0;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        Vector3 controlSignal = Vector3.zero;
        
        controlSignal.x =  actions.ContinuousActions[0];
        controlSignal.z = actions.ContinuousActions[1];
        controlSignal.y = 0.5f;
        
        _controlInterface.MoveToLocation(controlSignal);
        var tempScore = _controlInterface.GetUnitScore();

        if (_controlInterface.GetLocation().y < -1)
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
