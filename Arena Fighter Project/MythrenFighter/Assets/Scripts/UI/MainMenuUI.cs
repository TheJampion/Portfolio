using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MythrenFighter
{
    public class MainMenuUI : UI
    {
        // Dependencies
        [Header("Assign These")]
        [SerializeField]
        private Button onlineButton;
        [SerializeField]
        private Button quitButton;
        private MainMenuUIController mainMenuUIController;

        protected override void Awake()
        {
            base.Awake();
            mainMenuUIController = GetComponentInParent<MainMenuUIController>();
            onlineButton.onClick.AddListener(() => OnlineButtonClicked());
            quitButton.onClick.AddListener(() => QuitButtonClicked());
        }

        private void OnlineButtonClicked()
        {
            mainMenuUIController.SwapActiveUI(mainMenuUIController.lobbySelectUI);
        }

        private void QuitButtonClicked()
        {
            GameSceneManager.Instance.Quit();
        }
    }
}
