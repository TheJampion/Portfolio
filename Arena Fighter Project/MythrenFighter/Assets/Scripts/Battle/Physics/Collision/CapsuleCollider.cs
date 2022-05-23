using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedPoint;

namespace MythrenFighter
{
    public class CapsuleCollider : Collider
    {
        public fp radius;
        public fp3 center;
        public fp height;
        public enum directionAxes { X = 0, Y = 1, Z = 2 }
        public directionAxes direction = directionAxes.Y;
        public fp3 forward;
        public fp3 debugPoint1;
        public fp3 debugPoint2;

        public override void Awake()
        {
            base.Awake();
            if (direction == directionAxes.Y)
            {
                forward = fp3.up;
            }
            else if (direction == directionAxes.Z)
            {
                forward = fp3.forward;
            }
            else if (direction == directionAxes.X)
            {
                forward = fp3.left;
            }
        }
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(debugPoint1.ToVector3(), 0.1f);
            Gizmos.DrawSphere(debugPoint2.ToVector3(), 0.1f);

            Gizmos.matrix = transform.localToWorldMatrix;
            if (!isColliding)
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.red;
            }
            Vector3 sphere1Center = Vector3.zero;
            Vector3 sphere2Center = Vector3.zero;
            sphere1Center = center.ToVector3() + forward.ToVector3() * (height.AsFloat / 2);
            sphere2Center = center.ToVector3() - forward.ToVector3() * (height.AsFloat / 2);
            Gizmos.DrawWireSphere(sphere1Center, radius.AsFloat);
            Gizmos.DrawWireSphere(sphere2Center, radius.AsFloat);
            Gizmos.DrawLine(sphere1Center + Vector3.left * radius.AsFloat, sphere2Center + Vector3.left * radius.AsFloat);
            Gizmos.DrawLine(sphere1Center + Vector3.right * radius.AsFloat, sphere2Center + Vector3.right * radius.AsFloat);
            Gizmos.DrawLine(sphere1Center + Vector3.forward * radius.AsFloat, sphere2Center + Vector3.forward * radius.AsFloat);
            Gizmos.DrawLine(sphere1Center + Vector3.back * radius.AsFloat, sphere2Center + Vector3.back * radius.AsFloat);

        }

    }
}

