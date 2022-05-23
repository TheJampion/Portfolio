using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedPoint;
using System;
using Sirenix.OdinInspector;

namespace MythrenFighter
{
    [Serializable]
    public struct PhysicsBodyData
    {
        public fp3 position;
        public fp3 velocity;
        public fp3 pushbackComponent;

        public PhysicsBodyData(fp3 position, fp3 velocity, fp3 pushbackComponent)
        {
            this.position = position;
            this.velocity = velocity;
            this.pushbackComponent = pushbackComponent;
        }
    }

    public class PhysicsBody : MonoBehaviour, RollbackEntity
    {
        public static readonly fp TERMINAL_VELOCITY = -1000;
        public static readonly fp PUSHBACK_STRENGTH = 1.5f.ToFixedPoint();
        public static readonly fp SPEED = 4;
        public static readonly fp GROUND_RAYCAST_OFFSET = fp._0_50;
        public static readonly fp FLOOR_HEIGHT = 0;

        // Variables
        [HideInInspector]
        public Guid id { get; } = Guid.NewGuid();
        [ReadOnly] public PhysicsConstants physicsConstants = null;
        private Fighter fighter;

        // Dependencies
        [Header("Assign These")]
        public PhysicsBodyData initialState = new PhysicsBodyData(fp3.up * 5, fp3.zero, fp3.zero);
        public PhysicsBodyData currentState;
        public bool isGrounded;
        public fp groundPosition;

        private void Start()
        {
            fighter = GetComponent<Fighter>();
            currentState.position = transform.position.ToFp3();
            if(fighter != null)
            {
                physicsConstants = fighter.fighterData.physicsConstants;
            }
        }

        private void OnDestroy()
        {
            RollbackManager.QueueUnregisterRollbackEntity(this);
        }


        private void CheckGrounded()
        {
            if(physicsConstants.gravity != 0)
            {
                isGrounded = currentState.position.y == groundPosition;
                //Commented out due to box collider raycast logic not being setup. This should be the setup once this is fixed
                //RaycastHit hitInfo;
                //LayerMask layerMask = LayerMask.NameToLayer("Ground");
                //isGrounded = CollisionUtilities.Raycast(transform.position.ToFp3(), -fp3.up, GROUND_RAYCAST_OFFSET, layerMask, out hitInfo);
                //groundPosition = hitInfo.point.y;
            }
        }

        private void FixGroundPosition()
        {
            if(currentState.position.y < groundPosition && physicsConstants.gravity!= 0)
            {
                currentState.position.y = groundPosition;
            }
        }

        private void CalculateVelocity()
        {
            currentState.velocity += currentState.pushbackComponent / RollbackManager.FRAMERATE;
        }

        public dynamic GetInitialState()
        {
            return currentState;
        }

        public void SetState(dynamic state)
        {
            currentState = state;
        }

        public void SimulateFrame()
        {
            if (physicsConstants)
            {
                CheckGrounded();
            }
            CalculateVelocity();
            currentState.position = currentState.position + currentState.velocity;
            FixGroundPosition();
        }

        public dynamic GetUpdatedState()
        {
            return currentState;
        }

        public void UpdateVisuals()
        {
            transform.position = currentState.position.ToVector3();
        }
    }
}
