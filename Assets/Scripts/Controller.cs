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
        
        public SpawnArea spawnArea;
        
        public event Action<Vector2, Guid, Action<Vector3>> NeedDirectionEvent;

        private void Awake()
        {
            agent.EpisodeBegin += OnEpisodeBegin;
        }

        private void OnEpisodeBegin()
        {
            spawnArea.RespawnCollection();
        }
        
        private void Start()
        {
            var units = FindObjectsOfType<UnitMovement>()
                .Where(x => 
                    Vector3.Distance(x.transform.localPosition, 
                  transform.localPosition) < maxDistance).ToList();
            
            Debug.Log(units.Count);
            
            foreach (var u in units)
            {
                _units.Add(new UnitValues(u));
                u.NeedDirectionEvent += OnDirectionNeeded;
            }
        }

        public void RegisterSomething(Action<Vector3> callback)
        {
           
        }
        
        private void OnApplicationQuit()
        {
            agent.EpisodeBegin -= OnEpisodeBegin;
            foreach (var u in _units)
            {
                u.movement.NeedDirectionEvent -= OnDirectionNeeded;
            }
        }
        

        private void OnDirectionNeeded(Vector2 normPos, Guid guid, Action<Vector3> callBack)
        {
            NeedDirectionEvent?.Invoke(normPos, guid, callBack);
        }
    }
}