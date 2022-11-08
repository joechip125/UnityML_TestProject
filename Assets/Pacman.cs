using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class Pacman : Agent
{
     public Transform target;
     public Rigidbody rBody;
     public Transform startTrans;
     public int xMove;
     public int zMove;
    
    public override void OnEpisodeBegin()
    {
        if (transform.localPosition.y < 0)
        {
            transform.localPosition = startTrans.position;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(target.localPosition);
        sensor.AddObservation(transform.localPosition);

        // Agent movement
        sensor.AddObservation(xMove);
        sensor.AddObservation(zMove);
     
    }

    public float speed = 10;
    public override void OnActionReceived(ActionBuffers actions)
    {
        xMove =  actions.DiscreteActions[0];
        zMove = actions.DiscreteActions[1];

        transform.position += new Vector3(xMove, 0, zMove);
        
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Pellet>(out var pellet))
        {
            AddReward(0.1f);
            pellet.Deactivate();
        }
        else if(collision.gameObject.TryGetComponent<Ghost>(out var ghost))
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
}
