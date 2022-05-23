using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MythrenFighter
{
    public enum Triggers
    {
        empty,
        whenAnimationFinished,
        whenGrounded,
        moveInput,
        skillModifierInput,
        jabInput,
        jumpInput,
        dashInput,
        heavyInput,
        specialInput,
        shieldInput,
        skill1Input,
        skill2Input,
        skill3Input,
        skill4Input,
        swapInput,
        shieldReleaseInput,
    }

    [Serializable]
    public class FighterStateTransitionData
    {
        [HideInInspector]public FighterData fighterData;
        public Triggers trigger;
        public bool isHoldInput; // Check to make sure button is held for a certain number of frames before transitioning
        public bool isOnHitCancelInput; // Check to see if attack hit before allowing for transition
        public bool waitForRecoveryFramesStart; // Check to see if recovery frames start before allowing for the transition
        [ValueDropdown("GetListOfStates")]public FighterState stateToCall;
        public float animationBlendDuration;

        public IEnumerable<FighterState> GetListOfStates()
        {
            FighterData currentFighterData = UnityEngine.Object.FindObjectOfType<FighterEditor.FighterEditorData>().currentFighterData;
            if(currentFighterData != null)
            {
                fighterData = currentFighterData;
                return currentFighterData.fighterStates;
            }
            else
            {
                return fighterData.fighterStates;
            }
        }
    }
}