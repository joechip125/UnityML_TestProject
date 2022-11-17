﻿using UnityEngine;

namespace Interfaces
{
    public interface IUnitControlInterface
    {
        public void MoveToLocation(Vector3 newLocation);
        
        public Vector3 GetLocation();

        public void DestroyUnit();
        
        public int GetUnitScore();
    }
}