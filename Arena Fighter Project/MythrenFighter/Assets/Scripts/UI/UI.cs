using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MythrenFighter
{
    public class UI : MonoBehaviour
    {
        // Variables
        [HideInInspector]
        public bool uiEnabled = false;
        public bool canExit = false;

        // Dependencies
        [Header("Assign These")]
        [SerializeField]
        protected bool enabledOnAwake = false;
        [SerializeField]
        protected GameObject childPanel;

        protected virtual void Awake()
        {
            if (enabledOnAwake)
            {
                EnableUI();
            }
        }

        public virtual void EnableUI()
        {
            if (childPanel != null)
            {
                childPanel.SetActive(true);
            }
            uiEnabled = true;
        }

        public virtual void DisableUI()
        {
            if (childPanel != null)
            {
                childPanel.SetActive(false);
            }
            uiEnabled = false;
        }

    }
}
