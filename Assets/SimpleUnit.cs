using System;
using System.Collections;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;
using UnityEngine.AI;

public class SimpleUnit : MonoBehaviour, IUnitControlInterface
{
    private NavMeshAgent _navMeshAgent;
    private Vector3 _goal;

    public Vector3 Goal
    {
        get => _goal;
        set
        {
            if (_goal == value) return;
            _navMeshAgent.destination = value;
            _goal = value;
        }
    }
    
    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    public void MoveToLocation(Vector3 newLocation)
    {
        Goal = newLocation;
    }

    public void Interact(Vector3 newLocation)
    {
        throw new NotImplementedException();
    }
}
