using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedPoint;
namespace MythrenFighter
{
    [System.Serializable]
    public class FighterInputs
    {
        public bool jumpInput = false;
        public bool jabInput = false;
        public bool heavyInput = false;
        public bool specialInput = false;
        public bool dashInput = false;
        public bool skill1Input = false;
        public bool skill2Input = false;
        public bool skill3Input = false;
        public bool skill4Input = false;
        public bool skillModifierInput = false;
        public bool shieldInput = false;
        public bool shieldReleaseInput = false;
        public fp3 moveInput = fp3.zero;
    }
}