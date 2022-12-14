using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Integrations.Match3;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class SingleAgent : Agent
{
     public Rigidbody rBody;
     public Transform startTrans;
     public SpawnArea assetSpawner;
     private int _numberCollect;
     private int _numberPoison;
     private int _wallHits;
     private UnitMovement _movement;
     private List<Vector3> _possibleVectors = new();
     private bool _canMove;

     public override void OnEpisodeBegin()
     {
         transform.localPosition = startTrans.localPosition;
         
         _numberCollect = 0;
         _numberPoison = 0;
         _wallHits = 0;
         assetSpawner.RespawnCollection();
         RequestDecision();
     }
   
     
     public override void CollectObservations(VectorSensor sensor)
     {
         //sensor.AddObservation(_numberCollect);
         //sensor.AddObservation(transform.localPosition);
         var localVelocity = transform.InverseTransformDirection(rBody.velocity);
         sensor.AddObservation(localVelocity.x);
         sensor.AddObservation(localVelocity.z);
     }
   
     public float speed = 10;
     private float _range = 3;
     public override void OnActionReceived(ActionBuffers actions)
     {
         AddReward(-0.005f);
         //MoveCont(actions.ContinuousActions);
         //MoveFoodCollector(actions);
         MoveAbstract(actions.ContinuousActions);

         if (transform.localPosition.y < -1f)
         {
             SetReward(-1.0f);
             EndEpisode();
         }
     }

     public Vector3 GetAverageVector()
     {
         var vec = _possibleVectors
             .Aggregate(Vector3.zero, (acc, v) => acc + v) 
                   / _possibleVectors.Count;
         
         Debug.Log(vec.normalized);
         _possibleVectors.Clear();
         return vec.normalized;
     }

     public void MoveAbstract(ActionSegment<float> actions)
     {
         var con1 = Mathf.Clamp(actions[0] * _range, -_range, _range);
         var con2 = Mathf.Clamp(actions[1]* _range, -_range, _range);
         var next = transform.localPosition + new Vector3(con1, 0, con2);
         if (next.z is > -3 and < 33f && next.x is > -3 and < 33f)
         {
             _movement.Goal = next;
         }
         else
         {
             AddReward(-0.01f);
             RequestDecision();
         }
     }
     
     public void MoveCont(ActionSegment<float> actions)
     {
         var rotateDir = transform.up * Mathf.Clamp(actions[0], -1, 1);
         var con1 = Mathf.Clamp(actions[1], -1, 1);
         Vector3 dirToGo = transform.forward * con1;;
         transform.Rotate(rotateDir, Time.deltaTime * 15f);
         transform.localPosition += dirToGo / 15;
     }
     
     public float turnSpeed = 30;
     public float moveSpeed = 1;
     private void MoveFoodCollector(ActionBuffers actionBuffers)
     {
         var continuousActions = actionBuffers.ContinuousActions;
         var dirToGo = Vector3.zero;
         var rotateDir = Vector3.zero;

         var forward = Mathf.Clamp(continuousActions[0], -1f, 1f);
         var right = Mathf.Clamp(continuousActions[1], -1f, 1f);
         var rotate = Mathf.Clamp(continuousActions[2], -1f, 1f);
         
         dirToGo = transform.forward * forward;
         dirToGo += transform.right * right;
         rotateDir = -transform.up * rotate;
         rBody.AddForce(dirToGo * moveSpeed, ForceMode.VelocityChange);
         transform.Rotate(rotateDir, Time.fixedDeltaTime * turnSpeed);
         
         if (rBody.velocity.sqrMagnitude > 25f) // slow it down
         {
             rBody.velocity *= 0.95f;
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
                 dirToGo = transform.right * 1f;
                 break;
             case 4:
                 dirToGo = transform.right * -1f;
                 break;
             //case 3:
             //    rotateDir = transform.up * 1f;
             //    break;
             //case 4:
             //    rotateDir = transform.up * -1f;
             //    break;
         }
         //transform.Rotate(rotateDir, Time.deltaTime * 5f);
         //transform.localPosition += dirToGo / 15;
         rBody.AddForce(dirToGo * 0.5f, ForceMode.VelocityChange);
         if (rBody.velocity.sqrMagnitude > 25f) // slow it down
         {
             rBody.velocity *= 0.95f;
         }
     }
     
     
     private void OnCollisionEnter(Collision collision)
     {
         if (collision.gameObject.CompareTag("Wall"))
         {
             //AddReward(-1f);
             _wallHits++;
         }
     }

     private void OnCollisionStay(Collision collisionInfo)
     {
         if (collisionInfo.gameObject.CompareTag("Wall"))
         {
             //AddReward(-1f);
             _wallHits++;
         }
     }

     private void OnTriggerEnter(Collider other)
     {
         if (other.CompareTag("Collectable"))
         {
             AddReward(1f);
             other.gameObject.GetComponent<Collectable>().Deactivate();
             _numberCollect++;
         }

         if (other.CompareTag("Poison"))
         {
             AddReward(-1f);
             other.gameObject.GetComponent<Collectable>().Deactivate();
             _numberPoison++;
         }
     }

     public override void Heuristic(in ActionBuffers actionsOut)
     {
         var act = actionsOut.ContinuousActions;

         act[0] = Input.GetAxis("Horizontal");
         act[1] = Input.GetAxis("Vertical");
     }

     private void GoalReached()
     {
         RequestDecision();
     }
     
     void Start()
     {
         _canMove = true;
         rBody = GetComponent<Rigidbody>();
         _movement = GetComponent<UnitMovement>();
         _movement.MoveComplete += GoalReached;
     }
    
}
