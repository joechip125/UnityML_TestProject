using System;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class HunterGather : Agent
{
    private BufferSensorComponent BufferComponent ;
    private Rigidbody rBody;
    public List<Transform> foundObjects;

    private void Start()
    {
        BufferComponent = GetComponent<BufferSensorComponent>();
        rBody = GetComponent<Rigidbody>();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x =  actions.ContinuousActions[0];
        controlSignal.z = actions.ContinuousActions[1];
        transform.localPosition += controlSignal / 40;
    }
}
