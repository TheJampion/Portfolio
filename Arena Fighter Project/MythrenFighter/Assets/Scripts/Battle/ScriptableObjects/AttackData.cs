using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using FixedPoint;

namespace MythrenFighter
{
    [CreateAssetMenu(fileName = "Mythren/Fighter/AttackData")]
    [InlineEditor(InlineEditorModes.GUIOnly)]
    public class AttackData : ScriptableObject
    {
        [TabGroup("General")] public string attackName;
        [TabGroup("General")] public float damage;
        [TabGroup("General")] [ReadOnly] public fp shieldDamage;
        [TabGroup("General")] [ReadOnly] public float chargeTime;
        [TabGroup("General")] [ReadOnly] public bool isSkill;
        [TabGroup("General")] [ReadOnly] public bool autoLock;
        [TabGroup("General")] [ReadOnly] public bool rotatesCameraToSide;
        [TabGroup("General")] [ReadOnly] public bool shieldBreak;
        [TabGroup("General")] [ReadOnly] public bool isMultihit;
        [TabGroup("General")] public bool attackerHitlag;
        [TabGroup("General")] public bool defenderHitlag;
        [TabGroup("General")] [ReadOnly] public bool hitGrounded;
        [TabGroup("General")] public AnyStateTriggers triggerAfterHitlag = AnyStateTriggers.enterHitstun;
        [TabGroup("General")] public AnyStateTriggers triggerAfterHitlagAerial = AnyStateTriggers.enterAerialHitstun;

        [TabGroup("Frame Data")] [ReadOnly] public int numberOfFrames; // This should be set based on the animation used in the state that calls this attack
        [TabGroup("Frame Data")] [ReadOnly] public int recoveryFramesStart;
        [TabGroup("Frame Data")] public int hitlagFrames;
        [TabGroup("Frame Data")] public int hitstunFrames = 0;
        [TabGroup("Frame Data")] [ReadOnly] public int shieldstunFrames;
        [TabGroup("Frame Data")] [ReadOnly] public int ccFrames;
        [TabGroup("Frame Data")] [ReadOnly] public int getupWaitFrames;
        [TabGroup("Frame Data"), ShowIf("isSkill")] public int skillInvincibilityStartupFrames;

        [TabGroup("Physics")] public fp knockUpVerticalForce = 500;
        [TabGroup("Physics")] public fp knockUpHorizontalForce = 250;
        [ReadOnly] public List<HitboxData> hitboxData = new List<HitboxData>();


        [TabGroup("Prefab Settings")] [ReadOnly] public bool destroyPrefabOnHit;
        [TabGroup("Prefab Settings")] [ReadOnly] public bool destroyPrefabOnHitEnvironment;
        [TabGroup("Prefab Settings")] [ReadOnly] public bool destroyPrefabAfterTimer;
        [TabGroup("Prefab Settings")] [ReadOnly] public int destroyPrefabFramesToWait;
        [TabGroup("Prefab Settings")] [ReadOnly] public List<ObjectInstantiationData> objectsToInstantiate = new List<ObjectInstantiationData>();

        [TabGroup("Sfx")] [ReadOnly] public AudioClip soundEffectOnHit;
        [TabGroup("Sfx")] [ReadOnly] public float soundEffectVolume;

        public void init(string name)
        {
            this.name = name;
        }

        public static AttackData CreateInstance(string name)
        {
            var state = CreateInstance<AttackData>();
            state.init(name);
            return state;
        }
    }
    [System.Serializable]
    public class HitboxData
    {
        public string anchorObjectName;
        public bool instantiatedObject;
        public List<fp3> positionData;
        public List<fp> sizeData;
        public List<fp3> offsetData;
        public List<bool> frameActiveData;

        public HitboxData(HitboxData newHitboxData)
        {
            anchorObjectName = newHitboxData.anchorObjectName;
            frameActiveData = new List<bool>(newHitboxData.frameActiveData);
            sizeData = new List<fp>(newHitboxData.sizeData);
            offsetData = new List<fp3>(newHitboxData.offsetData);
        }

        public HitboxData(int numberOfFrames, string anchorObjectName)
        {
            this.anchorObjectName = anchorObjectName;
            frameActiveData = new List<bool>();
            sizeData = new List<fp>();
            offsetData = new List<fp3>();
            positionData = new List<fp3>();
            for(int i = 0; i < numberOfFrames; i++)
            {
                frameActiveData.Add(false);
                sizeData.Add(fp._0);
                offsetData.Add(fp3.zero);
                positionData.Add(fp3.zero);
            }
        }
    }

    [System.Serializable]
    public class ObjectInstantiationData
    {
        [ReadOnly]public int objectIndex;
        public int frameDelay;
        public bool isTrap;
        [HideInInspector]
        public float chargeTime;

        public ObjectInstantiationData(int index)
        {
            objectIndex = index;
        }
    }
}