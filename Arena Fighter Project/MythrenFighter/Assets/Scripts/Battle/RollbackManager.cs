using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using FixedPoint;
using System;
using NaughtyAttributes;
using Sirenix.OdinInspector;

namespace MythrenFighter
{
    public class RollbackManager : SerializedMonoBehaviour
    {
        public static readonly fp FRAMERATE = 60;
        public static readonly int FRAME_BUFFER = 5;
        // Example how to set up, might need this later. Need to implement sort of rollback entities in updateGameState
        //public static readonly List<Type> SCRIPT_EXECUTION_ORDER = new List<Type>
        //{
        //    typeof(CollisionManager),
        //    typeof(FighterMove),
        //    typeof(PhysicsBody)
        //};

        // Variables
        public bool simulating = false;
        private float startTime = 0;
        [ShowInInspector] public static int currentFrame = 0;
        [SerializeField]
        [ShowInInspector] private static Stack<Dictionary<Guid, dynamic>> gameState = new Stack<Dictionary<Guid, dynamic>>();

        // Dependencies
        private static List<RollbackEntity> rollbackEntities = new List<RollbackEntity>();
        private static List<RollbackEntity> queuedRollbackEntities = new List<RollbackEntity>();
        private static List<RollbackEntity> removedRollbackEntities = new List<RollbackEntity>();

        private void Awake()
        {
            rollbackEntities = FindObjectsOfType<MonoBehaviour>().OfType<RollbackEntity>().ToList();
            currentFrame = 0;
            if (FindObjectOfType<BattleManager>() == null)
            {
                StartRollbackSimulation();
            }
        }

        public void StartRollbackSimulation()
        {
            currentFrame = 0;
            simulating = true;
            startTime = Time.time;
        }

        public void PauseRollbackSimulation()
        {
            Time.timeScale = 0;
            simulating = false;
        }

        public void UnpauseRollbackSimulation()
        {
            Time.timeScale = 1;
            simulating = true;
        }

        public static void RegisterRollbackEntity(RollbackEntity rollbackEntity)
        {
            if (!rollbackEntities.Contains(rollbackEntity))
            {
                rollbackEntities.Add(rollbackEntity);
            }
        }

        public static void QueueRegisterRollbackEntity(RollbackEntity rollbackEntity)
        {
            if (!rollbackEntities.Contains(rollbackEntity))
            {
                queuedRollbackEntities.Add(rollbackEntity);
            }
        }

        public static void QueueUnregisterRollbackEntity(RollbackEntity rollbackEntity)
        {
            if (rollbackEntities.Contains(rollbackEntity))
            {
                removedRollbackEntities.Add(rollbackEntity);
            }
        }

        public static void UnregisterRollbackEntity(RollbackEntity rollbackEntity)
        {
            rollbackEntities.Remove(rollbackEntity);
        }
        public void ClearGameState()
        {
            gameState.Clear();

            foreach (RollbackEntity rollbackEntity in removedRollbackEntities)
            {
                rollbackEntities.Remove(rollbackEntity);
            }
            Dictionary<Guid, dynamic> currentGameState = new Dictionary<Guid, dynamic>();

            foreach (RollbackEntity rollbackEntity in rollbackEntities)
            {
                if (rollbackEntity.gameObject.activeInHierarchy && rollbackEntity.gameObject)
                {
                    currentGameState.Add(rollbackEntity.id, rollbackEntity.GetInitialState());
                }
            }
            gameState.Push(currentGameState);
        }
        public void Rollback(int frames)
        {
            frames = Mathf.Min(gameState.Count, frames);
            currentFrame -= frames;

            for (int i = 0; i < frames; i++)
            {
                gameState.Pop();
            }
            for (int i = 0; i < frames; i++)
            {
                UpdateGameState();
            }
            UpdateGameVisuals();
        }

        public void FastForward(int frames)
        {
            currentFrame += frames;

            for (int i = 0; i < frames; i++)
            {
                UpdateGameState();
            }

            UpdateGameVisuals();
        }

        public void Rewind(int frames)
        {
            frames = Mathf.Min(gameState.Count, frames);
            if (gameState.Count > 0)
            {
                currentFrame -= frames;
            }

            for (int i = 0; i < frames; i++)
            {
                gameState.Pop();
            }

            Dictionary<Guid, dynamic> priorGameState = new Dictionary<Guid, dynamic>();
            if (gameState.Count > 0)
            {
                priorGameState = gameState.Peek();
            }

            foreach (RollbackEntity rollbackEntity in rollbackEntities)
            {
                dynamic priorEntityState;
                if (priorGameState.ContainsKey(rollbackEntity.id))
                {
                    priorEntityState = priorGameState[rollbackEntity.id];
                }
                else
                {
                    priorEntityState = rollbackEntity.GetInitialState();
                }
                if (rollbackEntity.gameObject.activeInHierarchy)
                {
                    rollbackEntity.SetState(priorEntityState);
                }
            }
            UpdateGameVisuals();
        }

        public void UpdateGameState()
        {
            rollbackEntities.AddRange(queuedRollbackEntities);
            queuedRollbackEntities.Clear();

            foreach (RollbackEntity removedEntity in removedRollbackEntities)
            {
                rollbackEntities.Remove(removedEntity);
            }
            removedRollbackEntities.Clear();

            Dictionary<Guid, dynamic> priorGameState = new Dictionary<Guid, dynamic>();
            if (gameState.Count > 0)
            {
                priorGameState = gameState.Peek();
            }
            Dictionary<Guid, dynamic> currentGameState = new Dictionary<Guid, dynamic>();
            foreach (RollbackEntity rollbackEntity in rollbackEntities)
            {
                dynamic priorEntityState;
                if (priorGameState.ContainsKey(rollbackEntity.id))
                {
                    priorEntityState = priorGameState[rollbackEntity.id];
                }
                else
                {
                    priorEntityState = rollbackEntity.GetInitialState();
                }
                if (rollbackEntity.gameObject.activeInHierarchy)
                {
                    rollbackEntity.SetState(priorEntityState);
                }
                else
                {
                    currentGameState.Add(rollbackEntity.id, priorEntityState);
                }
            }
            foreach (RollbackEntity rollbackEntity in rollbackEntities)
            {
                if (rollbackEntity.gameObject.activeInHierarchy)
                {
                    rollbackEntity.SimulateFrame();
                }
            }

            foreach (RollbackEntity rollbackEntity in rollbackEntities)
            {
                if (rollbackEntity.gameObject.activeInHierarchy)
                {
                    if (currentGameState.ContainsKey(rollbackEntity.id))
                    {
                        currentGameState[rollbackEntity.id] = rollbackEntity.GetUpdatedState();
                    }
                    else
                    {
                        currentGameState.Add(rollbackEntity.id, rollbackEntity.GetUpdatedState());
                    }
                }
            }
            gameState.Push(currentGameState);
            currentFrame++;
        }

        public static void UpdateRollbackEntityOnCurrentFrame(Guid id, dynamic state)
        {
            Dictionary<Guid, dynamic> priorGameState = new Dictionary<Guid, dynamic>();
            if (gameState.Count > 0)
            {
                priorGameState = gameState.Peek();
            }
            if (priorGameState.ContainsKey(id))
            {
                priorGameState[id] = state;
            }
            if (gameState.Count > 0)
            {
                gameState.Pop();
            }
            gameState.Push(priorGameState);
        }

        private void UpdateGameVisuals()
        {
            foreach (RollbackEntity rollbackEntity in rollbackEntities)
            {
                if (rollbackEntity.gameObject.activeInHierarchy)
                {
                    rollbackEntity.UpdateVisuals();
                }
            }
        }

        private void Update()
        {
            if (simulating)
            {
                int frame = (int)((Time.time - startTime) * (int)FRAMERATE);
                for (int i = 0; i < frame - currentFrame; i++)
                {
                    UpdateGameState();
                }
                UpdateGameVisuals();
            }
        }
    }

}