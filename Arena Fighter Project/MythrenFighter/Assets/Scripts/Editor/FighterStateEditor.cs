using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using System.Linq;


namespace MythrenFighter.FighterEditor
{

    public class FighterEnemyEditor
    {
        public FighterStateMachine enemyFighterStateMachine { get; set; }
        public Fighter fighter { get; set; }
        public List<FighterState> FighterStates { get; set; }
        [Title("Select a State", "Choose enemy fighter's state")]
        [ValueDropdown("GetListOfStates", HideChildProperties = false)] public FighterState SelectedFighterState;
        [Button]
        public void SetState()
        {
            enemyFighterStateMachine.enterState(SelectedFighterState);
        }

        public IEnumerable<FighterState> GetListOfStates()
        {
            return fighter.fighterData.fighterStates;
        }
    }

    public class FighterStateEditor
    {
        //Hidden(In Insepctor) Attributes
        public List<FighterState> FighterStates { get; set; }
        public int CurrentFrame { get; set; }
        public FighterStateMachine SelectedFighterStateMachine { get; set; }
        public RollbackManager RollbackManager { get; set; }
        public GameObject CurrentFighter { get; set; }
        public FighterEditor FighterEditor { get; set; }

        private bool _hasAttack;

        //Visible (In Inspector) Attributes
        [Title("Select a Fighter", "Select a Fighter to Edit")]
        [ValueDropdown("GetListOfFighters")][OnValueChanged("SpawnSelectedFighter")] public FighterData SelectedFighter;

        [Title("Select a State", "Select a State to Edit")]
        [OnValueChanged("EditState")]
        [ValueDropdown("GetListOfStates", HideChildProperties = false), HideIf("@!CurrentFighter")] public FighterState SelectedFighterState; // Used purely to select the fighter's state

        [Title("State", "Edit Selected State")]
        [InlineEditor(InlineEditorModes.GUIOnly, InlineEditorObjectFieldModes.CompletelyHidden), HideIf("@!CurrentFighter")] public FighterState CurrentEditableFighterState; //Actual displayed State Data

        [Title("Hitbox Editor", "Edit Attack's Hitboxes")]
        [HideIf("@!_hasAttack"), HideLabel] public FighterAttackEditor AttackEditor;
        //Add warning to select a fighter if there is no fighter selected

        public void EditState()
        {
            SelectedFighterStateMachine = CurrentFighter.GetComponent<FighterStateMachine>();
            CurrentEditableFighterState = SelectedFighterState;
            _hasAttack = CurrentEditableFighterState.attackData;
            RollbackManager.ClearGameState();
            if (CurrentEditableFighterState.attackData != null)
            {
                AttackEditor.StateEditor = this;
                AttackEditor.AttackData = SelectedFighterState.attackData;
                AttackEditor.IsSettingUpAttack = false;
                AttackEditor.RootSkeleton = CurrentFighter.GetComponentsInChildren<Transform>()
                                 .FirstOrDefault(c => c.gameObject.name == SelectedFighter.rootSkeleton.name)?.gameObject;
                AttackEditor.AttackData.numberOfFrames = Mathf.FloorToInt(SelectedFighterState.animationClips[0].length * 60f);
                AttackEditor.SelectedFighterStateMachine = SelectedFighterStateMachine;
            }
            else
            {
                AttackEditor.AttackData = null;
            }
            if (SelectedFighterStateMachine)
            {
                SelectedFighterStateMachine.IsInFighterEditor = true;
                SelectedFighterStateMachine.enterState(SelectedFighterState);
                AttackEditor.SetupAttack();
            }
        }

        public IEnumerable<FighterState> GetListOfStates()
        {
            return FighterStates;
        }

        public IEnumerable GetListOfFighters()
        {
            string[] rootLabels = new string[] { "Assets/ScriptableObjects/FighterData" }; ;

            return UnityEditor.AssetDatabase.FindAssets("t:ScriptableObject", rootLabels).Select(x => AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(x), typeof(ScriptableObject)));
        }

        public void SpawnSelectedFighter()
        {
            if (Application.isPlaying)
                FighterEditor.SelectFighter(SelectedFighter);
        }
        //Currently trying to make it so you can select a fighter in the state editor so there isnt a need for two separate windows
    }
}