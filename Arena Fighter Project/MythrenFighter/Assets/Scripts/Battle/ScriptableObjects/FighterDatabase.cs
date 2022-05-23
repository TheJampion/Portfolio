using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MythrenFighter {
    [CreateAssetMenu(fileName = "FighterDatabase")]
    public class FighterDatabase : ScriptableObject
    {
        public List<FighterData> fighters;
    }
}
