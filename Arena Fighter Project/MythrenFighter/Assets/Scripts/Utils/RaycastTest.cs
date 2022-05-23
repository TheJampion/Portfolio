using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedPoint;

namespace MythrenFighter
{
    public class RaycastTest : MonoBehaviour
    {
        public float raycastLength = 1f;
        public bool isColliding;
        //public enum directionAxes { X = 0, Y = 1, Z = 2 }
        //public directionAxes direction = directionAxes.Y;
        //public fp3 forward;
        //public GameObject rayEnd;

        public void Awake()
        {
            //if (direction == directionAxes.Y)
            //{
            //    forward = fp3.up;
            //}
            //else if (direction == directionAxes.Z)
            //{
            //    forward = fp3.forward;
            //}
            //else if (direction == directionAxes.X)
            //{
            //    forward = fp3.left;
            //}
        }

        private void OnDrawGizmos()
        {
            if (isColliding)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.green;
            }
            Vector3 target =transform.position + Vector3.left * raycastLength;
            Gizmos.DrawLine(transform.position, target);
        }


        // Update is called once per frame
        void Update()
        {
            LayerMask layerMask = LayerMask.NameToLayer("Default");
            RaycastHit hit;
            CollisionUtilities.Raycast(transform.position.ToFp3(), -fp3.right, raycastLength.ToFixedPoint() , layerMask, out hit);
            isColliding = CollisionUtilities.Raycast(transform.position.ToFp3(), -fp3.right, raycastLength.ToFixedPoint(), 0, out hit);
        }
    }
}
