using FixedPoint;
using System;
using System.Linq;

namespace MythrenFighter
{
    public class HitlagController
    {
        private FighterStateMachine _fighterStateMachine;
        public HitlagController(FighterStateMachine stateMachine)
        {
            _fighterStateMachine = stateMachine;
        }
        public event Action EnterHitlag;
        public void enterHitlag(fp hitlagLength, AnyStateTriggers trigger)
        {
            _fighterStateMachine.StateMachineData.IsInHitlag = true;
            _fighterStateMachine.FighterMove.physicsBody.currentState.velocity = fp3.zero;
            _fighterStateMachine.StateMachineData.HitlagLength = hitlagLength.AsFloat;
            _fighterStateMachine.AnimationController.SetAnimationSpeed(0);
            _fighterStateMachine.StateMachineData.TriggerAfterHitlag = trigger;
        }
        public void exitHitlag()
        {
            _fighterStateMachine.StateMachineData.IsInHitlag = false;
            _fighterStateMachine.AnimationController.SetAnimationSpeed(1);

            if (_fighterStateMachine.StateMachineData.TriggerAfterHitlag == AnyStateTriggers.enterHitstun)
            {
                _fighterStateMachine.enterState(_fighterStateMachine.Fighter.fighterData.anyStateTransitions.Where(o => o.trigger == AnyStateTriggers.enterHitstun).First().stateToCall);
            }
            if (_fighterStateMachine.StateMachineData.TriggerAfterHitlag == AnyStateTriggers.enterAerialHitstun)
            {
                _fighterStateMachine.enterState(_fighterStateMachine.Fighter.fighterData.anyStateTransitions.Where(o => o.trigger == AnyStateTriggers.enterAerialHitstun).First().stateToCall);
            }
            if (_fighterStateMachine.StateMachineData.TriggerAfterHitlag == AnyStateTriggers.enterKnockup)
            {
                _fighterStateMachine.enterState(_fighterStateMachine.Fighter.fighterData.anyStateTransitions.Where(o => o.trigger == AnyStateTriggers.enterKnockup).First().stateToCall);
            }
        }
    }
}