using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MythrenFighter;
using System;
namespace MythrenFighter
{

    [Serializable]
    public struct HitboxOnTriggerData
    {
        public bool isActive;
    }

    public class HitboxOnTrigger : MonoBehaviour, RollbackEntity
    {
        public Guid id { get; } = Guid.NewGuid();

        public AttackData attack;
        public AttackController attackController;
        public int attackerIndex;
        public int defenderIndex;
        private Collider collider;
        private MeshRenderer meshRenderer;
        public HitboxOnTriggerData hitboxOnTriggerData;

        private void Awake()
        {
            collider = GetComponent<Collider>();
            meshRenderer = GetComponent<MeshRenderer>();
        }


        public void Start()
        {
            collider.onTriggerEnter += AttackHit;
        }
        private void OnDestroy()
        {
            RollbackManager.QueueUnregisterRollbackEntity(this);
        }

        public void AttackHit(MythrenFighter.Collider other)
        {
            int otherHitIndex = other.GetComponentInParent<HitHandler>().fighterHitIndex;
            FighterStateMachine stateMachine = other.GetComponentInParent<FighterStateMachine>();

            if (hitboxOnTriggerData.isActive && attackerIndex != otherHitIndex && !stateMachine.StateMachineData.IsInvincible && attackController.hitAllowedOn(otherHitIndex))
            {
                attackController.addHitDisallowedIndex(otherHitIndex);
                HitManager.reportHit(attackerIndex, otherHitIndex, attack);
                collider.onTriggerEnter -= AttackHit;
            }
        }

        public dynamic GetInitialState()
        {
            return hitboxOnTriggerData;
        }

        public void SetState(dynamic state)
        {
            hitboxOnTriggerData = state;
            meshRenderer.enabled = hitboxOnTriggerData.isActive;
        }

        public void SimulateFrame()
        {
            meshRenderer.enabled = hitboxOnTriggerData.isActive;
        }

        public dynamic GetUpdatedState()
        {
            return hitboxOnTriggerData;
        }

        public void UpdateVisuals()
        {
           
        }
    }

}
