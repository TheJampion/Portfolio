using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MythrenFighter {
    public class GameDataManager : MonoBehaviour
    {
        public static GameDataManager Instance { get; private set; }

        [SerializeField] public BattleLoadData battleLoadData;

        // Dependencies
        [Header("Assign These")]
        public FighterDatabase fighterDatabase;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }
    }
}
