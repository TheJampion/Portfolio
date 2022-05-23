using Epic.OnlineServices.Lobby;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MythrenFighter
{
    public class LobbySelectUI : UI
    {
        // Variables
        private List<LobbyDetailsUI> lobbyDetailsUIs = new List<LobbyDetailsUI>();

        // Dependencies
        [Header("Assign These")]
        [SerializeField]
        private Button createLobbyButton;
        [SerializeField]
        private Button refreshButton;
        [SerializeField]
        private Button backButton;
        [SerializeField]
        private GameObject lobbyListParent;
        [SerializeField]
        private GameObject lobbyDetailsPrefab;

        private MainMenuUIController mainMenuUIController;

        protected override void Awake()
        {
            base.Awake();
            mainMenuUIController = GetComponentInParent<MainMenuUIController>();
            createLobbyButton.onClick.AddListener(() => CreateLobbyButtonClicked());
            refreshButton.onClick.AddListener(() => RefreshButtonClicked());
            backButton.onClick.AddListener(() => BackButtonClicked());
            LobbyManager.Instance.lobbyCreatedEvent += LobbyCreatedHandler;
            LobbyManager.Instance.lobbyJoinedEvent += LobbyJoinedHandler;
            LobbyManager.Instance.lobbySearchCompleteEvent += LobbySearchCompleteHandler;
        }

        private void OnDestroy()
        {
            LobbyManager.Instance.lobbyCreatedEvent -= LobbyCreatedHandler;
            LobbyManager.Instance.lobbyJoinedEvent -= LobbyJoinedHandler;
            LobbyManager.Instance.lobbySearchCompleteEvent -= LobbySearchCompleteHandler;
        }

        public override void EnableUI()
        {
            base.EnableUI();
            LobbyManager.Instance.SearchLobbies();
        }

        private void CreateLobbyButtonClicked()
        {
            LobbyManager.Instance.CreateLobby();
        }

        private void RefreshButtonClicked()
        {
            LobbyManager.Instance.SearchLobbies();
        }

        private void BackButtonClicked()
        {
            mainMenuUIController.SwapActiveUI(mainMenuUIController.mainMenuUI);
        }

        private void LobbyCreatedHandler()
        {
            mainMenuUIController.SwapActiveUI(mainMenuUIController.lobbyUI);
        }

        private void LobbyJoinedHandler()
        {
            mainMenuUIController.SwapActiveUI(mainMenuUIController.lobbyUI);
        }

        private void LobbySearchCompleteHandler()
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            foreach (LobbyDetailsUI ui in lobbyDetailsUIs)
            {
                Destroy(ui.gameObject);
            }
            lobbyDetailsUIs.Clear();
            foreach (LobbyDetails details in LobbyManager.Instance.currentLobbySearch)
            {
                GameObject newUIObject = Instantiate(lobbyDetailsPrefab, lobbyListParent.transform);
                LobbyDetailsUI ui = newUIObject.GetComponent<LobbyDetailsUI>();
                ui.UpdateUI(details);
                lobbyDetailsUIs.Add(ui);
            }
        }

    }
}
