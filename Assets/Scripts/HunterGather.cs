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
        transform.localPosition = dropOff.localPosition;
        Debug.Log(dropOff.localPosition);
        Debug.Log(transform.localPosition);
        spawner.Respawn(6, 6);
        _haveCollect = false;
    }

    private void Start()
    {
        _rBody = GetComponent<Rigidbody>();
        _haveCollect = false;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            SetReward(-1f);
            EndEpisode();
        }
        if (collision.gameObject.CompareTag("Collectable"))
        {
            Destroy(collision.gameObject);
            SetReward(1f);
            _haveCollect = true;
            EndEpisode();
        }   
    }
    

    public override void CollectObservations(VectorSensor sensor)
    {
        var localVelocity = transform.InverseTransformDirection(_rBody.velocity);
        sensor.AddObservation(localVelocity.x);
        sensor.AddObservation(localVelocity.z);
    }

    private float moveSpeed = 0.4f;
    private float turnSpeed = 0.4f;
    public override void OnActionReceived(ActionBuffers actions)
    {
        AddReward(-0.005f);
        
        var forward = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        var right = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        var rotate = Mathf.Clamp(actions.ContinuousActions[2], -1f, 1f);
        
        var dirToGo = transform.forward * forward;
        dirToGo += transform.right * right;
        var rotateDir = -transform.up * rotate;
        
        
        _rBody.AddForce(dirToGo * moveSpeed, ForceMode.VelocityChange);
        transform.Rotate(rotateDir, Time.fixedDeltaTime * turnSpeed); 
       
        if (transform.localPosition.y < -1)
        {
            SetReward(-1f);
            EndEpisode();
        }

       // if (_haveCollect)
       // {
       //     var dist = Vector3.Distance(transform.localPosition, dropOff.localPosition);
       //     if (dist < 0.5f)
       //     {
       //         SetReward(1f);
       //         EndEpisode();
       //     }
       // }
    }
}
