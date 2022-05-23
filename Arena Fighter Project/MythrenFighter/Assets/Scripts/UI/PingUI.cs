using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MythrenFighter {
    public class PingUI : UI
    {
        // Dependencies
        [Header("Assign These")]
        [SerializeField]
        private TextMeshProUGUI pingText;

        private void Update()
        {
            pingText.text = Mathf.RoundToInt(LobbyManager.Instance.averagePing).ToString();
        }
    }
}
