using UnityEngine;

namespace Interfaces
{
    public interface IUnitControlInterface
    {
        public void MoveToLocation(Vector3 newLocation);
        public void Attack(Vector3 newLocation);
    }
}