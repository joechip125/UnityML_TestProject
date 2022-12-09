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
    public float moveSpeed = 0.002f;
    public float rotationSpeed = 0.001f;
    public Guid Guid;

    private bool _requestedDirection;
    public Action FoundObjectAct;
    public Action CollideWithWall;
    private int _unitScore;
    private Quaternion _nextRotation;
    private Quaternion _currentRotation;
    public Action MoveComplete;
    public event Action<Vector3, Action<Vector3>> NeedDirectionEvent; 
    public Action MoveStarted;

    public event Action<Vector3> GetDirectionEvent;

    private void Awake()
    {
        Guid = new Guid();
        GetDirectionEvent += OnGetDirection;
    }

    private void OnApplicationQuit()
    {
        GetDirectionEvent -= OnGetDirection;
    }
    

    private void RequestDirection()
    {
        if (NeedDirectionEvent != null)
        {
            _requestedDirection = true;
            NeedDirectionEvent.Invoke(transform.localPosition, GetDirectionEvent);
        }
      
    }

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
            _requestedDirection = false;
            _currentRotation = transform.rotation;
            _nextRotation = Quaternion.LookRotation(
                _goal - transform.localPosition, Vector3.up);
        }
    }

    private void OnGetDirection(Vector3 newPos)
    {
        Goal = newPos;
    }
    
    void Update()
    {
        if (_moveToGoal)
        {
            var currentPos = transform.localPosition;
            transform.localPosition = Vector3.Lerp(currentPos, _goal, _goalT);
            transform.rotation =Quaternion.Lerp(_currentRotation, _nextRotation, _goalR);

            if (Vector3.Distance(currentPos, _goal) < 0.05f)
            {
                _moveToGoal = false;
                _goalT = 0;
            }
            else _goalT += Time.deltaTime * moveSpeed;

            _goalR += Time.deltaTime * rotationSpeed;
        }
        if(!_moveToGoal && !_requestedDirection)
        {
            RequestDirection();
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

    public void OnDirectionGot(Vector3 nextPos)
    {
        Goal = nextPos;
    }
    
    public void DestroyUnit()
    {
        Destroy(gameObject);
    }
    

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<Collectable>(out var obstacle))
        {
            obstacle.Deactivate();
            FoundObjectAct?.Invoke();
        }
    }
    

    public int GetUnitScore()
    {
        return _unitScore;
    }
}
