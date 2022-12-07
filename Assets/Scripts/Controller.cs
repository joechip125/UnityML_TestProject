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
        public UnitValues(Vector2 pos, Action<Vector3> callBack)
        {
            unitPos = pos;
            CallBack = callBack;

        }

        public Vector2 unitPos;
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

        private void Awake()
        {
            agent.unitStore = _unitStore;
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

            foreach (var u in units)
            {
                _units.Add(u);
                u.NeedDirectionEvent += OnDirectionNeeded;
            }
        }
        
        private void OnApplicationQuit()
        {
            agent.EpisodeBegin -= OnEpisodeBegin;
            foreach (var u in _units)
            {
                u.NeedDirectionEvent -= OnDirectionNeeded;
            }
        }
        
        private void OnDirectionNeeded(Vector2 normPos, Action<Vector3> callBack)
        {
            _unitStore.Unit.Enqueue(new UnitValues(normPos, callBack));
            //NeedDirectionEvent?.Invoke(normPos,  callBack);
        }
    }
}