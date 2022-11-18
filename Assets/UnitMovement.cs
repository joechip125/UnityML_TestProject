using System;
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
    public float moveSpeed = 0.2f;

    private int _unitScore;
    
    public Vector3 Goal
    {
        get => _goal;
        set
        {
            if (value == _goal) return;
            if (_moveToGoal) return;
            _goal = value;
            _goalT = 0;
            _moveToGoal = true;
        }
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

    public void AddVector(Vector3 addAmount)
    {
        transform.localPosition += addAmount;
    }

    public Vector3 GetLocation()
    {
        return transform.localPosition;
    }

    public void DestroyUnit()
    {
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Collectable>(out var obstacle))
        {
            _unitScore++;
            obstacle.Deactivate();
        }
    }

    public int GetUnitScore()
    {
        return _unitScore;
    }
}
