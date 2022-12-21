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
    public class PositionStore
    {
        public PositionStore()
        {
            
        }

        public Queue<Vector3> positions = new();
    }
    
    public class Controller : MonoBehaviour
    {
        private float maxDistance = 12;
        [SerializeField]private List<UnitMovement> _units = new ();
       
        public GridAgentSearch agent;
        public SpawnArea spawnArea;
        public GameObject Collector;
        private PositionStore _positionStore;
        

        private void Awake()
        {
            _positionStore = agent.positions;
            agent.ResetMap += OnResetArea;
        }

        
        
        private void OnResetArea()
        {
            spawnArea.RespawnCollection();
        }
        
        private void Start()
        {
            var units = FindObjectsOfType<UnitMovement>()
                .Where(x => 
                    Vector3.Distance(x.transform.position, 
                  transform.position) < maxDistance).ToList();

            foreach (var u in units)
            {
                _units.Add(u);
                u._localPos = _positionStore;
                u.NeedDirectionEvent += OnDirectionNeeded;
            }
        }
        
        private void OnApplicationQuit()
        {
            agent.ResetMap -= OnResetArea;
            foreach (var u in _units)
            {
                u.NeedDirectionEvent -= OnDirectionNeeded;
            }
        }
        
        private void OnDirectionNeeded(Vector3 normPos, Action<Vector3> callBack)
        {
            
            if (_positionStore.positions.Count > 0)
            {
                callBack.Invoke(_positionStore.positions.Dequeue());
            }
        }
    }
}