using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MythrenFighter.FighterEditor
{
    [System.Serializable]
    public class FighterEditorData : MonoBehaviour
    {
        [ReadOnly]public GameObject hitboxPrefab;
        [ReadOnly]public FighterData currentFighterData;
        [ReadOnly]public FighterStateMachineBehaviour defaultStateBehaviour;
        [ReadOnly]public Cinemachine.CinemachineVirtualCamera editorCamera;
    }

    [System.Serializable]
    public class DisplayedHitboxData
    {
        public GameObject AnchorObject;
        public float SizeData;
        public Vector3 OffsetData;
        public bool FrameActiveData;

        public DisplayedHitboxData(GameObject anchorObject)
        {
            this.AnchorObject = anchorObject;
            SizeData = 0;
            OffsetData = Vector3.zero;
            FrameActiveData = false;
        }
    }

    [System.Serializable]
    public class ObjectToInstantiateData
    {
        public GameObject Prefab;
        public int FrameDelay;
    }
}