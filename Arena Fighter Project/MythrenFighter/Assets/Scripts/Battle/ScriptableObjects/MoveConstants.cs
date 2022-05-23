using FixedPoint;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MythrenFighter {
    [CreateAssetMenu(fileName = "MoveConstants")]
    public class MoveConstants : ScriptableObject
    {
        public fp moveSpeed = 0;
        public fp jumpStrength = 0;
    }
}
