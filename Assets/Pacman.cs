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
     public Transform ghostTrans;
     public int moveDir;
     
    public override void OnEpisodeBegin()
    {
        if (transform.localPosition.y < 0)
        {
            transform.localPosition = new Vector3(0,1,0);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent movement
        sensor.AddObservation(moveDir);
    }

    public float speed = 10;
    public override void OnActionReceived(ActionBuffers actions)
    {
        MoveAgent(actions);
        
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

    public void MoveAgent(ActionBuffers actions)
    {
        moveDir =  Mathf.Clamp(actions.DiscreteActions[0], 0, 3);
        var moveChoice = MazeDirections.ToIntVector2((MazeDirection) moveDir);
        Debug.Log($"{moveDir}"); 

        transform.position += new Vector3(moveChoice.x, 0, moveChoice.z);
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var act = actionsOut.DiscreteActions;
         Debug.Log($"{act[0]}, {act[1]}"); 
        //act[0] = Input.GetAxis("Horizontal");
        //act[1] = Input.GetAxis("Vertical");
    }
    
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }
}
