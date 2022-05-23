using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedPoint;

namespace MythrenFighter
{
    public class BoxCollider : Collider
    {
        public fp3 center;
        public fp3 size = new fp3(1,1,1);
        public fp minX, maxX, minY, maxY, minZ, maxZ;
        public fp3[] vertices = new fp3[8];
        public BoxCollider testBoxCollider;

        private void Start()
        {
            minX =-(size.x / 2) + center.x;
            maxX =(size.x / 2) + center.x;
            minY =-(size.y / 2) + center.y;
            maxY =(size.y / 2) + center.y;
            minZ =-(size.z / 2) + center.z;
            maxZ = (size.z / 2) + center.z;

            /* Cannot place min / max in their actual worldspace until the rotation has been applied.
             Instead we rotate each vertice, then apply extra position information after the rotation is finished */

            CollisionUtilities.CheckMinMax(minX, maxX, out minX, out maxX);
            CollisionUtilities.CheckMinMax(minY, maxY, out minY, out maxY);
            CollisionUtilities.CheckMinMax(minZ, maxZ, out minZ, out maxZ);

            vertices[0] = new fp3(minX, minY, minZ);
            vertices[1] = new fp3(minX, minY, maxZ);
            vertices[2] = new fp3(minX, maxY, minZ);
            vertices[3] = new fp3(minX, maxY, maxZ);
            vertices[4] = new fp3(maxX, maxY, maxZ);
            vertices[5] = new fp3(maxX, maxY, minZ);
            vertices[6] = new fp3(maxX, minY, minZ);
            vertices[7] = new fp3(maxX, minY, maxZ);
        }

        private void Update()
        {

            vertices[0] = new fp3(minX, minY, minZ);
            vertices[1] = new fp3(minX, minY, maxZ);
            vertices[2] = new fp3(minX, maxY, minZ);
            vertices[3] = new fp3(minX, maxY, maxZ);
            vertices[4] = new fp3(maxX, maxY, maxZ);
            vertices[5] = new fp3(maxX, maxY, minZ);
            vertices[6] = new fp3(maxX, minY, minZ);
            vertices[7] = new fp3(maxX, minY, maxZ);

            for (int i=0; i < 8; i++)
            {
                vertices[i] = vertices[i] + attachedPhysicsBody.currentState.position;
            }

        }

        public fp3 FindFurthestPoint(fp3 direction)
        {
            fp3 maxPoint = fp3.zero;
            fp maxDistance = -fp.usable_max;

            foreach(fp3 vertex in vertices)
            {
                fp distance = fixmath.Dot(vertex, direction);
                if(distance > maxDistance)
                {
                    maxDistance = distance;
                    maxPoint = vertex;
                }
            }
            return maxPoint;
        }

        private void OnDrawGizmos()
        {
            //Matrix4x4 customMatrix = Gizmos.matrix;
            //Gizmos.matrix = customMatrix;
            //Gizmos.color = Color.black;
            //foreach(fp3 vertex in vertices)
            //{
            //    Gizmos.DrawSphere(vertex.ToVector3(), .25f);
            //}

            Matrix4x4 customMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.matrix = customMatrix;

            if (!isColliding)
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.red;
            }
            Gizmos.DrawWireCube(center.ToVector3(), size.ToVector3());
        }
    }
}