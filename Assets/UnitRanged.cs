using UnityEngine;

namespace DefaultNamespace
{
    public class UnitRanged : UnitBase
    {
        public GameObject projectile;
        
        public override void Interact(Vector3 newLocation)
        {
            var dir = (newLocation - transform.position).normalized;
            var right = transform.right;
            var pos = transform.position + right * 1;
            var bullet = Instantiate(projectile, pos, Quaternion.identity).GetComponent<Bullet>();
            bullet.moveVector = dir;
        }
    }
}