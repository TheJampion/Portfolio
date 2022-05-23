using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MythrenFighter
{
    [Serializable]
    public struct HealthData
    {
        public float currentHealth;
    }
    public class HealthController : MonoBehaviour, RollbackEntity
    {
        public float MaxHealth = 100f;
        public HealthData HealthData = new HealthData();
        public Guid id => Guid.NewGuid();

        public void TakeDamage(float damage)
        {
            HealthData.currentHealth -= damage;
            Debug.Log(HealthData.currentHealth);
        }

        public void Heal(float healAmount)
        {
            HealthData.currentHealth += healAmount;
        }
        public dynamic GetInitialState()
        {
            HealthData.currentHealth = MaxHealth;
            return HealthData;
        }

        public dynamic GetUpdatedState()
        {
            return HealthData;
        }

        public void SetState(dynamic state)
        {
            HealthData = state;
        }

        public void SimulateFrame()
        {
            return;
        }

        public void UpdateVisuals()
        {
            return;
        }
    }
}
