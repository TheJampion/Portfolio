using Epic.OnlineServices.Lobby;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MythrenFighter {
    public class LobbyDetailsUI : MonoBehaviour
    {
        // Dependencies
        [Header("Assign These")]
        [SerializeField]
        private TextMeshProUGUI lobbyNameText;
        [SerializeField]
        private TextMeshProUGUI lobbyMemberCountText;
        [SerializeField]
        private Button joinButton;

        public void UpdateUI(LobbyDetails lobbyDetails)
        {
            lobbyNameText.text = "Lobby Name";
            lobbyMemberCountText.text = lobbyDetails.GetMemberCount(new LobbyDetailsGetMemberCountOptions()).ToString() + "/" + LobbyManager.MAX_LOBBY_MEMBER_COUNT;
            joinButton.onClick.AddListener(() => LobbyManager.Instance.JoinLobby(lobbyDetails));
        }
    
    }
}
