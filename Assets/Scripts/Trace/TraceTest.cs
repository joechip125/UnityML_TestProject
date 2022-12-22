using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TraceTest : MonoBehaviour
{
    [SerializeField, HideInInspector] private List<Vector3> traceLocations = new();
    [SerializeField, Range(0, 10)] private int numberDots;
    [SerializeField] private Vector3 placePos;
    [SerializeField] private float placeSphereRadius = 1.5f;
    private int _currentDots;
    private bool _flip;
    private Vector3 _currentPos;

    void Start()
    {
        
    }

    // Update is called once per frame
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
                var addPos = aPos + addDir;
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

    private void MoveDots()
    {
        foreach (var t in traceLocations)
        {
            
        }
    }

    private Vector3 GetDirectionFromRotation(float yRot)
    {
        return (transform.rotation 
                * Quaternion.Euler(0, yRot, 0))
                * Vector3.forward;
    }

    private void OnDrawGizmos()
    {
        if (_currentDots != numberDots)
        {
            ChangeDots();
        }


        var center = transform.position;
        Gizmos.DrawRay(center, GetDirectionFromRotation(34));
        Gizmos.DrawRay(center, GetDirectionFromRotation(180));
        
        if (Vector3.Distance(_currentPos, center) > 0.1f)
        {
            MoveDots();
            
        }
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(center, 3.0f);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube( center + placePos, new Vector3(4,4, 4 * numberDots));


        //foreach (var t in traceLocations)
        //{
        //    Gizmos.color = Color.green;
        //    Gizmos.DrawLine(center, t);
        //    Gizmos.DrawWireSphere(t, placeSphereRadius);
        //}
    }
}
