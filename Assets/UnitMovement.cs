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
    private float _goalR;
    public float moveSpeed = 0.2f;
    public float rotationSpeed = 0.001f;

    public Action FoundObjectAct;
    public Action CollideWithWall;
    private int _unitScore;
    private Quaternion _nextRotation;
    private Quaternion _currentRotation;
    
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
            _currentRotation = transform.rotation;
            _nextRotation = Quaternion.LookRotation(
                _goal - transform.localPosition, Vector3.up);
        }
    }
    
    void Update()
    {
        if (_moveToGoal)
        {
            var currentPos = transform.localPosition;
            transform.localPosition = Vector3.Lerp(currentPos, _goal, _goalT);
            transform.rotation =Quaternion.Lerp(_currentRotation, _nextRotation, _goalR);

            if (Vector3.Distance(currentPos, _goal) < 0.5f)
            {
                _moveToGoal = false;
                _goalT = 0;
            }
            else _goalT += Time.deltaTime * moveSpeed;

            _goalR += Time.deltaTime * rotationSpeed;
        }
    }

    public bool IsAtGoal(float tolerance)
    {
        return Vector3.Distance(transform.localPosition, Goal) < tolerance;
    }
    
    public void SetGoal(Vector3 newPos)
    {
        _goal = newPos;
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
            FoundObjectAct?.Invoke();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<Collectable>(out var obstacle))
        {
            FoundObjectAct?.Invoke();
        }
        
        if (other.CompareTag("Wall"))
        {
            CollideWithWall?.Invoke();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            CollideWithWall?.Invoke();
        }
    }

    public int GetUnitScore()
    {
        return _unitScore;
    }
}
