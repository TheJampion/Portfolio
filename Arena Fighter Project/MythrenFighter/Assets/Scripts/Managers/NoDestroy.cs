using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MythrenFighter {
    public class NoDestroy : MonoBehaviour
    {

        public static NoDestroy Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

    }
}
