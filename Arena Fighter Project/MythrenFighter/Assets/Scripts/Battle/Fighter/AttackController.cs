using FixedPoint;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MythrenFighter
{

    public struct AttackControllerData
    {
        public int currentAttackIndex;
        public bool attackActive;
        public bool attackAnimationActive;
        public bool attackPaused;
        public bool alwaysLockOn;
    }
    public class AttackController : MonoBehaviour, RollbackEntity
    {
        // Variables
        [SerializeField]
        private List<GameObject> activeHitboxes = new List<GameObject>();
        public List<GameObject> instantiatedObjects = new List<GameObject>();
        private List<int> hitNotAllowedIndexes = new List<int>();
        private List<Animator> attackAnimators = new List<Animator>();
        private List<ParticleSystem> attackParticleSystems = new List<ParticleSystem>();
        private List<IEnumerator> attackCoroutines = new List<IEnumerator>();
        private AttackData attackData;
        public AttackControllerData attackControllerData;

        // Dependencies - found with GetComponent or similar
        private HitHandler hitHandler;
        public FighterData fighterData { get; private set; }
        private Fighter _fighter;
        private FighterStateMachine _fighterStateMachine;
        private GameObject _hitboxPrefab;

        public Guid id { get; } = Guid.NewGuid();

        // Initialization
        private void Awake()
        {
            hitHandler = GetComponent<HitHandler>();
            _fighter = GetComponent<Fighter>();
            _fighterStateMachine = GetComponent<FighterStateMachine>();
        }

        private void OnEnable()
        {
            _fighterStateMachine.EnterStateAction += ActivateAttack;
            _fighterStateMachine.ExitStateAction += DeactivateAttack;
        }

        private void OnDisable()
        {
            _fighterStateMachine.EnterStateAction -= ActivateAttack;
            _fighterStateMachine.ExitStateAction -= DeactivateAttack;
        }

        private void Start()
        {
            fighterData = _fighter.fighterData;
            _hitboxPrefab = _fighter.hitboxPrefab;
        }

        private void OnDestroy()
        {
            if (activeHitboxes.Count > 0)
            {
                foreach (GameObject hitbox in activeHitboxes)
                {
                    Destroy(hitbox);
                }
                activeHitboxes.Clear();
            }

            foreach (IEnumerator enumerator in attackCoroutines)
            {
                StopCoroutine(enumerator);
            }
            attackCoroutines.Clear();

            foreach (GameObject instantiatedObject in instantiatedObjects)
            {
                Destroy(instantiatedObject);
            }
            clearNullPausables();
            ClearInactivePausables();
        }

        public void SetAttackData(AttackData attack)
        {
            attackData = attack;
        }

        public void pauseAttack()
        {
            attackControllerData.attackPaused = true;
            foreach (Animator animator in attackAnimators)
            {
                if (animator != null)
                {
                    animator.speed = 0;
                }
            }
            foreach (ParticleSystem particleSystem in attackParticleSystems)
            {
                if (particleSystem != null)
                {
                    particleSystem.Pause();
                }
            }
        }

        public void unpauseAttack()
        {
            attackControllerData.attackPaused = false;
            foreach (Animator animator in attackAnimators)
            {
                if (animator != null)
                {
                    animator.speed = 1;
                }
            }
            foreach (ParticleSystem particleSystem in attackParticleSystems)
            {
                if (particleSystem != null)
                {
                    particleSystem.Play();
                }
            }
        }

        public bool hitAllowedOn(int mythrenHitIndex)
        {
            return !hitNotAllowedIndexes.Contains(mythrenHitIndex);
        }

        public void addHitDisallowedIndex(int mythrenHitIndex)
        {
            hitNotAllowedIndexes.Add(mythrenHitIndex);
        }

        public void getPausables(GameObject g)
        {
            attackAnimators.AddRange(g.GetComponentsInChildren<Animator>());
            attackParticleSystems.AddRange(g.GetComponentsInChildren<ParticleSystem>());
        }

        private void clearNullPausables()
        {
            for (int i = attackAnimators.Count - 1; i >= 0; i--)
            {
                if (attackAnimators[i] == null)
                {
                    attackAnimators.RemoveAt(i);
                }
            }
            for (int i = attackParticleSystems.Count - 1; i >= 0; i--)
            {
                if (attackParticleSystems[i] == null)
                {
                    attackParticleSystems.RemoveAt(i);
                }
            }
        }

        private void ClearInactivePausables()
        {
            for (int i = attackAnimators.Count - 1; i >= 0; i--)
            {
                if (!attackAnimators[i].gameObject.activeInHierarchy)
                {
                    attackAnimators.RemoveAt(i);
                }
            }
            for (int i = attackParticleSystems.Count - 1; i >= 0; i--)
            {
                if (!attackParticleSystems[i].gameObject.activeInHierarchy)
                {
                    attackParticleSystems.RemoveAt(i);
                }
            }
        }

        public void ActivateAttack(FighterState state)
        {
            if(!state.attackData)
            {
                return;
            }
            attackData = state.attackData;
            hitNotAllowedIndexes.Clear();
            attackControllerData.attackActive = true;
            Transform nearbyEnemyTransform = null;
            if(attackData.objectsToInstantiate.Count > 0)
            {
                foreach (ObjectInstantiationData objectData in attackData.objectsToInstantiate)
                {
                    GameObject newObject = Instantiate(fighterData.objectsToInstantiate[objectData.objectIndex], gameObject.transform);
                    newObject.name = fighterData.objectsToInstantiate[objectData.objectIndex].name;
                    instantiatedObjects.Add(newObject);
                    newObject.SetActive(false);
                    List<RollbackEntity> rollbackEntities = newObject.GetComponents<RollbackEntity>().ToList();
                    foreach (RollbackEntity rollbackEntity in rollbackEntities)
                    {
                        RollbackManager.QueueRegisterRollbackEntity(rollbackEntity);
                    }
                }
            }
            if(_fighterStateMachine.StateMachineData.CurrentState.attackData.hitboxData.Count > 0)
            {
                for (int i = 0; i < _fighterStateMachine.StateMachineData.CurrentState.attackData.hitboxData.Count; i++)
                {
                    if (_fighterStateMachine.StateMachineData.CurrentState.attackData.hitboxData[i].anchorObjectName == null)
                    {
                        _fighterStateMachine.StateMachineData.CurrentState.attackData.hitboxData[i].anchorObjectName = gameObject.name;
                    }

                    SpawnHitbox(i);
                }
            }
        }

        private void SpawnHitbox(int hitboxDataIndex)
        {
            GameObject hitboxAnchor = gameObject.GetComponentsInChildren<Transform>(true).Where(obj => obj.name == attackData.hitboxData[hitboxDataIndex].anchorObjectName).First().gameObject;

            GameObject newAttackHitbox = Instantiate(_hitboxPrefab, hitboxAnchor.transform);
            HitboxOnTrigger newAttackHitboxScript = newAttackHitbox.GetComponent<HitboxOnTrigger>();

            List<RollbackEntity> rollbackEntities = newAttackHitbox.GetComponents<RollbackEntity>().ToList();
            foreach (RollbackEntity rollbackEntity in rollbackEntities)
            {
                RollbackManager.QueueRegisterRollbackEntity(rollbackEntity);
            }

            newAttackHitboxScript.attackController = this;
            newAttackHitboxScript.attackerIndex = hitHandler.fighterHitIndex;
            newAttackHitboxScript.attack = _fighterStateMachine.StateMachineData.CurrentState.attackData;

            newAttackHitbox.transform.forward = transform.forward;
            newAttackHitboxScript.hitboxOnTriggerData.isActive = false;
            activeHitboxes.Add(newAttackHitbox);
        }

        public void DeactivateAttack(FighterState state)
        {
            if (!state.attackData)
            {
                return;
            }
                attackControllerData.attackActive = false;
                attackData = null;
                if (activeHitboxes.Count > 0)
                {
                    foreach (GameObject hitbox in activeHitboxes)
                    {
                        List<RollbackEntity> rollbackEntities = hitbox.GetComponents<RollbackEntity>().ToList();
                        foreach (RollbackEntity rollbackEntity in rollbackEntities)
                        {
                            RollbackManager.QueueUnregisterRollbackEntity(rollbackEntity);
                        }
                        Destroy(hitbox);
                    }
                    activeHitboxes.Clear();
                }
                if (instantiatedObjects.Count > 0)
                {
                    foreach (GameObject instantiatedObject in instantiatedObjects)
                    {
                        List<RollbackEntity> rollbackEntities = instantiatedObject.GetComponents<RollbackEntity>().ToList();
                        foreach (RollbackEntity rollbackEntity in rollbackEntities)
                        {
                            RollbackManager.QueueUnregisterRollbackEntity(rollbackEntity);
                        }
                        Destroy(instantiatedObject);
                    }
                    instantiatedObjects.Clear();
                }

                foreach (IEnumerator enumerator in attackCoroutines)
                {
                    StopCoroutine(enumerator);
                }
                attackCoroutines.Clear();
                clearNullPausables();
                ClearInactivePausables();
           
        }

        IEnumerator delayObjectActivation(GameObject attackObject, int framesDelay)
        {
            int currentFrames = 0;
            while (currentFrames < framesDelay)
            {
                yield return new WaitForFixedUpdate();
                currentFrames++;
            }
            attackObject.SetActive(true);
            getPausables(attackObject);
        }

        public dynamic GetInitialState()
        {
            return attackControllerData;
        }

        public void SetState(dynamic state)
        {
            attackControllerData = state;
        }

        public void SimulateFrame()
        {
            if (attackControllerData.attackActive && !attackControllerData.attackPaused && _fighterStateMachine.StateMachineData.CurrentState.attackData)
            {
                int currentFrame = (int)(_fighterStateMachine.StateMachineData.TimeInState * RollbackManager.FRAMERATE.AsInt);

                //Check if there are objects that have been instantiated and then set them active

                if (_fighterStateMachine.StateMachineData.CurrentState.attackData.objectsToInstantiate.Count > 0)
                {
                    for (int i = 0; i < _fighterStateMachine.StateMachineData.CurrentState.attackData.objectsToInstantiate.Count; i++)
                    {
                        if (_fighterStateMachine.StateMachineData.CurrentState.attackData.objectsToInstantiate[i].frameDelay == currentFrame)
                        {
                            if(!instantiatedObjects[i].activeSelf)
                            instantiatedObjects[i].SetActive(true);
                        }
                    }
                }

                //Check if there are active hitboxes on this frame and if so set their data to the data for this frame

                for (int i = 0; i < activeHitboxes.Count; i++)
                {
                    if (currentFrame < _fighterStateMachine.StateMachineData.CurrentState.attackData.numberOfFrames && currentFrame > 0)
                    {
                        if (_fighterStateMachine.StateMachineData.CurrentState.attackData.hitboxData[i].frameActiveData[currentFrame - 1])
                        {
                            if (activeHitboxes[i])
                            {
                                HitboxOnTrigger hitbox = activeHitboxes[i].GetComponent<HitboxOnTrigger>();
                                hitbox.hitboxOnTriggerData.isActive = true;
                                fp3 newSize = fp3.one * _fighterStateMachine.StateMachineData.CurrentState.attackData.hitboxData[i].sizeData[currentFrame - 1];
                                activeHitboxes[i].transform.localScale = newSize.ToVector3();
                                activeHitboxes[i].GetComponent<PhysicsBody>().currentState.position = activeHitboxes[i].transform.parent.position.ToFp3() + _fighterStateMachine.StateMachineData.CurrentState.attackData.hitboxData[i].offsetData[currentFrame - 1];
                            }  
                        }
                        else
                        {
                            if (activeHitboxes[i].gameObject)
                            {
                                if (activeHitboxes[i].gameObject.activeInHierarchy)
                                {
                                    HitboxOnTrigger hitbox = activeHitboxes[i].GetComponent<HitboxOnTrigger>();
                                    hitbox.hitboxOnTriggerData.isActive = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("Attack has too many frames for this animation/state. Abort!");
                    }
                }

                //if (fighter.attacks[currentAttackIndex].isMultihit)
                //{
                //    hitNotAllowedIndexes.Clear();
                //}
            }
        }

        public dynamic GetUpdatedState()
        {
            return attackControllerData;
        }

        public void UpdateVisuals()
        {
            return;
        }
    }
}