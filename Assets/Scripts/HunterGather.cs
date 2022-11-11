﻿using System;
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
    private BufferSensorComponent _bufferComponent ;
    private Rigidbody _rBody;
    public List<Transform> foundObjects;
    public GameObject collectProto;
    private GameObject _heldCollect;
    private bool _haveCollect;
    public Transform dropOff;
    private RayPerceptionSensorComponent3D _perception;
    public CollectSpawner spawner;


    public override void OnEpisodeBegin()
    {
        transform.localPosition = dropOff.localPosition + new Vector3(0, 0.3f, 0);
        spawner.Respawn(6, 6);
    }

    private void Start()
    {
        _bufferComponent = GetComponent<BufferSensorComponent>();
        _rBody = GetComponent<Rigidbody>();
        _heldCollect = Instantiate(collectProto, 
            transform.localPosition +  new Vector3(0, 0.7f, 0), Quaternion.identity,
            transform);
        _heldCollect.SetActive(false);
        _haveCollect = false;
        _perception = GetComponent<RayPerceptionSensorComponent3D>();

        transform.localPosition = dropOff.localPosition + new Vector3(0, 0.3f, 0);
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Collectable"))
        {
            collision.gameObject.GetComponent<Collectable>().Deactivate();
            SetReward(1f);
            _haveCollect = true;
            EndEpisode();
           // _heldCollect.SetActive(true);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.forward);
        sensor.AddObservation(transform.localRotation.y);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        AddReward(-0.005f);
        Vector3 controlSignal = Vector3.zero;
        transform.localPosition += transform.forward *Mathf.Clamp(actions.ContinuousActions[0], 0 ,1) / 40;
        transform.Rotate(Vector3.up,actions.ContinuousActions[1], Space.Self);

        if (transform.localPosition.y < 0)
        {
            SetReward(-1f);
            EndEpisode();
        }

        if (_haveCollect)
        {
            var dist = Vector3.Distance(transform.localPosition, dropOff.localPosition);
            if (dist < 0.5f)
            {
                _heldCollect.SetActive(false);
                SetReward(1f);
                EndEpisode();
            }
        }
    }
}
