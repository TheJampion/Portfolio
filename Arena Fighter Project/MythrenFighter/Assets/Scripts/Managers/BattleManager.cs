using Epic.OnlineServices.Lobby;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace MythrenFighter {
    public class BattleManager : MonoBehaviour
    {

        // Variables
        public bool battleStarted = false;

        // Dependencies
        public RollbackManager rollbackManager;
        public CinemachineTargetGroup cameraTargetGroup;
        private InputManager inputManager;
        public List<Fighter> fighters = new List<Fighter>();

        private void Awake()
        {
            inputManager = FindObjectOfType<InputManager>();
        }

        private void Start()
        {
            LobbyManager.Instance.startBattleEvent += StartBattle;
            Application.targetFrameRate = RollbackManager.FRAMERATE.AsInt;
        }


        public void InitializeBattle()
        {
            Time.timeScale = 0;
            LobbyManager.Instance.startBattleEvent += StartBattle;
            rollbackManager = FindObjectOfType<RollbackManager>();
            LoadBattle();
        }

        private void OnDestroy()
        {
            LobbyManager.Instance.startBattleEvent += StartBattle;
        }

        private void LoadBattle()
        {
            BattleLoadData loadData = GameDataManager.Instance.battleLoadData;
            for (int i = 0; i < loadData.playerSlotDatas.Count; i++)
            {
                SpawnFighter(loadData.playerSlotDatas[i].fighterId, loadData.playerSlotDatas[i].playerSlot, loadData.battleEnvironment.spawnPositions[i]);
            }

            if (GameDataManager.Instance.battleLoadData.isOnline)
            {
                LobbyManager.Instance.UpdateLobby(new List<AttributeData>(), new List<AttributeData> { new AttributeData { Key = "isLoaded", Value = true } });
            }
            else
            {
                StartBattle();
            }
        }

        private void SpawnFighter(string fighterId, int playerSlot, Vector3 spawnPosition)
        {
            FighterData fighterData = GameDataManager.Instance.fighterDatabase.fighters.Find(x => x.fighterId == fighterId);
            if (fighterData != null)
            {
                GameObject fighterObject = Instantiate(fighterData.battlePrefab, spawnPosition, Quaternion.identity);
                Fighter fighter = fighterObject.GetComponent<Fighter>();
                fighters.Add(fighter);
                fighter.playerSlot = playerSlot;
                foreach (RollbackEntity rollbackEntity in fighterObject.GetComponents<RollbackEntity>())
                {
                    RollbackManager.RegisterRollbackEntity(rollbackEntity);
                }
                HitManager.registerHitHandler(fighterObject.GetComponent<HitHandler>());
            }
        }

        private void SpawnFighter(string fighterId, int playerSlot)
        {
            FighterData fighterData = GameDataManager.Instance.fighterDatabase.fighters.Find(x => x.fighterId == fighterId);
            if (fighterData != null)
            {
                GameObject fighterObject = Instantiate(fighterData.battlePrefab);
                Fighter fighter = fighterObject.GetComponent<Fighter>();
                fighters.Add(fighter);
                fighter.playerSlot = playerSlot;
                foreach (RollbackEntity rollbackEntity in fighterObject.GetComponents<RollbackEntity>())
                {
                    RollbackManager.RegisterRollbackEntity(rollbackEntity);
                }
                HitManager.registerHitHandler(fighterObject.GetComponent<HitHandler>());
            }
        }

        [Button]
        private void StartBattle()
        {
            Time.timeScale = 1;
            battleStarted = true;
            rollbackManager.StartRollbackSimulation();
            inputManager.inputsEnabled = true;
        }

        public void EndBattle()
        {
            foreach (Fighter fighter in fighters)
            {
                foreach (RollbackEntity rollbackEntity in fighter.gameObject.GetComponents<RollbackEntity>())
                {
                    RollbackManager.UnregisterRollbackEntity(rollbackEntity);
                }
                Destroy(fighter.gameObject);
            }
            fighters.Clear();
        }

    }
}
