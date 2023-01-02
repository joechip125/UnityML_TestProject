using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitMove : MonoBehaviour
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
}
