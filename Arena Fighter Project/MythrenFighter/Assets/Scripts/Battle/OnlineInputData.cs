using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MythrenFighter {
    [System.Serializable]
    public class OnlineInputData
    {
        public int playerSlot = 0;
        public int frame = 0;
        public FighterInputs fighterInputs = new FighterInputs();
    }
}
