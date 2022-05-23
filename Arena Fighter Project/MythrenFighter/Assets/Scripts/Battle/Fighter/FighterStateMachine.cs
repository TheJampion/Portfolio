using FixedPoint;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MythrenFighter
{
    [Serializable]
    public struct FighterStateMachineData
    {
        public FighterState PreviousState;
        public FighterState CurrentState;
        public AnyStateTriggers TriggerAfterHitlag;
        public AttackData AttackLastHitBy;
        public AttackData AttackLastHit;
        public bool IsInHitlag;
        public float HitlagLength;
        public float TimeInHitlag;
        public bool IsInvincible;
        public float TimeInInvicbility;
        public float TimeInState;
        public int FrameStateEntered;
        public float MaxTimeInState;
        public float TimeInHelplessGrounded;
        public float MaxTimeInHelplessGrounded;
        public float BlendTreeWeight;
    }

    public class FighterStateMachine : MonoBehaviour, RollbackEntity
    {
        [HideInInspector]
        public Guid id { get; } = Guid.NewGuid();

        public FighterStateMachineData InitialStateMachineData = new FighterStateMachineData();
        public FighterStateMachineData StateMachineData;
        public bool IsInFighterEditor;
        public bool IsUsingBlendTree;
        public float TransitionBlendTime;
        private List<FighterStateMachineBehaviour> stateBehaviours = new List<FighterStateMachineBehaviour>();

        public Fighter Fighter { get; private set; }
        public FighterMove FighterMove { get; private set; }
        public AttackController AttackController { get; private set; }
        public InputManager InputManager { get; private set; }

        public HitlagController HitlagController { get; private set; }
        public FighterAnimationController AnimationController { get; private set; }

        private RollbackManager _rollbackManager;
        private Animator _animator;

        public event Action<FighterState> EnterStateAction;
        public event Action<FighterState> ExitStateAction;

        private void Awake()
        {
            Fighter = GetComponent<Fighter>();
            FighterMove = GetComponent<FighterMove>();
            AttackController = GetComponent<AttackController>();
            InputManager = FindObjectOfType<InputManager>();
            _animator = GetComponent<Animator>();
            _rollbackManager = FindObjectOfType<RollbackManager>();
            HitlagController = new HitlagController(this);
            AnimationController = new FighterAnimationController(_animator);
        }

        private void OnDisable()
        {
            RollbackManager.UnregisterRollbackEntity(this);
            AnimationController.PlayableGraph.Destroy();
        }

        public void enterState(FighterState state)
        {
            //Exit the previous state and run all statebehaviours OnStateExit
            if (StateMachineData.CurrentState != null)
            {
                for (int i = 0; i < StateMachineData.CurrentState.stateBehaviours.Count; i++)
                {
                    stateBehaviours[i].OnStateExit();
                }
                ExitStateAction?.Invoke(StateMachineData.CurrentState);
                StateMachineData.PreviousState = StateMachineData.CurrentState;
            }
            stateBehaviours.Clear();
            
            StateMachineData.IsInvincible = state.isInvincibile;
            StateMachineData.TimeInInvicbility = 0;
            StateMachineData.TimeInState = 0;
            StateMachineData.FrameStateEntered = RollbackManager.currentFrame;

            StateMachineData.CurrentState = state;

            // Update StateMachine Data for this frame to the new state
            RollbackManager.UpdateRollbackEntityOnCurrentFrame(id, StateMachineData);

            for (int i = 0; i < state.stateBehaviours.Count; i++)
            {
                stateBehaviours.Add(state.stateBehaviours[i].Clone());
                stateBehaviours[i].fighterStateMachine = this;
                stateBehaviours[i].inputManager = InputManager;
                stateBehaviours[i].fighterSlot = Fighter.playerSlot;
                stateBehaviours[i].OnStateEnter();
            }
            // Send Events for entering a state
            EnterStateAction?.Invoke(state);

            //Update Movement Data on this frame for the state
            MythrenFighter.RollbackManager.UpdateRollbackEntityOnCurrentFrame(FighterMove.id, FighterMove.GetUpdatedState());
            
            AnimationController.SetupAnimations(state);
            CalculateMaxTimeInState(state);
        }

        private void CalculateMaxTimeInState(FighterState state)
        {
            if (state.animationClips.Count == 1)
            {
                StateMachineData.MaxTimeInState = state.animationClips[0].length;
                IsUsingBlendTree = false;
            }
            else if (state.animationClips.Count > 1)
            {
                float maxLength = 0f;

                for (int i = 0; i < state.animationClips.Count - 1; i++)
                {
                    if (state.animationClips[i].length > maxLength)
                    {
                        maxLength = state.animationClips[i].length;
                    }
                }
                StateMachineData.MaxTimeInState = maxLength;
                IsUsingBlendTree = true;
            }
        }

        public dynamic GetInitialState()
        {
            return InitialStateMachineData;
        }

        public dynamic GetUpdatedState()
        {
            return StateMachineData;
        }

        public void SetState(dynamic state)
        {
            StateMachineData = state;
        }

        public void SimulateFrame()
        {
            if (StateMachineData.CurrentState != null)
            {
                if (StateMachineData.CurrentState.stateBehaviours.Count > 0)
                    for (int i = 0; i < StateMachineData.CurrentState.stateBehaviours.Count; i++)
                    {
                        stateBehaviours[i].OnStateUpdate();
                    }
                if (StateMachineData.IsInHitlag)
                {
                    StateMachineData.TimeInHitlag += (fp._1 / RollbackManager.FRAMERATE).AsFloat;
                }
                if (StateMachineData.IsInvincible)
                {
                    StateMachineData.TimeInInvicbility += (fp._1 / RollbackManager.FRAMERATE).AsFloat;
                }
            }
            else
            {
                enterState(Fighter.fighterData.fighterStates[0]);
            }
            StateMachineData.TimeInState += (float)(fp._1 / RollbackManager.FRAMERATE);
        }

        public void UpdateVisuals()
        {
            if (IsUsingBlendTree)
            {
                StateMachineData.BlendTreeWeight = Mathf.Clamp01(StateMachineData.BlendTreeWeight);
                AnimationController.PlayMixerPlayable(StateMachineData.TimeInState,StateMachineData.BlendTreeWeight);
            }
            else
            {
                AnimationController.PlayPlayable(StateMachineData.TimeInState);
            }
        }
    }
}
