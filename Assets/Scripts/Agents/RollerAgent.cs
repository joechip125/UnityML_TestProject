using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class RollerAgent : Agent
{

    public Transform target;
    public Rigidbody rBody;
    
    
    public override void OnEpisodeBegin()
    {
        if (transform.localPosition.y < 0)
        {
            rBody.angularVelocity = Vector3.zero;
            rBody.velocity = Vector3.zero;
            transform.localPosition = new Vector3( 0, 0.5f, 0);
        }
        
        target.localPosition = new Vector3(Random.value * 8 - 4,
            0.5f,
            Random.value * 8 - 4);
        
        //Debug.Log($"{target.localPosition}, {transform.localPosition}, {rBody.velocity}");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        
        sensor.AddObservation(target.localPosition);
        sensor.AddObservation(transform.localPosition);

        // Agent velocity
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);
        
        Debug.Log($"{target.localPosition}, {transform.localPosition}, {rBody.velocity}");
    }

    public float speed = 10;
    public override void OnActionReceived(ActionBuffers actions)
    {
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x =  actions.ContinuousActions[0];
        controlSignal.z = actions.ContinuousActions[1];

        //controlSignal.x =  actions.DiscreteActions[0];
        //controlSignal.z = actions.DiscreteActions[0];
        
        rBody.AddForce(controlSignal * speed);

        float distanceToTarget = Vector3.Distance(transform.localPosition, target.localPosition);
        
        if (distanceToTarget < 1.42f)
        {
            SetReward(1.0f);
            EndEpisode();
        }

        // Fell off platform
        if (transform.localPosition.y < 0)
        {
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

    // Update is called once per frame
    void Update()
    {
    }
}
