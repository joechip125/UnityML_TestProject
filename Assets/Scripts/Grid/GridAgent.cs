using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class GridAgent : Agent
{
    public event Action EpisodeBegin;
    public SpawnArea spawnArea;
    private bool m_IsActive;
    private float m_StepTime;
    private float m_StepDuration = 12;
    

    public override void CollectObservations(VectorSensor sensor)
    {
    }
    
    
    public override void OnEpisodeBegin()
    {
        EpisodeBegin?.Invoke();
        //RequestDecision();
        spawnArea.RespawnCollection();
        m_StepTime = 0;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {   
        //AddReward(-0.005f);
        
        
        var continuousActions = actions.ContinuousActions;
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var forward = Mathf.Clamp(continuousActions[0], -1f, 1f);
        var right = Mathf.Clamp(continuousActions[1], -1f, 1f);
        var rotate = Mathf.Clamp(continuousActions[2], -1f, 1f);
         
        dirToGo = transform.forward * forward;
        dirToGo += transform.right * right;
        rotateDir = -transform.up * rotate;
        //transform.localPosition += dirToGo;
        //transform.Rotate(rotateDir, Time.fixedDeltaTime * turnSpeed);
        
        

        if (GetCumulativeReward() < -1.0f)
        {
            EndEpisode();
        }
    }

    private void FixedUpdate()
    {
        if (m_IsActive)
        {
            RequestDecision();
        }
        else if (m_StepDuration > 0)
        {
            m_StepTime += Time.fixedDeltaTime;
            m_IsActive = m_StepTime >= m_StepDuration;
        }
        else
        {
            // Wait one step before activating.
            m_IsActive = true;
        }
    }
    
    
}
