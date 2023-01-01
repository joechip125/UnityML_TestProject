using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
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
    public event Action TargetReachedEvent;

    private List<Vector3> positions = new();
    private Vector3 _currentStart;
    public PositionStore _localPos;

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
        if (_localPos.positions.Count <= 0) return;
        
        Goal = _localPos.positions.Dequeue();
        _currentStart = transform.localPosition;
        var dir = (Goal - _currentStart).normalized;
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
        _currentStart = transform.localPosition;
        _requestedDirection = true;
    }
    
    void Update()
    {
        if (_moveToGoal)
        {
            transform.localPosition = Vector3.Lerp(_currentStart, _goal, _goalT);
            //transform.rotation =Quaternion.Lerp(_currentRotation, _nextRotation, _goalR);

            if (Vector3.Distance(transform.localPosition, _goal) < 0.05f)
            {
                RequestDirection();
                _moveToGoal = false;
                _goalT = 0;
            }
            else
            {
                _goalT += Time.deltaTime * moveSpeed;
                //_goalR += Time.deltaTime * rotationSpeed;
            }
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


    public void MoveToLocation(Vector3 newLocation)
    {
        throw new NotImplementedException();
    }

    public void Interact(Vector3 newLocation)
    {
        throw new NotImplementedException();
    }

    public void AddVector(Vector3 addAmount)
    {
        throw new NotImplementedException();
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
