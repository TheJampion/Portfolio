using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MythrenFighter
{
    public class MainMenuUIController : UIController
    {
        // Dependencies
        [Header("Assign These")]
        public MainMenuUI mainMenuUI;
        public LobbySelectUI lobbySelectUI;
        public LobbyUI lobbyUI;
        public FighterSelectUI characterSelectUI;
    }
}