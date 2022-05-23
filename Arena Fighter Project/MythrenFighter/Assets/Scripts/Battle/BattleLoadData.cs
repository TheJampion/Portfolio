using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MythrenFighter {

    [System.Serializable]
    public class PlayerSlotData
    {
        public int playerSlot = 0;
        public bool isLocal = true;
        public string fighterId = "";
    }

    [CreateAssetMenu(fileName = "BattleLoadData")]
    public class BattleLoadData : ScriptableObject
    {
        public List<PlayerSlotData> playerSlotDatas = new List<PlayerSlotData>();
        public bool isOnline;
        public BattleEnvironmentData battleEnvironment;
    }
}
