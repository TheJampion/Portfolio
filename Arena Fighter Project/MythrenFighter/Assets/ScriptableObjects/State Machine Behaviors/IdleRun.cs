using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedPoint;

namespace MythrenFighter
{
    [CreateAssetMenu(fileName = "IdleRun", menuName = "Fighter/StateMachineBehaviours/IdleRun")]
    public class IdleRun : FighterStateMachineBehaviour
    {
        public override void OnStateUpdate()
        {
            //fighterStateMachine.stateMachineData.blendTreeWeight = (float)inputManager.GetInputs(fighterSlot).moveInput.Magnitude();
            if ((float)fighterStateMachine.FighterMove.inputManager.GetInputs(fighterStateMachine.Fighter.playerSlot).moveInput.Magnitude() == 0f)
            {
                fighterStateMachine.StateMachineData.BlendTreeWeight = 0;
            }
            else
            {
                fighterStateMachine.StateMachineData.BlendTreeWeight = 1;
            }
        }

        public override void OnStateExit()
        {
            base.OnStateExit();
        }
    }
}