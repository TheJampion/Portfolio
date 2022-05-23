using System.Collections;
using System.Collections.Generic;
using System;
using FixedPoint;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace MythrenFighter
{
    [Serializable]
    public struct StateMachineData
    {
        public bool inHitlag;
        public float hitlagLength;
        public float timeInHitlag;
        public float timeInState;
        public int frameStateEntered;
    }

    public class StateMachine : MonoBehaviour, RollbackEntity
    {
        public Guid id { get; } = Guid.NewGuid();
        public StateMachineData stateMachineData;
        public AnimationClip animation;
        private Animator animator;
        private PlayableGraph playableGraph;
        private PlayableOutput playableOutput;
        private Playable currentPlayable;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            playableGraph = PlayableGraph.Create();
            playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", animator);
            playableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
        }

        private void OnEnable()
        {
            if (animation)
            {
                currentPlayable = AnimationClipPlayable.Create(playableGraph, animation);
                playableOutput.SetSourcePlayable(currentPlayable);
            }
            else
            {
                Debug.Log("There is no animation assigned to this gameobject: " + gameObject.name);
            }
        }

        private void OnDisable()
        {
            RollbackManager.UnregisterRollbackEntity(this);
            playableGraph.Destroy();
        }

        public dynamic GetInitialState()
        {
            return new StateMachineData();
        }

        public dynamic GetUpdatedState()
        {
            return stateMachineData;
        }

        public void SetState(dynamic state)
        {
            stateMachineData = state;
        }

        public void SimulateFrame()
        {
            if (stateMachineData.inHitlag)
            {
                stateMachineData.timeInHitlag += (fp._1 / RollbackManager.FRAMERATE).AsFloat;
            }

            stateMachineData.timeInState += (float)(fp._1 / RollbackManager.FRAMERATE);
        }

        public void UpdateVisuals()
        {
            playableGraph.Evaluate((float)(fp._1 / RollbackManager.FRAMERATE)); // add something to make the playable graph go backwards when rewinding for replays/editing
            if (!currentPlayable.IsNull())
            {
                currentPlayable.SetTime(stateMachineData.timeInState);
            }
        }
    }

}
