using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MythrenFighter
{
    public class UIController : MonoBehaviour
    {

        // Variables
        public UI previousUI;
        public UI currentUI;

        public void SwapActiveUI(UI newUI)
        {
            previousUI = currentUI;
            currentUI = newUI;
            if (previousUI != null)
            {
                previousUI.DisableUI();
            }
            if (currentUI != null)
            {
                newUI.EnableUI();
            }
        }

        public void SwapToPreviousUI()
        {
            SwapActiveUI(previousUI);
        }
    }
}
