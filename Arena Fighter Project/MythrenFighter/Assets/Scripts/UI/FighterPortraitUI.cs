using Epic.OnlineServices.Lobby;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MythrenFighter {
    public class FighterPortraitUI : MonoBehaviour
    {

        // Variables
        private string fighterId;

        // Dependencies
        [Header("Assign These")]
        [SerializeField]
        private Image portrait;
        [SerializeField]
        private Button portraitButton;

        private FighterSelectUI fighterSelectUI;

        private void Awake()
        {
            fighterSelectUI = GetComponentInParent<FighterSelectUI>();
            portraitButton.onClick.AddListener(() => PortraitButtonClicked());
        }

        public void SetPortrait(string fighterId, Sprite portrait)
        {
            this.fighterId = fighterId;
            this.portrait.sprite = portrait;
        }

        private void PortraitButtonClicked()
        {
            fighterSelectUI.SetSelectedFighter(fighterId);
        }

    }
}
