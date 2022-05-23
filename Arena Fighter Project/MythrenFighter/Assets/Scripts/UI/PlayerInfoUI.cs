using Epic.OnlineServices;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MythrenFighter {

    public class PlayerInfoUI : MonoBehaviour
    {
        // Dependencies
        [Header("Assign These")]
        [SerializeField]
        private TextMeshProUGUI playerNameText;
        [SerializeField]
        private GameObject ownerImage;
        [SerializeField]
        private GameObject readyCheckImage;

        public void UpdateUI(PlayerInfo playerInfo)
        {
            playerNameText.text = playerInfo.userName;
            ownerImage.SetActive(playerInfo.isLobbyOwner);
            readyCheckImage.SetActive(playerInfo.isReady);
        }
    
    }
}
