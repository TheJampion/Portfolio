using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using FixedPoint;
namespace MythrenFighter
{
    [CreateAssetMenu(fileName = "FighterState")]
    public class FighterState : SerializedScriptableObject
    {
        public enum VelocityDirection { Forward, Backward, Right, Left, Up, Down, MoveInput }

        [TabGroup("StateDetails", "General")] public bool isInvincibile;
        [TabGroup("StateDetails", "General"), ShowIf("@isInvincibile")] public int invicibilityFrames;
        [TabGroup("StateDetails", "Forward")] public bool forwardFromInitialInputDirection;
        [TabGroup("StateDetails", "Forward")] public bool forwardFromInputDirection;
        [TabGroup("StateDetails", "Forward")] public bool forwardFixed;
        [TabGroup("StateDetails", "Forward")] public bool forwardFromVelocity;
        [TabGroup("StateDetails", "Velocity")] public bool setVelocity;
        [TabGroup("StateDetails", "Velocity"), ShowIf("@setVelocity")] public fp velocityMagnitude;
        [TabGroup("StateDetails", "Velocity")] public VelocityDirection velocityDirection;
        [TabGroup("StateDetails", "Velocity")] public bool initialVelocityFromMoveInput;
        [TabGroup("StateDetails", "Velocity"), ShowIf("@initialVelocityFromMoveInput")] public fp initialInputVelocityMagnitude;
        [TabGroup("StateDetails", "Velocity")] public bool continuousVelocityFromMoveInput;
        [TabGroup("StateDetails", "Velocity"), ShowIf("@continuousVelocityFromMoveInput")] public fp inputVelocityMagnitude;

        [TabGroup("StateDetails", "Physics")] public bool pushbackEnabled = true;
        [TabGroup("StateDetails", "Physics")] public bool gravityEnabled = true;
        [TabGroup("StateDetails", "Physics")] public bool setGravity;
        [TabGroup("StateDetails", "Physics"), ShowIf("@setGravity")]
        [TabGroup("StateDetails", "Physics")] public float gravity;
        [TabGroup("StateDetails", "Physics")] public bool setMaxFallSpeed;
        [TabGroup("StateDetails", "Physics"), ShowIf("@setMaxFallSpeed")]
        [TabGroup("StateDetails", "Physics")] public float maxFallSpeed;
        [TabGroup("StateDetails", "Physics")] public bool setGravityComponent;
        [TabGroup("StateDetails", "Physics"), ShowIf("@setGravityComponent")]
        [TabGroup("StateDetails", "Physics")] public float gravityComponent;
        [TabGroup("StateDetails", "Physics")] public bool frictionEnabled;
        [TabGroup("StateDetails", "Physics")] public bool setFriction;
        [TabGroup("StateDetails", "Physics"), ShowIf("setFriction")]
        [TabGroup("StateDetails", "Physics")] public float friction;

        [TabGroup("StateDetails", "Attack"), HideLabel]
        [InfoBox("Leave this empty if there is no attack for this state", VisibleIf = "@!attackData", InfoMessageType = InfoMessageType.Warning)]
        public AttackData attackData;

        [TabGroup("StateDetails", "Animation")] [InlineEditor(InlineEditorModes.LargePreview, ObjectFieldMode = InlineEditorObjectFieldModes.Boxed)] public List<AnimationClip> animationClips;

        [TabGroup("StateDetails", "Transitions")] public List<FighterStateTransitionData> transitions;
        [TabGroup("StateDetails", "Behaviours")] public List<FighterStateMachineBehaviour> stateBehaviours;

        public void init(string name)
        {
            this.name = name;
        }

        public static FighterState CreateInstance(string name, AnimationClip animation)
        {
            var state = CreateInstance<FighterState>();
            state.init(name);
            state.animationClips = new List<AnimationClip>();
            if(animation != null)
            {
                state.animationClips.Add(animation);
            }
            state.animationClips.Add(animation);
            return state;
        }
    }
}