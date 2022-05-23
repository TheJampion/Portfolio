using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedPoint;

namespace MythrenFighter
{
    public class SphereCollider : Collider
    {
        public fp radius;
        public fp3 center;

        private void OnDrawGizmos()
        {
            if(!isColliding)
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.red;
            }

            Gizmos.DrawWireSphere(center.ToVector3() + transform.position, radius.AsFloat);
        }
        
    }
}

