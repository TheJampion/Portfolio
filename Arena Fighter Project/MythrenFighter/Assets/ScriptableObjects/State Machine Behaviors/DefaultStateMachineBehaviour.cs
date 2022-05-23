using FixedPoint;
using UnityEngine;
using UnityEngine.Playables;

namespace MythrenFighter
{
    [CreateAssetMenu(fileName = "Mythren/Fighter/StateMachineBehaviour")]
    public class DefaultStateMachineBehaviour : FighterStateMachineBehaviour
    {
        public override void OnStateEnter()
        {
            FighterState currentState = fighterStateMachine.StateMachineData.CurrentState;

            if (currentState != null)
            {
                fighterStateMachine.FighterMove.frictionEnabled = currentState.frictionEnabled;

                if (currentState.setFriction)
                {
                    fighterStateMachine.FighterMove.fighterMovementData.frictionComponent = currentState.friction.ToFixedPoint();
                }
                else
                {
                    fighterStateMachine.FighterMove.fighterMovementData.frictionComponent = fighterStateMachine.FighterMove.defaultFriction;
                }

                fighterStateMachine.FighterMove.velocityAffectedByInput = fighterStateMachine.StateMachineData.CurrentState.initialVelocityFromMoveInput;
                fighterStateMachine.FighterMove.HandleInput(fighterStateMachine.StateMachineData.CurrentState.initialInputVelocityMagnitude);

                if (inputManager.GetInputs(fighterSlot).moveInput.Magnitude() != 0)
                {
                    fighterStateMachine.FighterMove.SetForward(inputManager.GetInputs(fighterSlot).moveInput);
                }

                if (currentState.setVelocity)
                {
                    if (currentState.velocityDirection == FighterState.VelocityDirection.Up)
                    {
                        fighterStateMachine.FighterMove.fighterMovementData.gravityComponent = fp3.up * currentState.velocityMagnitude;
                    }
                    else if (currentState.velocityDirection == FighterState.VelocityDirection.Down)
                    {
                        fighterStateMachine.FighterMove.fighterMovementData.gravityComponent = -fp3.up * currentState.velocityMagnitude;
                    }
                    else if (currentState.velocityDirection == FighterState.VelocityDirection.Forward)
                    {
                        fighterStateMachine.FighterMove.fighterMovementData.velocity = -fighterStateMachine.FighterMove.GetForward() * currentState.velocityMagnitude;
                    }
                    else if (currentState.velocityDirection == FighterState.VelocityDirection.MoveInput)
                    {
                        fighterStateMachine.FighterMove.HandleInput(currentState.velocityMagnitude);
                    }
                }
            }
        }

        public override void OnStateUpdate()
        {
            FighterState currentState = fighterStateMachine.StateMachineData.CurrentState;

            if (fighterStateMachine.StateMachineData.TimeInInvicbility >= currentState.invicibilityFrames / RollbackManager.FRAMERATE.AsInt && fighterStateMachine.StateMachineData.IsInvincible)
            {
                fighterStateMachine.StateMachineData.IsInvincible = false;
                fighterStateMachine.StateMachineData.TimeInInvicbility = 0;
            }
            fighterStateMachine.FighterMove.velocityAffectedByInput = currentState.continuousVelocityFromMoveInput;

            if (fighterStateMachine.StateMachineData.TimeInHitlag > fighterStateMachine.StateMachineData.HitlagLength && fighterStateMachine.StateMachineData.IsInHitlag)
            {
                fighterStateMachine.HitlagController.exitHitlag();
            }

            if (currentState.forwardFromInputDirection)
            {
                if (inputManager.GetInputs(fighterSlot).moveInput.Magnitude() != 0)
                {
                    fighterStateMachine.FighterMove.SetForward(inputManager.GetInputs(fighterSlot).moveInput);
                }
            }
            if (currentState.continuousVelocityFromMoveInput)
            {
                fighterStateMachine.FighterMove.HandleInput(currentState.inputVelocityMagnitude);
            }
            CheckTransitions();
        }

        private void CheckTransitions()
        {
            if (!fighterStateMachine.IsInFighterEditor)
            {
                foreach (FighterStateTransitionData transition in fighterStateMachine.StateMachineData.CurrentState.transitions)
                {
                    if (transition.stateToCall)
                    {
                        if (fighterStateMachine.StateMachineData.TimeInState >= fighterStateMachine.StateMachineData.MaxTimeInState && transition.trigger == Triggers.whenAnimationFinished)
                        {
                            fighterStateMachine.TransitionBlendTime = transition.animationBlendDuration;
                            fighterStateMachine.enterState(transition.stateToCall);
                        }
                        if (fighterStateMachine.FighterMove.inputManager.GetInputs(fighterStateMachine.Fighter.playerSlot).jumpInput && transition.trigger == Triggers.jumpInput)
                        {
                            fighterStateMachine.TransitionBlendTime = transition.animationBlendDuration;
                            fighterStateMachine.enterState(transition.stateToCall);
                        }
                        if (fighterStateMachine.FighterMove.inputManager.GetInputs(fighterStateMachine.Fighter.playerSlot).jabInput && transition.trigger == Triggers.jabInput)
                        {
                            fighterStateMachine.TransitionBlendTime = transition.animationBlendDuration;
                            fighterStateMachine.enterState(transition.stateToCall);
                        }
                        if (fighterStateMachine.FighterMove.inputManager.GetInputs(fighterStateMachine.Fighter.playerSlot).heavyInput && transition.trigger == Triggers.heavyInput)
                        {
                            fighterStateMachine.TransitionBlendTime = transition.animationBlendDuration;
                            fighterStateMachine.enterState(transition.stateToCall);
                        }
                        if (fighterStateMachine.FighterMove.inputManager.GetInputs(fighterStateMachine.Fighter.playerSlot).specialInput && transition.trigger == Triggers.specialInput)
                        {
                            fighterStateMachine.TransitionBlendTime = transition.animationBlendDuration;
                            fighterStateMachine.enterState(transition.stateToCall);
                        }
                        if (fighterStateMachine.FighterMove.inputManager.GetInputs(fighterStateMachine.Fighter.playerSlot).dashInput && transition.trigger == Triggers.dashInput)
                        {
                            fighterStateMachine.TransitionBlendTime = transition.animationBlendDuration;
                            fighterStateMachine.enterState(transition.stateToCall);
                        }
                        if (fighterStateMachine.FighterMove.inputManager.GetInputs(fighterStateMachine.Fighter.playerSlot).skill1Input && transition.trigger == Triggers.skill1Input)
                        {
                            fighterStateMachine.TransitionBlendTime = transition.animationBlendDuration;
                            fighterStateMachine.enterState(transition.stateToCall);
                        }
                        if (fighterStateMachine.FighterMove.inputManager.GetInputs(fighterStateMachine.Fighter.playerSlot).skill2Input && transition.trigger == Triggers.skill2Input)
                        {
                            fighterStateMachine.TransitionBlendTime = transition.animationBlendDuration;
                            fighterStateMachine.enterState(transition.stateToCall);
                        }
                        if (fighterStateMachine.FighterMove.inputManager.GetInputs(fighterStateMachine.Fighter.playerSlot).skill3Input && transition.trigger == Triggers.skill3Input)
                        {
                            fighterStateMachine.TransitionBlendTime = transition.animationBlendDuration;
                            fighterStateMachine.enterState(transition.stateToCall);
                        }
                        if (fighterStateMachine.FighterMove.inputManager.GetInputs(fighterStateMachine.Fighter.playerSlot).skill4Input && transition.trigger == Triggers.skill4Input)
                        {
                            fighterStateMachine.TransitionBlendTime = transition.animationBlendDuration;
                            fighterStateMachine.enterState(transition.stateToCall);
                        }
                        if (fighterStateMachine.FighterMove.inputManager.GetInputs(fighterStateMachine.Fighter.playerSlot).shieldInput && transition.trigger == Triggers.shieldInput)
                        {
                            fighterStateMachine.TransitionBlendTime = transition.animationBlendDuration;
                            fighterStateMachine.enterState(transition.stateToCall);
                        }
                        if (fighterStateMachine.FighterMove.inputManager.GetInputs(fighterStateMachine.Fighter.playerSlot).shieldReleaseInput && transition.trigger == Triggers.shieldReleaseInput)
                        {
                            fighterStateMachine.TransitionBlendTime = transition.animationBlendDuration;
                            fighterStateMachine.enterState(transition.stateToCall);
                        }
                        if (fighterStateMachine.FighterMove.physicsBody.isGrounded && fighterStateMachine.StateMachineData.TimeInState >= 0.05f && transition.trigger == Triggers.whenGrounded)
                        {
                            fighterStateMachine.TransitionBlendTime = transition.animationBlendDuration;
                            fighterStateMachine.enterState(transition.stateToCall);
                        }
                    }
                }
            }
            else
            {
                if (!fighterStateMachine.AnimationController.CurrentPlayable.IsNull())
                {
                    if (fighterStateMachine.StateMachineData.TimeInState >= fighterStateMachine.AnimationController.CurrentPlayable.GetAnimationClip().length)
                    {
                        fighterStateMachine.enterState(fighterStateMachine.StateMachineData.CurrentState);
                        fighterStateMachine.FighterMove.physicsBody.currentState.position = fp3.zero;
                    }
                }
                else if (fighterStateMachine.StateMachineData.TimeInState >= fighterStateMachine.AnimationController.CurrentMixerPlayable.GetDuration())
                {
                    fighterStateMachine.enterState(fighterStateMachine.StateMachineData.CurrentState);
                    fighterStateMachine.FighterMove.physicsBody.currentState.position = fp3.zero;
                }
            }
        }

        public override void OnStateExit()
        {
            fighterStateMachine.FighterMove.velocityAffectedByInput = false;
        }
    }
}