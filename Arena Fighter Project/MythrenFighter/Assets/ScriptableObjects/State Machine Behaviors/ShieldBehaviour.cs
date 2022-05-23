using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedPoint;
using UnityEngine.Playables;
using System;

namespace MythrenFighter
{
    [CreateAssetMenu(menuName = "Fighter/StateMachineBehaviours/Shield", fileName = "ShieldBehaviour")]
    public class ShieldBehaviour : FighterStateMachineBehaviour
    {
        private ShieldController _shieldController;

        public override void OnStateEnter()
        {
            base.OnStateEnter();
            _shieldController = fighterStateMachine.GetComponent<ShieldController>();
            _shieldController.ActivateShield();
        }

        public override void OnStateExit()
        {
            _shieldController.shieldControllerData.IsBlocking = false;
            _shieldController.DeactivateShield();
            base.OnStateExit(); 
        }
    }
}