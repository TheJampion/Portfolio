using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System.Linq;
using FixedPoint;


namespace MythrenFighter.FighterEditor
{
    [System.Serializable]
    public class FighterAttackEditor
    {
        //Hidden Attributes
        public AttackData AttackData { get; set; }
        public GameObject CurrentFighter { private get; set; }
        public bool IsSettingUpAttack { get; set; }
        public FighterStateMachine SelectedFighterStateMachine { private get; set; }
        public RollbackManager RollbackManager { private get; set; }
        public FighterStateEditor StateEditor { private get; set; }

        //Visible Attributes
        [ReadOnly] public GameObject RootSkeleton;
        [ReadOnly] public int currentFrame;

        //Editable Attributes
        [TableList(AlwaysExpanded = true)]
        [OnValueChanged("UpdateHitboxData", IncludeChildren = true), OnCollectionChanged(Before = "UpdateHitboxDataCollection")]
        [CustomContextMenu("Set All Active", "SetAllHitboxesActive")]
        [CustomContextMenu("Set All Inactive", "SetAllHitboxesInactive")]
        [CustomContextMenu("Set Offset on All Frames", "SetOffsetForAllFrames")]
        [CustomContextMenu("Set Size on All Frames", "SetSizeForAllFrames")]
        [CustomContextMenu("Set Hitboxes True on Frames(#- #)", "SetHitboxesActiveOnFrameRange")]
        public List<DisplayedHitboxData> DisplayedHitboxData = new List<DisplayedHitboxData>();
        [ValueDropdown("GetAllJoints")][InlineButton("AddHitbox")] public Transform HitboxAnchor;

        [Title("Extra Objects")]
        [InlineButton("AddNewObject")] public GameObject ObjectToAdd;
        [ValueDropdown("GetAllExistingObjects")][InlineButton("AddExistingObject")] public GameObject ExistingObjectToAdd;
        [ValueDropdown("GetAllObjects")][InlineButton("AddObjectHitbox")] public Transform ObjectHitboxAnchor;
        [OnValueChanged("UpdateObjectData", IncludeChildren = true), OnCollectionChanged(Before = "UpdateObjectsToInstantiate")]
        [ListDrawerSettings(HideAddButton = true, DraggableItems = false)]
        public List<ObjectInstantiationData> ObjectsToInstantiate = new List<ObjectInstantiationData>();

        private IEnumerable GetAllJoints()
        {
            if (RootSkeleton != null)
            {
                List<ValueDropdownItem> values = new List<ValueDropdownItem>();
                System.Func<Transform, string> getPath = null;
                getPath = x => (x ? getPath(x.parent) + "/" + x.gameObject.name : "");

                return RootSkeleton.GetComponentsInChildren<Transform>().Select(x => new ValueDropdownItem(getPath(x.transform), x)).Where(x => !x.Value.ToString().Contains("Hitbox")).Concat(SelectedFighterStateMachine.AttackController.instantiatedObjects.Select(x => new ValueDropdownItem(x.gameObject.name, x.transform)));
            }
            else
            {
                return null;
            }
        }

        private IEnumerable GetAllObjects()
        {
            if (SelectedFighterStateMachine.Fighter.fighterData.objectsToInstantiate.Count > 0)
            {
                List<ValueDropdownItem> values = new List<ValueDropdownItem>();
                System.Func<Transform, string> getPath = null;
                getPath = x => (x ? getPath(x.parent) + "/" + x.gameObject.name : "");
                foreach (GameObject gameObject in SelectedFighterStateMachine.Fighter.fighterData.objectsToInstantiate)
                {
                    values = gameObject.GetComponentsInChildren<Transform>().Select(x => new ValueDropdownItem(getPath(x.transform), x)).Where(x => !x.Value.ToString().Contains("Hitbox")).ToList();
                }
                return values;
            }
            else
            {
                return null;
            }
        }

        private IEnumerable GetAllExistingObjects()
        {
            if (SelectedFighterStateMachine.Fighter.fighterData.objectsToInstantiate.Count > 0)
            {
                List<ValueDropdownItem> values = new List<ValueDropdownItem>();
                foreach (GameObject gameObject in SelectedFighterStateMachine.Fighter.fighterData.objectsToInstantiate)
                {
                    values.Add(new ValueDropdownItem(gameObject.name, gameObject));
                }
                return values;
            }
            else
            {
                return null;
            }
        }

        public void SetupAttack()
        {
            Time.timeScale = 0;
            //Clear the current hitboxData in the attack editor
            DisplayedHitboxData.Clear();
            ObjectsToInstantiate.Clear();

            if (AttackData != null)
            {
                //Setup the hitboxData based on what is in the AttackEditor
                for (int i = 0; i < AttackData.hitboxData.Count; i++)
                {
                    GameObject hitboxAnchor = Object.FindObjectsOfType<GameObject>(true).Where(obj => obj.name == AttackData.hitboxData[i].anchorObjectName).First();

                    if (hitboxAnchor)
                    {
                        DisplayedHitboxData.Add(new DisplayedHitboxData(hitboxAnchor));
                    }
                    else
                    {
                        Debug.LogError("There is no hitbox anchor that matches this name: " + AttackData.hitboxData[i].anchorObjectName);
                    }
                }

                //Check if the frame data for the attack has changed (meaning the animation has been updated with new frame data), if so adjust the hitboxdata accordingly
                foreach (HitboxData hitboxData in AttackData.hitboxData)
                {
                    if (hitboxData.frameActiveData.Count < AttackData.numberOfFrames)
                    {
                        int framesNeeded = AttackData.numberOfFrames - hitboxData.frameActiveData.Count;
                        for (int i = 0; i < framesNeeded; i++)
                        {
                            hitboxData.frameActiveData.Add(false);
                            hitboxData.sizeData.Add(fp._0);
                            hitboxData.offsetData.Add(fp3.zero);
                            hitboxData.positionData.Add(fp3.zero);
                        }
                    }
                    else if (hitboxData.frameActiveData.Count > AttackData.numberOfFrames)
                    {
                        int finalFrame = AttackData.numberOfFrames - 1;
                        if (hitboxData.positionData.Count > AttackData.numberOfFrames)
                            hitboxData.positionData.RemoveRange(finalFrame, hitboxData.positionData.Count - AttackData.numberOfFrames);
                        if (hitboxData.sizeData.Count > AttackData.numberOfFrames)
                            hitboxData.sizeData.RemoveRange(finalFrame, hitboxData.sizeData.Count - AttackData.numberOfFrames);
                        if (hitboxData.offsetData.Count > AttackData.numberOfFrames)
                            hitboxData.offsetData.RemoveRange(finalFrame, hitboxData.offsetData.Count - AttackData.numberOfFrames);
                        if (hitboxData.frameActiveData.Count > AttackData.numberOfFrames)
                            hitboxData.frameActiveData.RemoveRange(finalFrame, hitboxData.frameActiveData.Count - AttackData.numberOfFrames);
                    }
                }
                //Check for objects that need to be instantiated
                foreach (ObjectInstantiationData objectData in AttackData.objectsToInstantiate)
                {
                    ObjectsToInstantiate.Add(objectData);
                }
                IsSettingUpAttack = true;
            }
            Time.timeScale = 1;
        }

        public void UpdateHitboxDataCollection(CollectionChangeInfo info, object value)
        {
            if (info.ChangeType == CollectionChangeType.RemoveIndex)
            {
                Debug.Log("Removing index: " + info.Index);
                AttackData.hitboxData.RemoveAt(info.Index);
                StateEditor.EditState();

            }
        }

        public void UpdateObjectsToInstantiate(CollectionChangeInfo info, object value)
        {
            if (info.ChangeType == CollectionChangeType.RemoveIndex)
            {
                Debug.Log("Removing index: " + info.Index);
                AttackData.objectsToInstantiate.RemoveAt(info.Index);
                StateEditor.EditState();
            }
        }

        #region"Buttons - Click To Expand
        public void AddHitbox()
        {
            AttackData.hitboxData.Add(new HitboxData(AttackData.numberOfFrames, HitboxAnchor.name));

            AttackData.hitboxData[AttackData.hitboxData.Count - 1].frameActiveData = new List<bool>(AttackData.hitboxData.First().frameActiveData);
            AttackData.hitboxData[AttackData.hitboxData.Count - 1].sizeData = new List<fp>(AttackData.hitboxData.First().sizeData);
            AttackData.hitboxData[AttackData.hitboxData.Count - 1].offsetData = new List<fp3>(AttackData.hitboxData.First().offsetData);

            DisplayedHitboxData.Add(new DisplayedHitboxData(HitboxAnchor.gameObject));
            DisplayHitboxData();
        }

        public void AddObjectHitbox()
        {
            HitboxData newHitbox = new HitboxData(AttackData.numberOfFrames, ObjectHitboxAnchor.name);
            List<string> hitboxNames = new List<string>();
            foreach (HitboxData hitboxData in AttackData.hitboxData)
            {
                hitboxNames.Add(hitboxData.anchorObjectName);
            }
            if (!hitboxNames.Contains(ObjectHitboxAnchor.name))
            {
                AttackData.hitboxData.Add(newHitbox);

                AttackData.hitboxData[AttackData.hitboxData.Count - 1].frameActiveData = new List<bool>(AttackData.hitboxData.First().frameActiveData);
                AttackData.hitboxData[AttackData.hitboxData.Count - 1].sizeData = new List<fp>(AttackData.hitboxData.First().sizeData);
                AttackData.hitboxData[AttackData.hitboxData.Count - 1].offsetData = new List<fp3>(AttackData.hitboxData.First().offsetData);

                DisplayedHitboxData.Add(new DisplayedHitboxData(ObjectHitboxAnchor.gameObject));
                DisplayHitboxData();
            }
            else
            {
                Debug.LogError("This hitbox name already exists in the hitboxData");
            }
        }

        public void AddNewObject()
        {
            if (ObjectToAdd)
            {
                SelectedFighterStateMachine.Fighter.fighterData.objectsToInstantiate.Add(ObjectToAdd);
                ObjectInstantiationData objectData = new ObjectInstantiationData(SelectedFighterStateMachine.Fighter.fighterData.objectsToInstantiate.Count - 1);
                AttackData.objectsToInstantiate.Add(objectData);
                ObjectsToInstantiate.Add(objectData);
            }
            else
            {
                Debug.LogError("Object to Add was not set");
            }
        }
        public void AddExistingObject()
        {
            ObjectInstantiationData objectData = new ObjectInstantiationData(SelectedFighterStateMachine.Fighter.fighterData.objectsToInstantiate.IndexOf(ExistingObjectToAdd));
            if (objectData.objectIndex != -1)
            {
                AttackData.objectsToInstantiate.Add(objectData);
                ObjectsToInstantiate.Add(objectData);
            }
            else
            {
                Debug.LogError("Existing Object was not set or the gameobject was not found in FighterData - Objets to Instantiate");
            }
        }

        [ButtonGroup("Playback"), OnInspectorGUI("PreviousFrame")]
        public void PreviousFrame()
        {
            RollbackManager rollbackManager = Object.FindObjectOfType<RollbackManager>();
            if (SirenixEditorGUI.IconButton(EditorIcons.Previous, GUI.skin.button, 25, 25, "Previous Frame"))
            {
                if (currentFrame > 1)
                {
                    rollbackManager.Rewind(1);
                    currentFrame--;
                    DisplayHitboxData();
                }
            }
        }

        [ButtonGroup("Playback"), OnInspectorGUI("Play")]
        public void Play()
        {
            RollbackManager rollbackManager = Object.FindObjectOfType<RollbackManager>();

            bool simulating = Time.timeScale == 1;
            if (simulating)
            {
                if (SirenixEditorGUI.IconButton(EditorIcons.Pause, GUI.skin.button, 25, 25, "Pause"))
                {
                    rollbackManager.PauseRollbackSimulation();
                }
            }
            else
            {
                if (SirenixEditorGUI.IconButton(EditorIcons.Play, GUI.skin.button, 25, 25, "Play"))
                {
                    rollbackManager.UnpauseRollbackSimulation();
                }
            }
        }

        [ButtonGroup("Playback"), OnInspectorGUI("NextFrame")]
        public void NextFrame()
        {
            RollbackManager rollbackManager = Object.FindObjectOfType<RollbackManager>();

            if (SirenixEditorGUI.IconButton(EditorIcons.Next, GUI.skin.button, 25, 25, "Next Frame"))
            {
                if (currentFrame < AttackData?.numberOfFrames)
                {
                    rollbackManager.FastForward(1);
                    currentFrame++;
                    DisplayHitboxData();
                }
            }
        }
        #endregion

        public void DisplayHitboxData()
        {
            currentFrame = Mathf.Max(1, currentFrame);
            if (AttackData)
            {
                if (currentFrame <= AttackData.numberOfFrames)
                {
                    for (int i = 0; i < AttackData.hitboxData.Count; i++)
                    {
                        DisplayedHitboxData[i].FrameActiveData = AttackData.hitboxData[i].frameActiveData[currentFrame - 1];
                        DisplayedHitboxData[i].OffsetData = AttackData.hitboxData[i].offsetData[currentFrame - 1].ToVector3();
                        DisplayedHitboxData[i].SizeData = AttackData.hitboxData[i].sizeData[currentFrame - 1].AsFloatRounded;
                    }
                }
            }
        }

        public void UpdateHitboxData()
        {
            if (AttackData.hitboxData.Count == DisplayedHitboxData.Count)
            {
                for (int i = 0; i < AttackData.hitboxData.Count; i++)
                {
                    AttackData.hitboxData[i].frameActiveData[currentFrame - 1] = DisplayedHitboxData[i].FrameActiveData;
                    AttackData.hitboxData[i].offsetData[currentFrame - 1] = DisplayedHitboxData[i].OffsetData.ToFp3();
                    AttackData.hitboxData[i].sizeData[currentFrame - 1] = DisplayedHitboxData[i].SizeData.ToFixedPoint();
                }
            }
            else
            {
                //Figure out which hitboxData does not exist and delete it
            }
        }

        public void UpdateObjectData()
        {
            if (AttackData.objectsToInstantiate.Count == ObjectsToInstantiate.Count)
            {
                for (int i = 0; i < AttackData.objectsToInstantiate.Count; i++)
                {
                    AttackData.objectsToInstantiate[i].frameDelay = ObjectsToInstantiate[i].frameDelay;
                    AttackData.objectsToInstantiate[i].isTrap = ObjectsToInstantiate[i].isTrap;
                    AttackData.objectsToInstantiate[i].chargeTime = ObjectsToInstantiate[i].chargeTime;
                }
            }
            else
            {
                //Figure out which hitboxData does not exist and delete it
            }
        }

        public void SetAllHitboxesActive()
        {
            if (AttackData.hitboxData.Count == DisplayedHitboxData.Count)
            {
                for (int i = 0; i < AttackData.hitboxData.Count; i++)
                {
                    AttackData.hitboxData[i].frameActiveData[currentFrame - 1] = true;
                }
            }
            DisplayHitboxData();
        }

        public void SetAllHitboxesInactive()
        {
            if (AttackData.hitboxData.Count == DisplayedHitboxData.Count)
            {
                for (int i = 0; i < AttackData.hitboxData.Count; i++)
                {
                    AttackData.hitboxData[i].frameActiveData[currentFrame - 1] = false;
                }
            }
            DisplayHitboxData();
        }

        public void SetOffsetForAllFrames()
        {
            if (AttackData.hitboxData.Count == DisplayedHitboxData.Count)
            {
                for (int i = 0; i < AttackData.hitboxData.Count; i++)
                {
                    for (int o = 1; o < AttackData.numberOfFrames; o++)
                    {
                        AttackData.hitboxData[i].offsetData[o] = DisplayedHitboxData[i].OffsetData.ToFp3();
                    }
                }
            }
        }

        public void SetSizeForAllFrames()
        {
            if (AttackData.hitboxData.Count == DisplayedHitboxData.Count)
            {
                for (int i = 0; i < AttackData.hitboxData.Count; i++)
                {
                    for (int o = 1; o < AttackData.numberOfFrames; o++)
                    {
                        AttackData.hitboxData[i].sizeData[o] = DisplayedHitboxData[i].SizeData.ToFixedPoint();
                    }
                }
            }
        }

        public void SetHitboxesActiveOnFrameRange()
        {
            ChangeActiveHitboxesOnMultipleFrames window = new ChangeActiveHitboxesOnMultipleFrames();
            window.attackData = AttackData;
            window.maxSize = new Vector2(300, 150);
            window.ShowUtility();
            window.OnFinish += () => ChangeHitboxDataOnFrames(window.startFrame, window.endFrame, window.active);
        }

        public void ChangeHitboxDataOnFrames(int start, int end, bool active)
        {
            for (int o = 0; o < AttackData.hitboxData.Count; o++)
            {
                for (int i = start - 1; i < end; i++)
                {
                    AttackData.hitboxData[o].frameActiveData[i] = active;
                }
            }
        }
    }
}