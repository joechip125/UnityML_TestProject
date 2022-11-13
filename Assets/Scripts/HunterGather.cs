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
    public int HitCount;
    public float totalRotation;
    public float rotateDiff;
    public float lastRot;
    private float _oldDistance;
    private bool haveObject;
    public Transform endZone;
    
    
    public override void OnEpisodeBegin()
    {
        transform.localPosition = dropOff.localPosition;
        transform.localRotation = Quaternion.identity;
        lastRot = transform.localRotation.y;
        if (HitCount > 4) HitCount = 0;
        spawner.SpawnSingle(HitCount, 10);
        _haveCollect = false;
        totalRotation = 0;
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
            AddReward(-0.02f);
        }
        if (collision.gameObject.CompareTag("Collectable"))
        {
            Destroy(collision.gameObject);
            _haveCollect = true;
            SetReward(1.0f);
        }   
        if (collision.gameObject.CompareTag("EndZone"))
        {
            if (_haveCollect)
            {
                SetReward(1f);
                EndEpisode();
            }
            else
            {
                AddReward(-0.02f);
            }
        }   
    }

    private void OnCollisionStay(Collision collisionInfo)
    {
        if (collisionInfo.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.02f);
        }
        
        if (collisionInfo.gameObject.CompareTag("EndZone"))
        {
            if (!_haveCollect)
            {
                AddReward(-0.02f);
            }
        }   
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        var localVelocity = transform.InverseTransformDirection(_rBody.velocity);
        sensor.AddObservation(localVelocity.x);
        sensor.AddObservation(localVelocity.z);
        sensor.AddObservation(transform.rotation);
    }

    private float moveSpeed = 0.05f;
    private float turnSpeed = 8f;
    public override void OnActionReceived(ActionBuffers actions)
    {
        AddReward(-0.005f);

        var forward = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        var right = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        var rotate = Mathf.Clamp(actions.ContinuousActions[2], -1f, 1f);

        var dirToGo = transform.forward * forward;
        dirToGo += transform.right * right;
        var rotateDir = -transform.up * rotate;

        transform.localPosition += dirToGo * moveSpeed;
        
        
        //_rBody.AddForce(dirToGo * moveSpeed, ForceMode.VelocityChange);
        transform.Rotate(rotateDir, Time.fixedDeltaTime * turnSpeed, Space.Self);

        if (transform.localPosition.y < -1)
        {
            SetReward(-1f);
            EndEpisode();
        }

        lastRot = transform.rotation.eulerAngles.y;
        var dist = Vector3.Distance(transform.localPosition, endZone.localPosition);
        
        if (dist < _oldDistance)
        {
            if(_haveCollect) AddReward(0.008f);
        }
        _oldDistance = dist;
    }
}
