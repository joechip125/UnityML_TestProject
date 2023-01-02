using System;
using System.Collections;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;
using UnityEngine.AI;

public abstract class UnitBase : MonoBehaviour, IUnitControlInterface
{
    private NavMeshAgent _navMeshAgent;
    private Vector3 _goal;

    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

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
    
    public virtual void MoveToLocation(Vector3 newLocation)
    {
        Goal = newLocation;
    }

    public virtual void Interact(Vector3 newLocation)
    {
        
    }
}
