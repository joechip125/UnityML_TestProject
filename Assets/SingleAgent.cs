using System;
using System.Collections;
using System.Collections.Generic;
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
     private int _wallHits;

     public override void OnEpisodeBegin()
     {
         transform.localPosition = startTrans.localPosition;
         _numberCollect = 0;
         _wallHits = 0;
         assetSpawner.RespawnCollection();
     }
   
     public override void CollectObservations(VectorSensor sensor)
     {
         //sensor.AddObservation(_numberCollect);
         //sensor.AddObservation(transform.localPosition);
     }
   
     public float speed = 10;
     public override void OnActionReceived(ActionBuffers actions)
     {
         AddReward(-0.005f);
         MoveAgent(actions.DiscreteActions);
         
         if (transform.localPosition.y < -1f)
         {
             SetReward(-1.0f);
             EndEpisode();
         }

         if (_wallHits > 1)
         {
             SetReward(-1.0f);
             EndEpisode();
         }
         
         if (_numberCollect >= 3)
         {
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
         transform.Rotate(rotateDir, Time.deltaTime * 20f);
         rBody.AddForce(dirToGo * 0.5f, ForceMode.VelocityChange);
     }
     

     private void OnCollisionStay(Collision collisionInfo)
     {
         if (collisionInfo.gameObject.TryGetComponent<Obstacle>(out var obstacle))
         {
             AddReward(-0.3f);
         }
         
     }

     private void OnCollisionEnter(Collision collision)
     {
         if (collision.gameObject.CompareTag("Wall"))
         {
             AddReward(-0.1f);
             _wallHits++;
         }
     }

     private void OnTriggerEnter(Collider other)
     {
         if (other.gameObject.TryGetComponent<Collectable>(out var obstacle))
         {
             AddReward(1f/ 3f);
             obstacle.Deactivate();
             _numberCollect++;
         }
     }


     public override void Heuristic(in ActionBuffers actionsOut)
     {
         var act = actionsOut.ContinuousActions;

         act[0] = Input.GetAxis("Horizontal");
         act[1] = Input.GetAxis("Vertical");
     }
     
     void Start()
     {
         rBody = GetComponent<Rigidbody>();
     }
}
