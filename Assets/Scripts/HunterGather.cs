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
    private Vector3 lastForward;
    
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
            SetReward(1f);
            HitCount++;
            _haveCollect = true;
            EndEpisode();
        }   
    }

    private void OnCollisionStay(Collision collisionInfo)
    {
        if (collisionInfo.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.02f);
        }
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        var localVelocity = transform.InverseTransformDirection(_rBody.velocity);
        sensor.AddObservation(localVelocity.x);
        sensor.AddObservation(localVelocity.z);
    }

    private float moveSpeed = 0.3f;
    private float turnSpeed = 0.2f;
    public override void OnActionReceived(ActionBuffers actions)
    {
        AddReward(-0.005f);
        var dot = Vector3.Dot(lastForward, transform.forward);

        var forward = Mathf.Clamp(actions.ContinuousActions[0], 0, 1f);
        var right = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        var rotate = Mathf.Clamp(actions.ContinuousActions[2], -1f, 1f);

        var dirToGo = transform.forward * forward;
        dirToGo += transform.right * right;
        var rotateDir = -transform.up * rotate;


        _rBody.AddForce(dirToGo * moveSpeed, ForceMode.VelocityChange);
        transform.Rotate(rotateDir, Time.fixedDeltaTime * turnSpeed);

        rotateDiff =Mathf.DeltaAngle(transform.rotation.eulerAngles.y, lastRot);

        if (Math.Abs(rotateDiff) > 0.5f)
        {
            AddReward(-0.05f);
            Debug.Log(dot);
        }
        if (rotateDir.y != 0)
        {
            totalRotation += Time.fixedDeltaTime * turnSpeed;
            //Debug.Log(totalRotation);
            //Debug.Log((transform.rotation.eulerAngles));
        }
        
        if (totalRotation > 400)
        {
            AddReward(-0.005f);
        }
        
        if (transform.localPosition.y < -1)
        {
            SetReward(-1f);
            EndEpisode();
        }

        lastRot = transform.rotation.eulerAngles.y;
        lastForward = transform.forward;

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
