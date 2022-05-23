using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedPoint;

namespace MythrenFighter
{
    [Serializable]
    public struct ShieldControllerData
    {
        public float shieldHealth;
        public bool IsBlocking;
    }
    public class ShieldController : MonoBehaviour, RollbackEntity
    {
        public ShieldControllerData shieldControllerData;
        //Assigned in Inspector
        [Header("Dependencies - Needs Assignment")]
        public Material HealthyShieldMat;
        public Material MediumShieldMat;
        public Material LowShieldMat;
        public GameObject ShieldGameObject;

        public MeshRenderer shieldMesh;
        public event Action ActionBlocking;
        public Guid id { get; } = Guid.NewGuid();

        public void ActivateShield()
        {
            shieldControllerData.IsBlocking = true;
            ShieldGameObject.SetActive(true);
            RollbackManager.UpdateRollbackEntityOnCurrentFrame(id, shieldControllerData);
            ActionBlocking?.Invoke();
        }
        public void DeactivateShield()
        {
            shieldControllerData.IsBlocking = false;
            ShieldGameObject.SetActive(false);
        }

        public void TakeShieldDamage(float damage)
        {
            shieldControllerData.shieldHealth -= damage;
            shieldControllerData.shieldHealth = Mathf.Clamp(shieldControllerData.shieldHealth, 0, 100);
        }
        public dynamic GetInitialState()
        {
            shieldControllerData.shieldHealth = 100;
            return shieldControllerData;
        }

        public dynamic GetUpdatedState()
        {
            return shieldControllerData;
        }

        public void SetState(dynamic state)
        {
            shieldControllerData = state;
        }

        public void SimulateFrame()
        {
            shieldControllerData.shieldHealth = Mathf.Clamp(shieldControllerData.shieldHealth, 0f, 100f);
            if (shieldControllerData.IsBlocking)
            {
                shieldControllerData.shieldHealth -= 10f / RollbackManager.FRAMERATE.AsInt;
            }
            else
            {
                shieldControllerData.shieldHealth += 10f / RollbackManager.FRAMERATE.AsInt;
            }
        }

        public void UpdateVisuals()
        {
            if (shieldControllerData.shieldHealth >= 67f)
            {
                shieldMesh.sharedMaterial = HealthyShieldMat;
            }
            else if (shieldControllerData.shieldHealth >= 33f && shieldControllerData.shieldHealth < 67f)
            {
                shieldMesh.sharedMaterial = MediumShieldMat;
            }
            else
            {
                shieldMesh.sharedMaterial = LowShieldMat;
            }

        }
    }
}
