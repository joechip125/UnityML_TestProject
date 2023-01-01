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
    [SerializeField, HideInInspector] private List<Vector3> traceLocations = new();
    [SerializeField, Range(0, 10)] private int numberDots;
    [SerializeField] private Vector3 placePos;
    [SerializeField] private List<Vector3> positions = new();
    [SerializeField] private float placeSphereRadius = 1.5f;
    private int _currentDots;
    private bool _flip;
    private Vector3 _currentPos;
    public List<UnitPositions> ThePositions = new();

    private List<Vector3> _randPos = new();

    private List<IUnitControlInterface> _unitControlInterfaces = new();

     private float _moveSpeed = 5f;
     private float _rotateSpeed = 10f;
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

        return startDir + new Vector3(0, 0, index * -placeSphereRadius * 2);
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
            var center = transform.position;
            var aPos = center + placePos;
            
            for (int i = _currentDots; i < numberDots; i++)
            {
                var addDir = (transform.right + new Vector3(0,0,numberDots * 0.3f)) * 12;
                var addPos = center;
                traceLocations.Add(addPos);
            }
        }
        
        if (numberDots < _currentDots)
        {
            for (int i = _currentDots; i > numberDots; i--)
            {
                traceLocations.RemoveAt(traceLocations.Count() - 1);
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
                unitInterface?.MoveToLocation(positions[i]);
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

    private void GetDirections(int numbers, int start, int increment)
    {
        positions.Clear();
        var startDir = transform.position + GetDirectionFromRotation(start) * 12;

        for (int i = 0; i < numbers; i++)
        {
            positions.Add(startDir + new Vector3(0,0, i * -increment));
            if(ThePositions.Count < i)
                ThePositions.Add(new UnitPositions(false, startDir + new Vector3(0,0, i * -increment)));
        }
    }

    private void ScanAreas()
    {
        Collider[] colliders = new Collider[30];
        
        for (int i = 0; i < positions.Count; i++)
        {
            var numHits = Physics.OverlapSphereNonAlloc(positions[i], placeSphereRadius, colliders);
            var someHit = false;
            for (int j = 0; j < numHits; j++)
            {
                if (colliders[j].CompareTag("Collector"))
                {
                    someHit = true;
                }
            }
            Gizmos.color = someHit ? Color.green : Color.red;
            Gizmos.DrawWireSphere(positions[i], placeSphereRadius);
        }
    }
    
    
    private void OnDrawGizmos()
    {
        var center = transform.position;
        
        if (_currentDots != numberDots)
        {
            ChangeDots();
            GetDirections(numberDots, 45, 2);
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(center, 2.0f);

        for (int i = 0; i < ThePositions.Count; i++)
        {
            var pos = GetPosition(i);
            if (ThePositions.Count >= i)
            {
                Gizmos.color = ThePositions[i].occupied ? Color.green : Color.red;
                Gizmos.DrawWireSphere(pos, placeSphereRadius);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(center, pos);
            }
        }

        if (_singleInteract)
        {
            Debug.Log(_singleChoice);
            Debug.Log(_singleRotation);
            var pos = GetPosition(_singleChoice);
            var dir = GetDirectionFromRotation(_singleRotation);
            Gizmos.color = Color.black;
            Gizmos.DrawRay(pos, dir);
        }
    }
}
