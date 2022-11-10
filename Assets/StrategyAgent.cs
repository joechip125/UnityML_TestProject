using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

public class StrategyAgent : Agent
{
     public Transform target;
     public Rigidbody rBody;
     public Transform startTrans;
     public List<Transform> obstacles;
     private float _lastDistance;
     private float _obstacleDist;

     public override void OnEpisodeBegin()
     {
         transform.localPosition = startTrans.localPosition;
         
         foreach (var o in obstacles)
         {
             o.localPosition = new Vector3(Random.value * 8 - 4,
                 0.5f, 2);
         }
         
         target.localPosition = new Vector3(Random.value * 8 - 4,
             0.06f, 4);
     }
   
     public override void CollectObservations(VectorSensor sensor)
     {
         sensor.AddObservation(rBody.velocity.x);
         sensor.AddObservation(rBody.velocity.z);
         sensor.AddObservation(target.localPosition);
         sensor.AddObservation(transform.localPosition);
         
         foreach (var o in obstacles)
         {
             sensor.AddObservation(o.localPosition);
         }
     }
   
     public float speed = 10;
     public override void OnActionReceived(ActionBuffers actions)
     {
         Vector3 controlSignal = Vector3.zero;
         controlSignal.x =  actions.ContinuousActions[0];
         controlSignal.z = actions.ContinuousActions[1];
         transform.localPosition += controlSignal / 40;
         
         //rBody.AddForce(controlSignal * speed);
         var local = transform.localPosition;
         
         var dist = Vector3.Distance(local, target.localPosition);
         var obsDist = Vector3.Distance(local, obstacles[0].localPosition);

         
         if (dist < _lastDistance)
         {
             AddReward(0.02f);
         }
         else
         {
             AddReward(-0.02f);    
         }
         
         if (dist < 0.5f)
         {
             AddReward(2.0f);
             EndEpisode();
         }
         
         if (transform.localPosition.y < -1)
         {
             EndEpisode();
         }

         _obstacleDist = obsDist;
         _lastDistance = dist;
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
         if (collision.gameObject.TryGetComponent<Obstacle>(out var obstacle))
         {
           
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
