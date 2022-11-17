using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using Interfaces;
using UnityEngine;

public class UnitMovement : MonoBehaviour, IUnitControlInterface
{
    private Vector3 _goal;
    private bool _moveToGoal;
    private float _goalT;
    public float moveSpeed = 10.0f;
    
    public Vector3 Goal
    {
        get => _goal;
        set
        {
            if (value == _goal) return;
            _goal = value;
            SetNewDestination();
            _moveToGoal = true;
        }
    }

    void Start()
    {
        Goal = transform.localPosition + new Vector3(20, 0, 20);
    }

    public void SetNewDestination()
    {
        
    }
    
    void Update()
    {
        if (_moveToGoal)
        {
            var currentPos = transform.localPosition;
            transform.localPosition = Vector3.Lerp(currentPos, _goal, _goalT);

            if (Vector3.Distance(currentPos, _goal) < 0.5f)
            {
                _moveToGoal = false;
                _goalT = 0;
            }
            else _goalT += Time.deltaTime * moveSpeed;
        }
    }

    public void MoveToLocation(Vector3 newLocation)
    {
        Goal = newLocation;
    }
}
