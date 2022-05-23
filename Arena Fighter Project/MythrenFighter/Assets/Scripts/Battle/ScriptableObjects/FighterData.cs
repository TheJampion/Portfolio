using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MythrenFighter
{
    [CreateAssetMenu(fileName = "FighterData")]
    public class FighterData : ScriptableObject
    {
        [ReadOnly]
        public string fighterId = Guid.NewGuid().ToString();
        public string fighterName = "";
        [PreviewField] public GameObject battlePrefab;
        [ValueDropdown("GetAllJoints")] public GameObject rootSkeleton;
        [FolderPath]
        public string statesPath;
        [FolderPath]
        public string attacksPath;


        [TabGroup("General","UI")] public Sprite characterSelectPortrait;
        [TabGroup("General", "Chromas")] public List<ChromaData> chromas;
        [TabGroup("General", "Constants")] public PhysicsConstants physicsConstants;
        [TabGroup("General", "Constants")] public MoveConstants moveConstants;
        [TabGroup("General", "Constants")] public List<GameObject> objectsToInstantiate;
        [HideInInspector] [TabGroup("States")] public string stateTabName;
        [TabGroup("States", "All States")] public List<FighterState> fighterStates;
        [TabGroup("States", "Any State Transitions")] public List<FighterAnyStateTransitionData> anyStateTransitions;

        private IEnumerable GetAllJoints()
        {
            Transform[] allChildren = battlePrefab.GetComponentsInChildren<Transform>();
            for (int i = 0; i < allChildren.Length; i++)
            {
                yield return allChildren[i].gameObject;
            }
        }
    }

    [Serializable]
    public class ChromaData
    {
        public List<Material> chromaMaterials;
    }
}
