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
        public UnitValues(Vector3 pos, Action<Vector3> callBack)
        {
            unitPos = pos;
            CallBack = callBack;
        }

        public Vector3 unitPos;
        public Action<Vector3> CallBack;
    }
    
    [Serializable]
    public class UnitStore
    {
        public UnitStore()
        {
            
        }

        public Queue<UnitValues> Unit = new();
    }
    
    public class Controller : MonoBehaviour
    {
        private float maxDistance = 12;
        private List<UnitMovement> _units = new ();
        public UnitStore _unitStore;
        public GridAgent agent;
        public SpawnArea spawnArea;
        public GameObject Collector;
        

        private void Awake()
        {
            agent.unitStore = _unitStore;
            agent.EpisodeBegin += OnResetArea;
        }

        
        
        private void OnResetArea()
        {
            spawnArea.RespawnCollection();
        }
        
        private void Start()
        {
            var units = FindObjectsOfType<UnitMovement>()
                .Where(x => 
                    Vector3.Distance(x.transform.localPosition, 
                  transform.localPosition) < maxDistance).ToList();

            foreach (var u in units)
            {
                _units.Add(u);
                u.NeedDirectionEvent += OnDirectionNeeded;
            }
        }
        
        private void OnApplicationQuit()
        {
            agent.EpisodeBegin -= OnResetArea;
            foreach (var u in _units)
            {
                u.NeedDirectionEvent -= OnDirectionNeeded;
            }
        }
        
        private void OnDirectionNeeded(Vector3 normPos, Action<Vector3> callBack)
        {
            _unitStore.Unit.Enqueue(new UnitValues(normPos, callBack));
            //NeedDirectionEvent?.Invoke(normPos,  callBack);
        }
    }
}