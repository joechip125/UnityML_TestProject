using System;
using System.Collections.Generic;
using System.Numerics;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class HunterGather : Agent
{
    private BufferSensorComponent BufferComponent ;
    private Rigidbody rBody;
    public List<Transform> foundObjects;
    public GameObject CollectProto;
    private GameObject heldCollect;

    private void Start()
    {
        BufferComponent = GetComponent<BufferSensorComponent>();
        rBody = GetComponent<Rigidbody>();
        heldCollect = Instantiate(CollectProto, 
            transform.localPosition +  new Vector3(0, 0.7f, 0), Quaternion.identity,
            transform);
        heldCollect.SetActive(false);
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
