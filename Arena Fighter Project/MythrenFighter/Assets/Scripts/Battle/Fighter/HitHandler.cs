using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedPoint;

namespace MythrenFighter
{
    public class HitHandler : MonoBehaviour
    {
        public int fighterHitIndex { get; private set; }

        // Dependencies - found with GetComponent or similar
        [HideInInspector]
        public FighterStateMachine FighterStateMachine { get; private set; }
        public ShieldController ShieldController { get; private set; }

        public HealthController HealthController { get; private set; }
        public HitHandler EnemyHitHandler { get; private set; }

        public virtual void Awake()
        {
            fighterHitIndex = HitManager.registerHitHandler(this);
            FighterStateMachine = GetComponent<FighterStateMachine>();
            ShieldController = GetComponent<ShieldController>();
            HealthController = GetComponent<HealthController>();
        }

        private void OnDestroy()
        {
            HitManager.unregisterHitHandler(fighterHitIndex);
        }

        public virtual void performHit(AttackData attack)
        {
            if (attack.attackerHitlag)
            {
                FighterStateMachine.HitlagController.enterHitlag(attack.hitlagFrames / RollbackManager.FRAMERATE, AnyStateTriggers.undefined);
            }
        }

        public virtual void getHit(AttackData attack, HitHandler enemyHitHandler)
        {
            this.EnemyHitHandler = enemyHitHandler;

            // Attack hit
            Vector3 toTarget = (enemyHitHandler.transform.position - transform.position).normalized;

            transform.forward = -(enemyHitHandler.transform.position - transform.position).noY();

            if (ShieldController.shieldControllerData.IsBlocking && !attack.shieldBreak)
            {
                ShieldController.TakeShieldDamage(attack.shieldDamage.AsFloatRounded);
            }
            else
            {
                HealthController.TakeDamage(attack.damage);
                if (FighterStateMachine)
                {
                    if (FighterStateMachine.StateMachineData.MaxTimeInHelplessGrounded * RollbackManager.FRAMERATE.AsInt < attack.getupWaitFrames)
                    {
                        float previousMaxTime = FighterStateMachine.StateMachineData.MaxTimeInHelplessGrounded;
                        FighterStateMachine.StateMachineData.MaxTimeInHelplessGrounded = (float)attack.getupWaitFrames / RollbackManager.FRAMERATE.AsInt;
                        FighterStateMachine.StateMachineData.TimeInHelplessGrounded += FighterStateMachine.StateMachineData.MaxTimeInHelplessGrounded - previousMaxTime;
                    }

                    FighterStateMachine.StateMachineData.AttackLastHitBy = attack;

                    if (attack.defenderHitlag)
                    {
                        if (FighterStateMachine.FighterMove.physicsBody.isGrounded)
                        {
                            FighterStateMachine.HitlagController.enterHitlag(attack.hitlagFrames / RollbackManager.FRAMERATE, attack.triggerAfterHitlag);
                        }
                        else
                        {
                            FighterStateMachine.HitlagController.enterHitlag(attack.hitlagFrames / RollbackManager.FRAMERATE, attack.triggerAfterHitlagAerial);
                        }
                    }
                }
            }
        }
    }
}