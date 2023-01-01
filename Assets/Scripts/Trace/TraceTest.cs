using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class UnitPositions
{
    public bool Occupied = false;
    public Vector3 RelativePosition;
    public IUnitControlInterface UnitInterface;

    public UnitPositions(bool occupied, Vector3 relativePosition)
    {
        Occupied = occupied;
        RelativePosition = relativePosition;
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

    private void Awake()
    {
        SetUniquePositions(4);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.D))
        {
            transform.position -= new Vector3(0, 0, 0.5f * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.position += new Vector3(0, 0, 0.5f * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += new Vector3(0.5f * Time.deltaTime,0,0);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position += new Vector3(0.5f * Time.deltaTime,0,0);
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
            if (!ThePositions[i].Occupied)
            {
                unitInterface?.MoveToLocation(positions[i]);
                ThePositions[i].Occupied = true;
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
            if (ThePositions.Count >= i)
            {
                Gizmos.color = ThePositions[i].Occupied ? Color.green : Color.red;
                Gizmos.DrawWireSphere(positions[i], placeSphereRadius);
            }
        }
        
        foreach (var t in positions)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(center, t);
        }

        //if (Application.isPlaying)
        //{
        //    foreach (var r in _randPos)
        //    {
        //        Gizmos.DrawWireSphere(r, placeSphereRadius);
        //    }
        //}
        
    }
}
