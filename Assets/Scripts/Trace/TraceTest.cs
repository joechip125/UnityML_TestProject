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

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ChangeDots()
    {
        if (numberDots > _currentDots)
        {
            var add = numberDots - _currentDots;
            var center = transform.position;
            var aPos = center + placePos;
            
            for (int i = _currentDots; i < numberDots; i++)
            {
                var addDir = (transform.right + new Vector3(0,0,numberDots * 0.3f)) * 12;
                var addPos = aPos + addDir;
                traceLocations.Add(addPos);
            }

            foreach (var t in traceLocations)
            {
                
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

    private void OnDrawGizmos()
    {
        if (_currentDots != numberDots)
        {
            ChangeDots();
        }
        
        var center = transform.position;
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(center, 3.0f);

        foreach (var t in traceLocations)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(center, t);
            Gizmos.DrawWireSphere(t, placeSphereRadius);
        }
    }
}
