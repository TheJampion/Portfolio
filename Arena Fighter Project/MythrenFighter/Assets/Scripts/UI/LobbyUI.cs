using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MythrenFighter
{
    public class LobbyUI : UI
    {
        // Variables
        private List<PlayerInfoUI> playerInfoUIs = new List<PlayerInfoUI>();

        // Dependencies
        [Header("Assign These")]
        [SerializeField]
        private GameObject playersParentPanel;
        [SerializeField]
        private GameObject playerInfoUIPrefab;
        [SerializeField]
        private Button selectCharacterButton;
        [SerializeField]
        private Button startGameButton;
        [SerializeField]
        private Button backButton;

        private MainMenuUIController mainMenuUIController;

        protected override void Awake()
        {
            base.Awake();
            mainMenuUIController = GetComponentInParent<MainMenuUIController>();
            selectCharacterButton.onClick.AddListener(() => SelectCharacterButtonClicked());
            startGameButton.onClick.AddListener(() => StartGameButtonClicked());
            backButton.onClick.AddListener(() => BackButtonClicked());
            LobbyManager.Instance.lobbyUpdatedEvent += UpdateUI;
        }

        private void OnDestroy()
        {
            LobbyManager.Instance.lobbyUpdatedEvent -= UpdateUI;
        }

        private void SelectCharacterButtonClicked()
        {
            mainMenuUIController.SwapActiveUI(mainMenuUIController.characterSelectUI);
        }

        private void StartGameButtonClicked()
        {
            LobbyManager.Instance.UpdateLobby(new List<AttributeData> { new AttributeData { Key = "inGame", Value = true } }, new List<AttributeData>());
        }

        public override void EnableUI()
        {
            base.EnableUI();
            checkReadyToStart();
        }

        private void checkReadyToStart()
        {
            startGameButton.interactable = LobbyManager.Instance.isLobbyOwner && LobbyManager.Instance.playerInfos.Count > 1 && LobbyManager.Instance.playerInfos.All(x => x.isReady);
        }

        private void UpdateUI()
        {
            foreach (PlayerInfoUI ui in playerInfoUIs)
            {
                Destroy(ui.gameObject);
            }
            playerInfoUIs.Clear();
            foreach (PlayerInfo playerInfo in LobbyManager.Instance.playerInfos.OrderBy(x => x.playerSlot))
            {
                GameObject newUIObject = Instantiate(playerInfoUIPrefab, playersParentPanel.transform);
                PlayerInfoUI ui = newUIObject.GetComponent<PlayerInfoUI>();
                ui.UpdateUI(playerInfo);
                playerInfoUIs.Add(ui);
            }
            checkReadyToStart();
        }

        private void BackButtonClicked()
        {
            mainMenuUIController.SwapActiveUI(mainMenuUIController.lobbySelectUI);
            LobbyManager.Instance.LeaveLobby();
        }

    }
}