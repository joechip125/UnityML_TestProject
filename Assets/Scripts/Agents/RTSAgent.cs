using System.Collections;
using System.Collections.Generic;
using Interfaces;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class RTSAgent : Agent
{
    public UnitSpawner unitSpawner;
    public AssetSpawner assetSpawner;
    private int _unitScore;
    private IUnitControlInterface _controlInterface;
    private Vector3 _unitLocation;
    public GameObject spawnUnit;
    private Rigidbody _rBody;
    public Transform start;
    public SpawnArea spawnArea;
    public float rewardRange = 0.05f;
    public float currentReward;
    public float rewardIncrement = 0.005f;
    private int _hits;
    private UnitMovement _movement;
    
    
    public override void CollectObservations(VectorSensor sensor)
    {
        //sensor.AddObservation(currentReward);
        sensor.AddObservation(spawnUnit.transform.localPosition);
    
    }

    void Start()
    {
        _movement = spawnUnit.GetComponent<UnitMovement>();
        _movement.FoundObjectAct -= ObjectFound;
        _movement.FoundObjectAct += ObjectFound;
        _movement.CollideWithWall -= CollideWithWall;
        _movement.CollideWithWall += CollideWithWall;
        _rBody = spawnUnit.GetComponent<Rigidbody>();
    }

    private void ObjectFound()
    {
        SetReward(1.0f);
        EndEpisode();
    }
    private void CollideWithWall()
    {
        AddReward(-0.1f);
    }

    public override void OnEpisodeBegin()
    {
        spawnUnit.transform.localPosition = start.localPosition;
        spawnArea.RespawnCollect();
    }

    public float range = 1;
    public float speed = 0.01f;
    private Rigidbody _mAgentRb;

    private float _agentRunSpeed = 1f;

    public override void OnActionReceived(ActionBuffers actions)
    {
        AddReward(-0.005f);
        var choice = ChooseMove(actions.DiscreteActions);
        //MoveAgent(actions.DiscreteActions);
        spawnUnit.transform.localPosition += new Vector3(choice.x * speed, 0, choice.z * speed);
        
        if (spawnUnit.transform.localPosition.y < -1f)
        {
            SetReward(-1.0f);
            EndEpisode();
        }
    }


    private IntVector2 ChooseMove(ActionSegment<int> act)
    {
        var action = act[0];
        var moveDir = new IntVector2();
        switch (action)
        {
            case 1:
                moveDir = new IntVector2(1,1);
                break;
            case 2:
                moveDir = new IntVector2(-1,1);
                break;
            case 3:
                moveDir = new IntVector2(-1,-1);
                break;
            case 4:
                moveDir = new IntVector2(1,-1);
                break;
        }
        
        return moveDir;
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;
        Vector3 localForward = transform.worldToLocalMatrix.MultiplyVector(transform.forward);

        var action = act[0];
        switch (action)
        {
            case 1:
                dirToGo = new Vector3(-1,0,0);
                break;
            case 2:
                dirToGo = new Vector3(1,0,0);
                break;
            case 3:
                dirToGo = new Vector3(0,0,-1);
                break;
            case 4:
                dirToGo = new Vector3(0,0,1);
                break;
        }
        

        _movement.Goal = transform.localPosition + new Vector3(0,0.5f,0) + dirToGo * _agentRunSpeed;
        //spawnUnit.transform.Rotate(rotateDir, Time.deltaTime * 150f);
        //_rBody.AddForce(dirToGo * _agentRunSpeed, ForceMode.VelocityChange);
    }
}
