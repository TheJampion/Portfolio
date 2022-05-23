using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedPoint;

namespace MythrenFighter
{
    public struct RaycastHit
    {
        public Collider collider;
        public fp3 normal;
        public fp3 point;
        public PhysicsBody physicsBody;
        public fp3 position;
        public fp distance;

        public RaycastHit(Collider collider, fp3 normal, fp3 point, PhysicsBody physicsBody, fp3 position, fp distance)
        {
            this.collider = collider;
            this.normal = normal;
            this.point = point;
            this.physicsBody = physicsBody;
            this.position = position;
            this.distance = distance;
        }
    }

    public static class CollisionUtilities
    {
        public static fp3 GetClosestPointOnLineSegment(this fp3 point, fp3 lineA, fp3 lineB)
        {
            fp3 lineAToPoint = point - lineA;
            fp3 lineAToB = lineB - lineA;
            fp dotAToB = fixmath.Dot(lineAToB, lineAToB);
            fp dotPointToLine = fixmath.Dot((point - lineA), lineAToB);
            fp t;
            if (dotPointToLine == fp._0 || dotAToB == fp._0)
            {
                t = fp._0;
            }
            else
            {
                t = dotPointToLine / dotAToB;
            }

            return lineA + fixmath.Min((fixmath.Max(t, 0)), 1) * lineAToB;

        }

        public static fp3 GetBoxClosestPoint(fp3 a, BoxCollider b)
        {
            fp x = fixmath.Max(b.minX, fixmath.Min(a.x, b.maxX));
            fp y = fixmath.Max(b.minY, fixmath.Min(a.y, b.maxY));
            fp z = fixmath.Max(b.minZ, fixmath.Min(a.z, b.maxZ));

            return new fp3(x, y, z);
        }

        public static bool ClipLine(fp min, fp max, fp lineOriginDim, fp lineEndDim, fp low, fp high, out fp f_Low)
        {
            fp dim_low, dim_high;
            if (lineEndDim - lineOriginDim == fp._0)
            {
                dim_low = min;
                dim_high = max;


                if (dim_high < dim_low)
                {
                    fp tmp = dim_high;
                    dim_high = dim_low;
                    dim_low = tmp;
                }

                low = fixmath.Max(dim_low, low);
                high = fixmath.Max(dim_high, high);

                f_Low = low;

                if (lineOriginDim < dim_low || lineEndDim < dim_low)
                {
                    return false;
                }
                if (lineOriginDim > dim_high || lineEndDim > dim_high)
                {
                    return false;
                }
            }
            else
            {
                dim_low = ((min - lineOriginDim) / (lineEndDim - lineOriginDim));
                dim_high = ((max - lineOriginDim) / (lineEndDim - lineOriginDim));


                if (dim_high < dim_low)
                {
                    fp tmp = dim_high;
                    dim_high = dim_low;
                    dim_low = tmp;
                }


                if (dim_high < low || dim_low > high)
                {
                    f_Low = low;
                    return false;
                }

                low = fixmath.Max(dim_low, low);
                high = fixmath.Max(dim_high, high);
                f_Low = low;

                if (low > high)
                {
                    return false;
                }
            }

            return true;
        }

        public static void SetBoxMinMax(BoxCollider a)
        {
            a.minX = a.attachedPhysicsBody.currentState.position.x + -(a.size.x / 2);
            a.maxX = a.attachedPhysicsBody.currentState.position.x + a.size.x / 2;
            a.minY = a.attachedPhysicsBody.currentState.position.y + -(a.size.y / 2);
            a.maxY = a.attachedPhysicsBody.currentState.position.y + a.size.y / 2;
            a.minZ = a.attachedPhysicsBody.currentState.position.z + -(a.size.z / 2);
            a.maxZ = a.attachedPhysicsBody.currentState.position.z + a.size.z / 2;
        }

        public static void SphereBoxCollision(SphereCollider a, BoxCollider b)
        {
            fp3 pointA = a.attachedPhysicsBody.currentState.position + a.center;
            fp3 pointB = b.attachedPhysicsBody.currentState.position + b.center;

            fp3 boxClosestPoint = GetBoxClosestPoint(pointA, b);

            fp distance = fixmath.Distance(boxClosestPoint, pointA);
            bool sphereBoxCollision = distance < a.radius;
            if (sphereBoxCollision && !Physics.GetIgnoreLayerCollision(a.gameObject.layer, b.gameObject.layer))
            {
                a.isColliding = true;
                b.isColliding = true;
                if (!a.currentCollisions.Contains(b))
                {
                    a.onTriggerEnter?.Invoke(b);
                    a.currentCollisions.Add(b);
                }

                if (b.currentCollisions.Contains(a))
                {
                    b.onTriggerEnter?.Invoke(a);
                    b.currentCollisions.Add(a);
                }
                //Moved Pushback Logic to be part of CollisionManager in the function called ApplyPushback()

                //if(!a.isTrigger && !b.isTrigger)
                //{
                //    a.totalPushback += -((new fp3(pointB.x, pointB.y, pointB.z) - new fp3(pointA.x, pointA.y, pointA.z)).Normalize() * (PhysicsBody.PUSHBACK_STRENGTH * 2));
                //}
            }
            else
            {
                if (a.currentCollisions.Contains(b))
                {
                    a.currentCollisions.Remove(b);
                }
                if (b.currentCollisions.Contains(a))
                {
                    b.currentCollisions.Remove(a);
                }
            }
        }

        public static void SpherePairCollision(SphereCollider a, SphereCollider b)
        {
            fp3 pointA = a.attachedPhysicsBody.currentState.position + a.center;
            fp3 pointB = b.attachedPhysicsBody.currentState.position + b.center;
            fp distance = fixmath.Distance(pointA, pointB) / 2;
            bool spheresColliding = (distance < (a.radius + b.radius) / 2);

            if (spheresColliding && !Physics.GetIgnoreLayerCollision(a.gameObject.layer, b.gameObject.layer) && a.gameObject.activeSelf && b.gameObject.activeSelf)
            {
                a.isColliding = true;
                b.isColliding = true;
                a.onTriggerEnter?.Invoke(b);
                b.onTriggerEnter?.Invoke(a);

                if (!a.currentCollisions.Contains(b))
                {
                    a.currentCollisions.Add(b);
                }

                if (b.currentCollisions.Contains(a))
                {
                    b.currentCollisions.Add(a);
                }
            }
            else
            {
                if (a.currentCollisions.Contains(b))
                {
                    a.currentCollisions.Remove(b);
                }
                if (b.currentCollisions.Contains(a))
                {
                    b.currentCollisions.Remove(a);
                }
            }
            //Moved Pushback Logic to be part of CollisionManager in the function called ApplyPushback()

            //if(!a.isTrigger && !b.isTrigger)
            //{
            //    a.totalPushback += -((new fp3(pointB.x, 0, pointB.z) - new fp3(pointA.x, 0, pointA.z)).Normalize() * PhysicsBody.PUSHBACK_STRENGTH);
            //}
        }

        public static void SphereCapsuleCollision(SphereCollider a, CapsuleCollider b)
        {
            fp3 pointA = a.attachedPhysicsBody.initialState.position + a.center;

            fp4 capsule1Quaternion = new fp4(b.transform.rotation.x.ToFixedPoint(), b.transform.rotation.y.ToFixedPoint(), b.transform.rotation.z.ToFixedPoint(), b.transform.rotation.w.ToFixedPoint());
            fp3 capsuleA = (b.forward * (b.height / 2)).Rotate(capsule1Quaternion);
            fp3 capsuleB = (-b.forward * (b.height / 2)).Rotate(capsule1Quaternion);
            capsuleA += a.center + a.attachedPhysicsBody.currentState.position;
            capsuleB += a.center + a.attachedPhysicsBody.currentState.position;
            fp3 pointB = pointA.GetClosestPointOnLineSegment(capsuleA, capsuleB);

            bool sphereCapsuleColliding;
            sphereCapsuleColliding = (fixmath.Distance(pointA.GetClosestPointOnLineSegment(capsuleA, capsuleB), pointA) < (a.radius + b.radius));

            if (sphereCapsuleColliding && !Physics.GetIgnoreLayerCollision(a.gameObject.layer, b.gameObject.layer))
            {
                a.isColliding = true;
                b.isColliding = true;
                if (!a.currentCollisions.Contains(b))
                {
                    a.onTriggerEnter?.Invoke(b);
                    a.currentCollisions.Add(b);
                }

                if (b.currentCollisions.Contains(a))
                {
                    b.onTriggerEnter?.Invoke(a);
                    b.currentCollisions.Add(a);
                }
            }
            else
            {
                if (a.currentCollisions.Contains(b))
                {
                    a.currentCollisions.Remove(b);
                }
                if (b.currentCollisions.Contains(a))
                {
                    b.currentCollisions.Remove(a);
                }
            }
            //Moved Pushback Logic to be part of CollisionManager in the function called ApplyPushback()

            //if(!a.isTrigger && !b.isTrigger)
            //{
            //    a.totalPushback += -((new fp3(pointB.x, 0, pointB.z) - new fp3(pointA.x, 0, pointA.z)).Normalize() * PhysicsBody.PUSHBACK_STRENGTH);
            //    b.totalPushback += -((new fp3(pointA.x, 0, pointA.z) - new fp3(pointB.x, 0, pointB.z)).Normalize() * PhysicsBody.PUSHBACK_STRENGTH);
            //}
        }

        public static void CapsulePairCollision(CapsuleCollider a, CapsuleCollider b)
        {
            fp4 capsule1Quaternion = new fp4(a.transform.rotation.x.ToFixedPoint(), a.transform.rotation.y.ToFixedPoint(), a.transform.rotation.z.ToFixedPoint(), a.transform.rotation.w.ToFixedPoint());
            fp4 capsule2Quaternion = new fp4(b.transform.rotation.x.ToFixedPoint(), b.transform.rotation.y.ToFixedPoint(), b.transform.rotation.z.ToFixedPoint(), b.transform.rotation.w.ToFixedPoint());
            fp3 capsule1A = (a.forward * (a.height / 2)).Rotate(capsule1Quaternion);
            fp3 capsule1B = (-a.forward * (a.height / 2)).Rotate(capsule1Quaternion);
            capsule1A += a.center + a.attachedPhysicsBody.currentState.position;
            capsule1B += a.center + a.attachedPhysicsBody.currentState.position;

            a.debugPoint1 = capsule1A;
            a.debugPoint2 = capsule1B;

            fp3 capsule2A = (b.forward * (b.height / 2)).Rotate(capsule2Quaternion);
            fp3 capsule2B = (-b.forward * (b.height / 2)).Rotate(capsule2Quaternion);
            capsule2A += b.center + b.attachedPhysicsBody.currentState.position;
            capsule2B += b.center + b.attachedPhysicsBody.currentState.position;
            b.debugPoint1 = capsule2A;
            b.debugPoint2 = capsule2B;

            fp3 bestA = fp3.zero;
            fp3 bestB = fp3.zero;
            //Vectors between line end points
            fp3 v0 = capsule2A - capsule1A;
            fp3 v1 = capsule2B - capsule1A;
            fp3 v2 = capsule2A - capsule1B;
            fp3 v3 = capsule2B - capsule1B;

            //Squared Distances
            fp d0 = fixmath.DistanceSqr(v0, v0);
            fp d1 = fixmath.DistanceSqr(v1, v1);
            fp d2 = fixmath.DistanceSqr(v2, v2);
            fp d3 = fixmath.DistanceSqr(v3, v3);

            if (d2 < d0 || d2 < d1 || d3 < d0 || d3 < d1)
            {
                bestA = capsule1B;
            }
            else
            {
                bestA = capsule1A;
            }

            bestB = bestA.GetClosestPointOnLineSegment(capsule2A, capsule2B);
            bestA = bestB.GetClosestPointOnLineSegment(capsule1A, capsule1B);

            bool capsulesColliding = fixmath.Distance(bestA, bestB) < (a.radius + b.radius);
            Debug.Log(capsulesColliding);
            if (capsulesColliding && !Physics.GetIgnoreLayerCollision(a.gameObject.layer, b.gameObject.layer))
            {
                a.isColliding = true;
                b.isColliding = true;
                if (!a.currentCollisions.Contains(b))
                {
                    a.onTriggerEnter?.Invoke(b);
                    a.currentCollisions.Add(b);
                }

                if (b.currentCollisions.Contains(a))
                {
                    b.onTriggerEnter?.Invoke(a);
                    b.currentCollisions.Add(a);
                }
            }
            else
            {
                if (a.currentCollisions.Contains(b))
                {
                    a.currentCollisions.Remove(b);
                }
                if (b.currentCollisions.Contains(a))
                {
                    b.currentCollisions.Remove(a);
                }
            }
            //Moved Pushback Logic to be part of CollisionManager in the function called ApplyPushback()

            //if (!a.isTrigger && !b.isTrigger)
            //{
            //    a.totalPushback += -((new fp3(bestB.x, 0, bestB.z) - new fp3(bestA.x, 0, bestA.z)).Normalize() * PhysicsBody.PUSHBACK_STRENGTH);
            //    b.totalPushback += -((new fp3(bestA.x, 0, bestA.z) - new fp3(bestB.x, 0, bestB.z)).Normalize() * PhysicsBody.PUSHBACK_STRENGTH);
            //}
        }

        public static void CapsuleBoxCollision(CapsuleCollider a, BoxCollider b)
        {
            fp4 capsule1Quaternion = new fp4(a.transform.rotation.x.ToFixedPoint(), a.transform.rotation.y.ToFixedPoint(), a.transform.rotation.z.ToFixedPoint(), a.transform.rotation.w.ToFixedPoint());
            fp3 capsule1A = (a.forward * (a.height / 2)).Rotate(capsule1Quaternion);
            fp3 capsule1B = (-a.forward * (a.height / 2)).Rotate(capsule1Quaternion);
            capsule1A += a.center + a.attachedPhysicsBody.currentState.position;
            capsule1B += a.center + a.attachedPhysicsBody.currentState.position;

            fp3 v0 = GetBoxClosestPoint(capsule1A, b);
            fp3 v1 = GetBoxClosestPoint(capsule1B, b);
            fp3 bestPoint;
            if (fixmath.Distance(capsule1A, v0) < fixmath.Distance(capsule1B, v0))
            {
                bestPoint = v0;
            }
            else if (fixmath.Distance(capsule1A, v0) == fixmath.Distance(capsule1B, v0))
            {
                bestPoint = a.center + a.attachedPhysicsBody.currentState.position;
            }
            else
            {
                bestPoint = v1;
            }

            if (fixmath.Distance(GetClosestPointOnLineSegment(bestPoint, capsule1A, capsule1B), bestPoint) < a.radius)
            {
                a.isColliding = true;
                b.isColliding = true;
                if (!a.currentCollisions.Contains(b))
                {
                    a.onTriggerEnter?.Invoke(b);
                    a.currentCollisions.Add(b);
                }

                if (b.currentCollisions.Contains(a))
                {
                    b.onTriggerEnter?.Invoke(a);
                    b.currentCollisions.Add(a);
                }
            }
            else
            {
                if (a.currentCollisions.Contains(b))
                {
                    a.currentCollisions.Remove(b);
                }
                if (b.currentCollisions.Contains(a))
                {
                    b.currentCollisions.Remove(a);
                }
            }
        }

        public static void BoxPairCollision(BoxCollider a, BoxCollider b)
        {
            SetBoxMinMax(a);
            SetBoxMinMax(b);
            if (a.minX <= b.maxX && a.maxX >= b.minX && a.minY <= b.maxY && a.maxY >= b.minY && a.minZ <= b.maxZ && a.maxZ >= b.minZ)
            {
                a.isColliding = true;
                b.isColliding = true;
                if (!a.currentCollisions.Contains(b))
                {
                    a.onTriggerEnter?.Invoke(b);
                    a.currentCollisions.Add(b);
                }

                if (b.currentCollisions.Contains(a))
                {
                    b.onTriggerEnter?.Invoke(a);
                    b.currentCollisions.Add(a);
                }
            }
            else
            {
                if (a.currentCollisions.Contains(b))
                {
                    a.currentCollisions.Remove(b);
                }
                if (b.currentCollisions.Contains(a))
                {
                    b.currentCollisions.Remove(a);
                }
            }
        }

        /// <summary>
        /// This function returns the FIRST object hit by a raycast
        /// </summary>
        /// <param name="origin"> Origin point of the raycast </param>
        /// <param name="direction"> Direction of the raycast</param>
        /// <param name="maxDistance"> Distance of the raycast, default is 0</param>
        /// <param name="layerMask"> Layer mask to check the raycast against.  Default is 0</param>
        /// <param name="hitInfo"> hit information from the raycast</param>
        /// <returns></returns>

        public static bool Raycast(fp3 origin, fp3 direction, fp maxDistance, int layerMask, out RaycastHit hitInfo)
        {
            if (maxDistance == fp._0)
            {
                maxDistance = 1;
            }
            hitInfo = new RaycastHit(null, fp3.zero, fp3.zero, null, fp3.zero, fp._0);
            fp3 raycastEnd = origin + direction * maxDistance;
            foreach (Collider collider in CollisionManager.colliders)
            {
                if (collider.gameObject.layer == layerMask)
                {
                    if (collider is SphereCollider)
                    {
                        SphereCollider sphereCollider = collider as SphereCollider;
                        fp3 actualSphereCenter = sphereCollider.attachedPhysicsBody.currentState.position + sphereCollider.center;
                        fp distance = fixmath.Distance(actualSphereCenter.GetClosestPointOnLineSegment(origin, raycastEnd), actualSphereCenter);
                        fp3 position = sphereCollider.attachedPhysicsBody.currentState.position;
                        fp3 normal = actualSphereCenter - actualSphereCenter.GetClosestPointOnLineSegment(origin, raycastEnd);
                        fp3 collisionPoint = actualSphereCenter.GetClosestPointOnLineSegment(origin, raycastEnd);
                        if (distance <= sphereCollider.radius)
                        {
                            hitInfo = new RaycastHit(sphereCollider, normal, collisionPoint, sphereCollider.attachedPhysicsBody, position, distance);
                            return true;
                        }
                    }
                    if (collider is BoxCollider)
                    {

                    }

                    if (collider is CapsuleCollider)
                    {
                        CapsuleCollider capsuleCollider = collider as CapsuleCollider;
                        fp3 capsule1A = (capsuleCollider.center + capsuleCollider.attachedPhysicsBody.currentState.position + capsuleCollider.forward * (capsuleCollider.height / 2));
                        fp3 capsule1B = (capsuleCollider.center + capsuleCollider.attachedPhysicsBody.currentState.position - capsuleCollider.forward * (capsuleCollider.height / 2));

                        fp3 bestA = fp3.zero;
                        fp3 bestB = fp3.zero;
                        //Vectors between line end points
                        fp3 v0 = origin - capsule1A;
                        fp3 v1 = raycastEnd - capsule1A;
                        fp3 v2 = origin - capsule1B;
                        fp3 v3 = raycastEnd - capsule1B;

                        //Squared Distances
                        fp d0 = fixmath.DistanceSqr(v0, v0);
                        fp d1 = fixmath.DistanceSqr(v1, v1);
                        fp d2 = fixmath.DistanceSqr(v2, v2);
                        fp d3 = fixmath.DistanceSqr(v3, v3);

                        if (d2 < d0 || d2 < d1 || d3 < d0 || d3 < d1)
                        {
                            bestA = capsule1B;
                        }
                        else
                        {
                            bestA = capsule1A;
                        }

                        bestB = bestA.GetClosestPointOnLineSegment(origin, raycastEnd);
                        bestA = bestB.GetClosestPointOnLineSegment(capsule1A, capsule1B);

                        fp distance = fixmath.Distance(bestA, bestB);
                        fp3 position = capsuleCollider.attachedPhysicsBody.currentState.position;
                        fp3 normal = (bestB - bestA).Normalize();
                        fp3 collisionPoint = bestA;

                        if (distance < capsuleCollider.radius)
                        {
                            hitInfo = new RaycastHit(capsuleCollider, normal, collisionPoint, capsuleCollider.attachedPhysicsBody, position, distance);
                            return true;
                        }
                    }
                }
            }

            if (origin + direction * maxDistance == fp3.zero)
            {
                return false;
            }

            return false;
        }

        public static void CheckMinMax(fp min, fp max, out fp minOutput, out fp maxOutput)
        {
            if (max < min)
            {
                fp tmp = min;
                min = max;
                max = tmp;
            }
            minOutput = min;
            maxOutput = max;
        }

        public static fp3 Rotate(this fp3 p, fp4 quaternion)
        {

            fp4x4 rotationMatrix = fp4x4.Rotate(quaternion);
            return rotationMatrix.MultiplyPoint(p);
        }
    }
}
