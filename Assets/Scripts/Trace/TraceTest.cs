using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class UnitPositions
{
    public bool occupied = false;
    public Vector3 relativePosition;
    public IUnitControlInterface UnitInterface;

    public UnitPositions(bool occupied, Vector3 relativePosition)
    {
        this.occupied = occupied;
        this.relativePosition = relativePosition;
    }
}

public class TraceTest : MonoBehaviour
{
    [SerializeField, Range(0, 10)] private int numberDots;
    [SerializeField] private float placeSphereRadius = 1.5f;
    private int _currentDots;
    private bool _flip;
    private Vector3 _currentPos;
    [SerializeField, HideInInspector]public List<UnitPositions> ThePositions = new();

    private List<Vector3> _randPos = new();
    
     private float _moveSpeed = 5f;
     private float _rotateSpeed = 16f;
     private float _singleRotation;
     private int _singleChoice = 0;

     private bool _singleInteract = false;

    private void Awake()
    {
        SetUniquePositions(4);
    }

    private Vector3 GetPosition(int index)
    {
        var startDir = transform.position + GetDirectionFromRotation(45) * 12;
     
        return startDir + transform.forward * (index * -placeSphereRadius * 2);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            _singleInteract = !_singleInteract;
        }

        if (!_singleInteract)
        {
            if (Input.GetKey(KeyCode.D))
            {
                transform.position -= new Vector3(0, 0, _moveSpeed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.A))
            {
                transform.position += new Vector3(0, 0, _moveSpeed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.W))
            {
                transform.position += new Vector3(_moveSpeed * Time.deltaTime, 0, 0);
            }

            if (Input.GetKey(KeyCode.S))
            {
                transform.position -= new Vector3(_moveSpeed * Time.deltaTime, 0, 0);
            }
            UpdatePositions();
        }
        else
        {
            if (Input.GetKey(KeyCode.A))
            {
                _singleRotation -= _rotateSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.D))
            {
                _singleRotation += _rotateSpeed * Time.deltaTime;
            }

            _singleRotation = Mathf.Clamp(_singleRotation, 45, 120);
            
            if (Input.anyKeyDown)
            {
                if (int.TryParse(Input.inputString, out var num))
                {
                    _singleChoice = num;
                }

                if (Input.GetKeyDown(KeyCode.H))
                {
                    LaunchUnit();
                }
            }
        }
    }

    private void LaunchUnit()
    {
        if (ThePositions[_singleChoice].UnitInterface == null) return;
        
        var dir = GetDirectionFromRotation(_singleRotation);
        var pos = GetPosition(_singleChoice) + dir * 12;
        ThePositions[_singleChoice].UnitInterface.MoveToLocation(pos);
    }

    private void UpdatePositions()
    {
        for (int i = 0; i < ThePositions.Count; i++)
        {
            if(ThePositions[i].UnitInterface == null) continue;
            var pos = GetPosition(i);
            ThePositions[i].UnitInterface.MoveToLocation(pos);
        }
    }
    
    private void ChangeDots()
    {
        if (numberDots > _currentDots)
        {
            for (int i = _currentDots; i < numberDots; i++)
            {
                var aPos = GetPosition(i);
                ThePositions.Add(new UnitPositions(false,aPos));
            }
        }
        
        if (numberDots < _currentDots)
        {
            for (int i = _currentDots; i > numberDots; i--)
            {
                ThePositions.RemoveAt(ThePositions.Count() - 1);
            }
        }
        
        _currentDots = numberDots;
    }

    public void RegisterUnit(IUnitControlInterface unitInterface)
    {
        for (int i = 0; i < ThePositions.Count; i++)
        {
            if (!ThePositions[i].occupied)
            {
                unitInterface?.MoveToLocation(GetPosition(i));
                ThePositions[i].occupied = true;
                ThePositions[i].UnitInterface = unitInterface;
                break;
            }
        }
    }
    
    private void SetUniquePositions(float range)
    {
        _randPos.Clear();
        var center = transform.position;
        
        var unique = false;
        
        for (var i = 0; i < numberDots; i++)
        {
            while (!unique)
            {
                var location = center 
                               + new Vector3(Random.Range(-range, range), 0, Random.Range(-range, range));
                
                if (_randPos.Any(x => Vector3.Distance(location, x) < placeSphereRadius * 2)) continue;
                
                _randPos.Add(location);
                unique = true;
            }

            unique = false;
        }
    }

    private Vector3 GetDirectionFromRotation(float yRot)
    {
        return (transform.rotation 
                * Quaternion.Euler(0, yRot, 0))
                * Vector3.forward;
    }

    private void ScanAreas()
    {
        Collider[] colliders = new Collider[30];
        
        for (int i = 0; i < numberDots; i++)
        {
            var pos = GetPosition(i);
            var numHits = Physics.OverlapSphereNonAlloc(pos, placeSphereRadius, colliders);
            var someHit = false;
            for (int j = 0; j < numHits; j++)
            {
                if (colliders[j].CompareTag("Collector"))
                {
                    someHit = true;
                }
            }
            Gizmos.color = someHit ? Color.green : Color.red;
            Gizmos.DrawWireSphere(pos, placeSphereRadius);
        }
    }
    
    
    private void OnDrawGizmos()
    {
        var center = transform.position;

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(center, 2.0f);
        
        if (_currentDots != numberDots)
        {
            ChangeDots();
        }

        for (int i = 0; i < numberDots; i++)
        {
            var pos = GetPosition(i);
            
            Gizmos.color = ThePositions[i].occupied ? Color.green : Color.red;
            Gizmos.DrawWireSphere(pos, placeSphereRadius);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(center, pos);
        }

        if (_singleInteract)
        {
            var pos = GetPosition(_singleChoice);
            var dir = GetDirectionFromRotation(_singleRotation);
            Gizmos.color = Color.black;
            Gizmos.DrawRay(pos, dir);
        }
    }
}
