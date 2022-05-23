using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System.Linq;
using FixedPoint;
using System.IO;


namespace MythrenFighter.FighterEditor
{
    public class FighterEditor : OdinMenuEditorWindow
    {

        public static FighterEditorData s_FighterEditorData;
        public static int s_CurrentFrame;

        private GameObject _currentFighter;
        private FighterData _selectedFighterData;
        private FighterStateMachine _selectedFighterStateMachine;
        private FighterStateEditor _fighterStateEditor = new FighterStateEditor();
        private FighterAttackEditor _fighterAttackEditor = new FighterAttackEditor();
        private FighterEnemyEditor _fighterEnemyEditor = new FighterEnemyEditor();
        private RollbackManager _rollbackManager;
        private bool _isPlaytesting;
        
        public List<RollbackEntity> FighterRollbackEntities = new List<RollbackEntity>();
        public EditorIcon Icon;

        [MenuItem("Tools/Fighter Editor")]
        private static void OpenWindow()
        {
            GetWindow<FighterEditor>().Show();
        }
        private void Awake()
        {
            _rollbackManager = FindObjectOfType<RollbackManager>();
            _fighterStateEditor.RollbackManager = _rollbackManager;
            _fighterAttackEditor.RollbackManager = _rollbackManager;
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _fighterAttackEditor = new FighterAttackEditor();
            _fighterStateEditor = new FighterStateEditor();
            _fighterEnemyEditor = new FighterEnemyEditor();
            if (_currentFighter != null)
            {
                DestroyImmediate(_currentFighter);
            }
        }
        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                _rollbackManager = FindObjectOfType<RollbackManager>();
                _fighterStateEditor.RollbackManager = _rollbackManager;
                _fighterAttackEditor.RollbackManager = _rollbackManager;
                _rollbackManager.StartRollbackSimulation();

                s_FighterEditorData = FindObjectOfType<FighterEditorData>();
                string[] hitbox = AssetDatabase.FindAssets("Hitbox", new[] { "Assets/Prefabs/Battle" });
                string hitboxPath = AssetDatabase.GUIDToAssetPath(hitbox[0]);
                s_FighterEditorData.hitboxPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(hitboxPath, typeof(GameObject));

                FindObjectOfType<InputManager>().inputsEnabled = false;
                _isPlaytesting = false;
                if (_fighterStateEditor.SelectedFighter)
                {
                    SelectFighter(_fighterStateEditor.SelectedFighter);
                }
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                s_FighterEditorData.currentFighterData = null;
                _fighterStateEditor = new FighterStateEditor();
            }
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree();
            tree.Add("State Editor", _fighterStateEditor);
            tree.AddAllAssetsAtPath("Fighters", "ScriptableObjects/FighterData", typeof(ScriptableObject), true);
            tree.Add("Update Spawned Enemy", _fighterEnemyEditor);
            var selected = tree.Selection.FirstOrDefault();
            return tree;
        }
        protected override void OnBeginDrawEditors()
        {
            if (EditorApplication.isPlaying)
            {
                var selected = this.MenuTree.Selection.FirstOrDefault();
                _selectedFighterData = selected.Value as FighterData;
                _rollbackManager = FindObjectOfType<RollbackManager>();
                _fighterStateEditor.RollbackManager = _rollbackManager;
                _fighterAttackEditor.RollbackManager = _rollbackManager;
                _fighterStateEditor.FighterEditor = this;

                if (_selectedFighterData != null)
                {
                    SelectFighter(_selectedFighterData);
                    _selectedFighterStateMachine = _currentFighter.GetComponent<FighterStateMachine>();
                }
                else if (selected.Value as FighterStateEditor != null)
                {
                    SirenixEditorGUI.BeginHorizontalToolbar();
                    CheckAttackEditorButtons();
                }
            }
            base.OnBeginDrawEditors();
        }

        private void Update()
        {
            if (!_isPlaytesting)
            {
                if (_selectedFighterStateMachine)
                {
                    if (_fighterStateEditor.SelectedFighterState)
                    {
                        if (_fighterStateEditor.SelectedFighterState.attackData && Time.timeScale > 0)
                        {
                            if (_fighterAttackEditor.IsSettingUpAttack)
                            {
                                _fighterAttackEditor.DisplayHitboxData();
                            }
                        }
                    }
                    if (_fighterStateEditor.SelectedFighterState)
                    {
                        CheckForEndOfState();
                    }
                }
                if (_fighterStateEditor.SelectedFighterState && _selectedFighterStateMachine)
                {
                    if (Time.timeScale > 0)
                    {
                        s_CurrentFrame = (int)(_selectedFighterStateMachine.StateMachineData.TimeInState * 60f);
                        _fighterStateEditor.CurrentFrame = s_CurrentFrame;
                        _fighterAttackEditor.currentFrame = s_CurrentFrame;
                    }
                }
            }

        }

        private void CheckForEndOfState()
        {
            if (!_selectedFighterStateMachine.IsUsingBlendTree)
            {
                if (_fighterStateEditor.SelectedFighterStateMachine.StateMachineData.TimeInState >= _fighterStateEditor.SelectedFighterStateMachine.StateMachineData.MaxTimeInState)
                {
                    int animationNumberOfFrames = (int)(_fighterStateEditor.SelectedFighterStateMachine.StateMachineData.CurrentState.animationClips[0].length * 60f);
                    if (_fighterStateEditor.SelectedFighterStateMachine.StateMachineData.TimeInState * 60f > animationNumberOfFrames)
                    {
                        _rollbackManager.ClearGameState();
                        _fighterStateEditor.SelectedFighterStateMachine.FighterMove.physicsBody.currentState.position = fp3.zero;
                        RollbackManager.UpdateRollbackEntityOnCurrentFrame(_fighterStateEditor.SelectedFighterStateMachine.FighterMove.physicsBody.id, _fighterStateEditor.SelectedFighterStateMachine.FighterMove.physicsBody.GetUpdatedState());
                        _fighterStateEditor.SelectedFighterStateMachine.enterState(_fighterStateEditor.SelectedFighterStateMachine.StateMachineData.CurrentState);
                        if (_fighterAttackEditor.AttackData)
                        {
                            _fighterAttackEditor.SetupAttack();
                        }
                    }
                }
            }
            else
            {
                if (_selectedFighterStateMachine.StateMachineData.TimeInState >= _selectedFighterStateMachine.AnimationController.CurrentMixerPlayable.GetDuration())
                {
                    _rollbackManager.ClearGameState();
                    _fighterStateEditor.SelectedFighterStateMachine.FighterMove.physicsBody.currentState.position = fp3.zero;
                    _fighterStateEditor.SelectedFighterStateMachine.enterState(_fighterStateEditor.SelectedFighterStateMachine.StateMachineData.CurrentState);
                }
            }
        }

        public void SelectFighter(FighterData fighterData)
        {
            //Destroy the Fighter and Remove it from the rollback simulation
            DestroyCurrentFighter();
            //Create new Fighter's gameobject and add it to the simulation
            _currentFighter = Instantiate(fighterData.battlePrefab);
            _selectedFighterStateMachine = _currentFighter.GetComponent<FighterStateMachine>();
            s_FighterEditorData.currentFighterData = fighterData;
            //Setup State and Attack Editors
            _fighterStateEditor.CurrentFighter = _currentFighter;
            _fighterAttackEditor.CurrentFighter = _currentFighter;
            _fighterStateEditor.SelectedFighterState = null;
            _fighterStateEditor.CurrentEditableFighterState = _fighterStateEditor.SelectedFighterState;
            _fighterStateEditor.FighterStates = fighterData.fighterStates;
            _fighterStateEditor.AttackEditor = _fighterAttackEditor;
            for (int i = 0; i < fighterData.fighterStates.Count; i++)
            {
                if (fighterData.fighterStates[i] == null)
                {
                    fighterData.fighterStates.Remove(fighterData.fighterStates[i]);
                }
            }
            FighterRollbackEntities = _currentFighter.GetComponents<RollbackEntity>().ToList();
            foreach (RollbackEntity rollbackEntity in FighterRollbackEntities)
            {
                RollbackManager.RegisterRollbackEntity(rollbackEntity);
            }
            AssetDatabase.SaveAssets();
        }

        public void DestroyCurrentFighter()
        {
            if (_currentFighter)
                DestroyImmediate(_currentFighter);

            foreach (RollbackEntity rollbackEntity in FighterRollbackEntities)
            {
                RollbackManager.QueueUnregisterRollbackEntity(rollbackEntity);
            }
        }

        #region Fighter Editor Buttons - Click to Expand
        private void CheckAttackEditorButtons()
        {
            {
                GUILayout.FlexibleSpace();
                if (!_isPlaytesting)
                {
                    if (SirenixEditorGUI.ToolbarButton("Start Playtesting"))
                    {
                        //Clear Game State, setup Battle with current Fighter, start Battle, enable Inputs
                        DestroyCurrentFighter();
                        _rollbackManager.ClearGameState();
                        BattleManager battleManager = FindObjectOfType<BattleManager>();
                        GameDataManager.Instance.battleLoadData.playerSlotDatas[0].fighterId = _fighterStateEditor.SelectedFighter.fighterId;
                        _isPlaytesting = true;
                        s_FighterEditorData.editorCamera.Priority = 0;
                        battleManager.InitializeBattle();
                        _fighterEnemyEditor.enemyFighterStateMachine = battleManager.fighters[1].GetComponent<FighterStateMachine>();
                        _fighterEnemyEditor.fighter = battleManager.fighters[1];
                    }
                    if (SirenixEditorGUI.ToolbarButton("Add State"))
                    {
                        if (_fighterStateEditor.SelectedFighter)
                        {
                            StateSetup stateSetup = new StateSetup();
                            stateSetup.maxSize = new Vector2(300, 150);
                            stateSetup.ShowUtility();
                            stateSetup.OnFinish += () => CreateNewState(stateSetup.stateName, stateSetup.animationToAdd);
                        }
                        else
                        {
                            Debug.LogError("No Fighter Selected to add a state to");
                        }
                    }
                    if (_fighterStateEditor.SelectedFighterState)
                    {
                        if (SirenixEditorGUI.ToolbarButton("Delete State"))
                        {
                            _fighterStateEditor.SelectedFighter.fighterStates.Remove(_fighterStateEditor.SelectedFighterState);
                            DeleteState(_fighterStateEditor.SelectedFighterState);
                            _fighterStateEditor.SelectedFighterState = _fighterStateEditor.SelectedFighter.fighterStates.Last();
                            _fighterStateEditor.EditState();
                        }

                        if (!_fighterStateEditor.SelectedFighterState.attackData)
                        {
                            if (SirenixEditorGUI.ToolbarButton("Add Attack"))
                            {
                                if (_fighterStateEditor.SelectedFighterState)
                                {
                                    AttackSetup attackSetup = new AttackSetup();
                                    attackSetup.maxSize = new Vector2(300, 300);
                                    attackSetup.ShowUtility();
                                    attackSetup.OnFinish += () => CreateNewAttack(attackSetup.attackName);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (SirenixEditorGUI.ToolbarButton("Stop Playtesting"))
                    {
                        //Clear Game State, setup Battle with current Fighter, start Battle, enable Inputs
                        BattleManager battleManager = FindObjectOfType<BattleManager>();
                        FindObjectOfType<InputManager>().inputsEnabled = false;
                        battleManager.EndBattle();
                        s_FighterEditorData.editorCamera.Priority = 999;
                        _rollbackManager.ClearGameState();
                        SelectFighter(_fighterStateEditor.SelectedFighter);
                        _isPlaytesting = false;
                    }
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }

        private void CreateNewState(string newStateFileName, AnimationClip animationClip)
        {
            FighterState newFighterState = FighterState.CreateInstance(newStateFileName, animationClip);
            newFighterState.stateBehaviours = new List<FighterStateMachineBehaviour>();
            newFighterState.stateBehaviours.Add(s_FighterEditorData.defaultStateBehaviour);
            string assetPath = _fighterStateEditor.SelectedFighter.statesPath;
            AssetDatabase.CreateAsset(newFighterState, assetPath + "/" + _fighterStateEditor.SelectedFighter.fighterName + "_" + newStateFileName + ".asset");
            _fighterStateEditor.SelectedFighter.fighterStates.Add(newFighterState);
            EditorUtility.SetDirty(_fighterStateEditor.SelectedFighter);
            AssetDatabase.SaveAssets();
            // This was code that automatically gets pathing of the first state in fighterStates; string assetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(fighterStateEditor.selectedFighter.fighterStates[0]));
        }

        private void CreateNewAttack(string newAttackFileName)
        {
            if (_fighterStateEditor.SelectedFighterState.animationClips.Count > 0)
            {
                AttackData newAttackData = AttackData.CreateInstance(newAttackFileName);
                newAttackData.attackName = newAttackFileName;
                newAttackData.numberOfFrames = Mathf.FloorToInt(_fighterStateEditor.SelectedFighterState.animationClips[0].length * 60f);
                string assetPath = _fighterStateEditor.SelectedFighter.attacksPath;
                AssetDatabase.CreateAsset(newAttackData, assetPath + "/" + _fighterStateEditor.SelectedFighter.fighterName + "_" + newAttackFileName + "_" + "AttackData" + ".asset");
                _fighterStateEditor.SelectedFighterState.attackData = newAttackData;
                _fighterStateEditor.EditState();
                AssetDatabase.SaveAssets();
            }
            else
            {
                Debug.LogError("You tried to add an attack when no animation exists on this state");
            }
            // This was code that automatically gets pathing of the first state in fighterStates; string assetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(fighterStateEditor.selectedFighter.fighterStates[0]));
        }

        private void DeleteState(FighterState state)
        {
            if (state.attackData)
            {
                string attackAssetPath = AssetDatabase.GetAssetPath(state.attackData);
                AssetDatabase.DeleteAsset(attackAssetPath);
            }
            string assetPath = AssetDatabase.GetAssetPath(state);
            AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.SaveAssets();
        }
        #endregion
    }
}