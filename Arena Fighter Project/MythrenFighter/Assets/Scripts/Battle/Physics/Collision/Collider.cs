using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedPoint;

namespace MythrenFighter
{
    [RequireComponent (typeof(PhysicsBody))]
    public class Collider : MonoBehaviour
    {

        [HideInInspector]
        public PhysicsBody attachedPhysicsBody;
        public bool isTrigger;
        public bool isColliding;
        public fp3 totalPushback;
        public List<Collider> currentCollisions = new List<Collider>();
        public delegate void OnTriggerEnter(Collider other);
        public OnTriggerEnter onTriggerEnter;

        public virtual void Awake()
        {
            attachedPhysicsBody = GetComponent<PhysicsBody>();
            CollisionManager.AddCollider(this);
        }

        public virtual void OnDestroy()
        {
            CollisionManager.RemoveCollider(this);
        }

    }
}
