using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using FixedPoint;

namespace MythrenFighter
{
    [Serializable]
    public struct FighterMovementData
    {
        public fp3 inputComponent;
        public fp3 gravityComponent;
        public fp3 pushbackComponent;
        public fp frictionComponent;
        public fp3 velocity;
    }

    public class FighterMove : MonoBehaviour, RollbackEntity
    {
        [HideInInspector]
        public Guid id { get; } = Guid.NewGuid();
        public bool velocityAffectedByInput;
        public bool frictionEnabled;
        public fp defaultFriction;
        public fp currentFriction;

        private Fighter fighter;
        public PhysicsBody physicsBody;
        private BattleCamera battleCamera;
        public InputManager inputManager;
        public FighterMovementData fighterMovementData;
        private FighterStateMachine fighterStateMachine;
        public PhysicsConstants physicsConstants;
        public MoveConstants moveConstants;

        private void Awake()
        {
            fighter = GetComponent<Fighter>();
            physicsBody = GetComponent<PhysicsBody>();
            inputManager = FindObjectOfType<InputManager>();
            battleCamera = FindObjectOfType<BattleCamera>();
            fighterStateMachine = GetComponent<FighterStateMachine>();
            if (fighter != null)
            {
                physicsConstants = fighter.fighterData.physicsConstants;
                moveConstants = fighter.fighterData.moveConstants;
            }
            defaultFriction = physicsConstants.friction;
        }

        public dynamic GetInitialState()
        {
            return new FighterMovementData();
        }

        public void SetForward(fp3 forward)
        {
            transform.forward = -forward.ToVector3();
        }

        public fp3 GetForward()
        {
            return transform.forward.normalized.ToFp3();
        }

        private void SetGravity()
        {
            if (!physicsBody.isGrounded)
            {
                fighterMovementData.gravityComponent -= fp3.up * physicsConstants.gravity;
                fighterMovementData.gravityComponent.y = fixmath.Clamp(fighterMovementData.gravityComponent.y, PhysicsBody.TERMINAL_VELOCITY, fp.usable_max);
            }
        }

        public void HandleInput(fp customVelocity) // This function needs to be reworked to work for air and ground. Currently uses the mythren's move speed and not aerial drift speed if in the air
        {
            if (velocityAffectedByInput)
            {
                fighterMovementData.inputComponent = inputManager.GetInputs(fighterStateMachine.Fighter.playerSlot).moveInput.Normalize() * customVelocity;
            }
            else
            {
                fighterMovementData.inputComponent = fp3.zero;
            }
        }

        private void applyFriction()
        {
            if (fighterMovementData.velocity.Magnitude() > fighterMovementData.frictionComponent)
            {
                fighterMovementData.velocity = (fighterMovementData.velocity.Magnitude() - fighterMovementData.frictionComponent) * fighterMovementData.velocity.Normalize();
            }
            else
            {
                fighterMovementData.velocity = fp3.zero;
            }
        }

        private void moveFighter()
        {
            physicsBody.currentState.velocity = (fighterMovementData.velocity + fighterMovementData.inputComponent + fighterMovementData.gravityComponent) / RollbackManager.FRAMERATE;
        }

        public void SetState(dynamic state)
        {
            fighterMovementData = state;
        }

        public void SimulateFrame()
        {
            if (frictionEnabled)
            {
                applyFriction();
            }
            SetGravity();
            moveFighter();
        }

        public dynamic GetUpdatedState()
        {
            return fighterMovementData;
        }

        public void UpdateVisuals()
        {

        }
    }
}
