using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class SingleAgent : Agent
{
     public Rigidbody rBody;
     public Transform startTrans;
     public AssetSpawner assetSpawner;

     public override void OnEpisodeBegin()
     {
         transform.localPosition = startTrans.localPosition;
         assetSpawner.SpawnCollect();
     }
   
     public override void CollectObservations(VectorSensor sensor)
     {
         sensor.AddObservation(transform.localPosition);
     }
   
     public float speed = 10;
     public override void OnActionReceived(ActionBuffers actions)
     {
         AddReward(-0.005f);
         Vector3 controlSignal = Vector3.zero;
         controlSignal.x =  actions.ContinuousActions[0];
         controlSignal.z = actions.ContinuousActions[1];
         transform.localPosition += controlSignal / 40;

         if (transform.localPosition.y < -1)
         {
             SetReward(-1.0f);
             EndEpisode();
         }
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
         if (collision.gameObject.TryGetComponent<Collectable>(out var obstacle))
         {
             SetReward(1.0f);
             EndEpisode();
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
