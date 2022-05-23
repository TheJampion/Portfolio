using FixedPoint;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MythrenFighter
{
    [CreateAssetMenu(fileName = "PhysicsConstants")]
    public class PhysicsConstants : ScriptableObject
    {
        public fp gravity = 0;
        public fp friction = 0;
    }
}
