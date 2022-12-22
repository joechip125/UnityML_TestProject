using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Trace
{
    [Serializable]
    public class TraceLet : MonoBehaviour
    {
        public Vector3 mainDirection;
        public float maxRange = 12;
        private bool _moveToTarget = false;
        private Vector3 _target;
        private Vector3 _aPos;
        private float _time = 0;
        private float _distance;
        private float _speed = 1;

        private void FixedUpdate()
        {
            _distance = Vector3.Distance(transform.position, transform.parent.position);
            if (_distance > maxRange && !_moveToTarget)
            {
                _time = 0;
                _target = transform.parent.position;
                _aPos = transform.position;
                _moveToTarget = true;
            }

            if (_moveToTarget)
            {
                Vector3.LerpUnclamped(_aPos, _target, _time);
                _time += (1 / _distance) * (Time.deltaTime * _speed);

                if (_distance < maxRange)
                {
                    _moveToTarget = false;
                    _time = 0;
                }
            }
        }
    }
}