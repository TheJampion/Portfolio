using Epic.OnlineServices.Lobby;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MythrenFighter
{
    public class FighterSelectUI : UI
    {
        // Variables
        private string fighterId;

        // Dependencies
        [Header("Assign These")]
        [SerializeField]
        private Button readyUpButton;
        [SerializeField]
        private Button backButton;
        [SerializeField]
        private List<FighterPortraitUI> fighterPortraitUIs = new List<FighterPortraitUI>();

        private MainMenuUIController mainMenuUIController;

        protected override void Awake()
        {
            base.Awake();
            mainMenuUIController = GetComponentInParent<MainMenuUIController>();
            readyUpButton.onClick.AddListener(() => ReadyUpButtonClicked());
            backButton.onClick.AddListener(() => BackButtonClicked());
        }

        public override void EnableUI()
        {
            base.EnableUI();

            LobbyManager.Instance.UpdateLobby(new List<AttributeData>(), new List<AttributeData> { new AttributeData { Key = "isReady", Value = false } });

            readyUpButton.interactable = false;
            List<FighterData> fighters = GameDataManager.Instance.fighterDatabase.fighters;

            for (int i = 0; i < fighterPortraitUIs.Count; i++)
            {
                if (fighters.Count > i)
                {
                    fighterPortraitUIs[i].gameObject.SetActive(true);
                    fighterPortraitUIs[i].SetPortrait(fighters[i].fighterId, fighters[i].characterSelectPortrait);
                }
                else
                {
                    fighterPortraitUIs[i].gameObject.SetActive(false);
                }
            }
        }

        public void SetSelectedFighter(string fighterId)
        {
            this.fighterId = fighterId;
            readyUpButton.interactable = true;
        }

        private void ReadyUpButtonClicked()
        {
            LobbyManager.Instance.UpdateLobby(new List<AttributeData>(), new List<AttributeData> { new AttributeData { Key = "selectedFighterId", Value = fighterId }, new AttributeData { Key = "isReady", Value = true } });
            mainMenuUIController.SwapActiveUI(mainMenuUIController.lobbyUI);
        }

        private void BackButtonClicked()
        {
            mainMenuUIController.SwapActiveUI(mainMenuUIController.lobbyUI);
        }
    }
}