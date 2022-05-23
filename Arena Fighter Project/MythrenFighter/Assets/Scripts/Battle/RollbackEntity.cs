using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MythrenFighter
{
    public interface RollbackEntity
    {
        public Guid id { get; }
        public GameObject gameObject { get; }
        public dynamic GetInitialState();
        public void SetState(dynamic state);
        public void SimulateFrame();
        public dynamic GetUpdatedState();
        public void UpdateVisuals();
    }
}
