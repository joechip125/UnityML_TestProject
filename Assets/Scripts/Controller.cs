using System;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
using UnityEngine;

namespace DefaultNamespace
{

    [Serializable]
    public class UnitValues
    {
        public UnitValues(UnitMovement movement)
        {
            this.movement = movement;
        }
        
        public UnitMovement movement;
        public Guid Guid;
    }
    
    public class Controller : MonoBehaviour
    {
        private float maxDistance = 12;
        private List<UnitValues> _units = new ();
        public GridAgent agent;
        
        public event Action<Vector2, Guid> NeedDirectionEvent;

        private void Start()
        {
            var units = FindObjectsOfType<UnitMovement>()
                .Where(x => 
                    Vector3.Distance(x.transform.localPosition, 
                  transform.localPosition) < maxDistance).ToList();
            
            foreach (var u in units)
            {
                _units.Add(new UnitValues(u));
                u.NeedDirectionEvent += OnDirectionNeeded;
            }
        }

        public void RegisterSomething(Action<Vector2, Guid> callback)
        {
            callback += (vector2, guid) =>
            {

            };
        }
        
        private void OnApplicationQuit()
        {
            foreach (var u in _units)
            {
               
                u.movement.NeedDirectionEvent -= OnDirectionNeeded;
            }
        }

        public void OnDirectionReceived(Vector3 pos, Guid guid)
        {
            
        }

        private void OnDirectionNeeded(Vector2 normPos, Guid guid)
        {
            NeedDirectionEvent?.Invoke(normPos, guid);
        }
    }
}