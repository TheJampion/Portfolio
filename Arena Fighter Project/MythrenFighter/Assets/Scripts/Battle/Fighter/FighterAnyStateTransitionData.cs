using System;

namespace MythrenFighter
{
    public enum AnyStateTriggers
    {
        undefined,
        enterHitstun,
        enterAerialHitstun,
        enterKnockup,
        enterDeath,
        enterBounce,
        enterCrowdControl
    }

    [Serializable]
    public class FighterAnyStateTransitionData
    {
        public AnyStateTriggers trigger;
        public FighterState stateToCall;
    }
}
