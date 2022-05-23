using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FixedPoint;

namespace MythrenFighter
{
    public class CollisionManager : MonoBehaviour, RollbackEntity
    {
        [HideInInspector]
        public Guid id { get; } = Guid.NewGuid();
        public static List<Collider> colliders = new List<Collider>();

        private void OnDestroy()
        {
            colliders.Clear();
        }

        private void OnDisable()
        {
            colliders.Clear();
        }

        public dynamic GetInitialState()
        {
            return colliders;
        }

        public void SetState(dynamic state)
        {
            colliders = state;
        }

        public void SimulateFrame()
        {
            InitializeColliders();
            DetectCollision();
            ApplyPushback();
        }

        public dynamic GetUpdatedState()
        {
            return colliders;
        }

        public void UpdateVisuals()
        {

        }

        public static void AddCollider(Collider collider) 
        { 
            colliders.Add(collider); 
        }

        public static void RemoveCollider(Collider collider)
        { 
            colliders.Remove(collider); 
        }

        private void InitializeColliders()
        {
            for (int i= 0; i < colliders.Count; i++)
            {
                colliders[i].attachedPhysicsBody.initialState.pushbackComponent = fp3.zero;
                colliders[i].totalPushback = fp3.zero;
                colliders[i].isColliding = false;
            }
        }

        private void DetectCollision()
        {
            //debugColliders = new List<Collider>(colliders);
            List<Tuple<Collider, Collider>> pairs = colliders.SelectMany(x => colliders, (x, y) => Tuple.Create(x, y)).Where(x => colliders.IndexOf(x.Item1) < colliders.IndexOf(x.Item2)).ToList();

            foreach (Tuple<Collider, Collider> colliderPair in pairs)
            {
                if (colliderPair.Item1 is SphereCollider && colliderPair.Item2 is BoxCollider)
                {
                    CollisionUtilities.SphereBoxCollision(colliderPair.Item1 as SphereCollider, colliderPair.Item2 as BoxCollider);
                }

                if(colliderPair.Item1 is BoxCollider && colliderPair.Item2 is SphereCollider)
                {
                    CollisionUtilities.SphereBoxCollision(colliderPair.Item2 as SphereCollider, colliderPair.Item1 as BoxCollider);
                }

                if (colliderPair.Item1 is SphereCollider && colliderPair.Item2 is SphereCollider)
                {
                    CollisionUtilities.SpherePairCollision(colliderPair.Item1 as SphereCollider, colliderPair.Item2 as SphereCollider);
                }

                if (colliderPair.Item1 is SphereCollider && colliderPair.Item2 is CapsuleCollider)
                {
                    CollisionUtilities.SphereCapsuleCollision(colliderPair.Item1 as SphereCollider, colliderPair.Item2 as CapsuleCollider);
                }

                if(colliderPair.Item1 is CapsuleCollider && colliderPair.Item2 is SphereCollider)
                {
                    CollisionUtilities.SphereCapsuleCollision(colliderPair.Item2 as SphereCollider, colliderPair.Item1 as CapsuleCollider);
                }
                if(colliderPair.Item1 is CapsuleCollider &&  colliderPair.Item2 is BoxCollider)
                {
                    CollisionUtilities.CapsuleBoxCollision(colliderPair.Item1 as CapsuleCollider, colliderPair.Item2 as BoxCollider);
                }
                if (colliderPair.Item1 is BoxCollider && colliderPair.Item2 is CapsuleCollider)
                {
                    CollisionUtilities.CapsuleBoxCollision(colliderPair.Item2 as CapsuleCollider, colliderPair.Item1 as BoxCollider);
                }
                if (colliderPair.Item1 is CapsuleCollider && colliderPair.Item2 is CapsuleCollider)
                {
                    CollisionUtilities.CapsulePairCollision(colliderPair.Item1 as CapsuleCollider, colliderPair.Item2 as CapsuleCollider);
                }
                if(colliderPair.Item1 is BoxCollider && colliderPair.Item2 is BoxCollider)
                {
                    CollisionUtilities.BoxPairCollision(colliderPair.Item1 as BoxCollider, colliderPair.Item2 as BoxCollider);
                }
            }
        }

        private void ApplyPushback()
        {
            foreach(Collider collider in colliders)
            {
                collider.attachedPhysicsBody.currentState.pushbackComponent = collider.totalPushback;
            }
        }
    }
}