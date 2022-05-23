using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using FixedPoint;
using Rewired;

namespace MythrenFighter
{
    public class FocusTargetData
    {
        public GameObject player1;
        public GameObject player2;
        public float player1CenterHeight;
        public float player2CenterHeight;
        public Vector3 p1Pos;
        public Vector3 p2Pos;
        public Vector3 vectorBetween;
        public float distanceBetween;
        public Vector3 focusPoint;
        public float zoomAmount;
    }

    public class BattleCamera : MonoBehaviour
    {
        // Battle Camera
        public static readonly Vector3 BATTLE_CAMERA_START_OFFSET = new Vector3(4f, 2f, 1.5f);
        public static readonly float BATTLE_CAMERA_HEIGHT = 4f;
        public static readonly float BATTLE_CAMERA_ZOOM_MODIFIER_DEFAULT = 1.1f;
        public static readonly float BATTLE_CAMERA_ZOOM_MODIFIER_MINIMUM = 0.7f;
        public static readonly float BATTLE_CAMERA_ZOOM_MODIFIER_NUMERATOR = 25f;
        public static readonly float BATTLE_CAMERA_CENTER_DIAMETER = 5.5f;
        public static readonly float BATTLE_CAMERA_ROTATE_SPEED = 30f;
        public static readonly float BATTLE_CAMERA_ROTATE_ANGLE = 18f;

        public enum BattleCameraModes { LockOn, RotateToSide, Solo }

        // Variables
        public BattleCameraModes cameraMode;
        private BattleCameraModes previousCameraMode;
        public Fighter lockedOnSlot;
        private Fighter rotationTarget;
        private bool cameraRotationEnabled = true;
        private float mouseDeltaX;
        private float rotateToSideTimer = 0f;
        public Vector3 sideVector = Vector3.zero;
        public Vector3 focusPoint = Vector3.zero;
        public bool cameraEnabled = true;

        private Rewired.Player rewiredPlayer;

        // Dependencies - assigned in inspector
        [SerializeField]
        private LayerMask envOnly;
        [SerializeField]
        private Vector3 soloCameraOffset = Vector3.zero;
        [SerializeField]
        private float rotationLerpSpeed = 0;
        [SerializeField]
        private float zoomLerpSpeed = 0;
        [SerializeField]
        private float lockOnCameraRotateSpeed = 0;
        [SerializeField]
        private float defaultCameraSensitivity;
        [SerializeField]
        private float minRotateToSideTime;

        // Dependencies - found with GetComponent or similar
        private BattleManager battleManager;

        private void Awake()
        {
            battleManager = FindObjectOfType<BattleManager>();
            rewiredPlayer = ReInput.players.GetPlayer(0);
        }

        public void StartCamera()
        {
            if (battleManager.fighters.Count > 1)
            {
                setCameraMode(BattleCameraModes.LockOn);
            }
            else
            {
                setCameraMode(BattleCameraModes.Solo);
            }
        }

        public void enableCameraInput()
        {
            cameraRotationEnabled = true;
        }

        public void disableCameraInput()
        {
            cameraRotationEnabled = false;
        }

        public void setCameraEnabled(bool enabled)
        {
            cameraEnabled = enabled;
        }

        public void enterRotateMode(Fighter attackingMythren, Fighter defendingMythren)
        {
            Fighter player = battleManager.fighters[0];

            if (attackingMythren == player || defendingMythren == player)
            {
                if (cameraMode == BattleCameraModes.Solo)
                {
                    if (attackingMythren == player)
                    {
                        rotationTarget = defendingMythren;
                    }
                    else
                    {
                        rotationTarget = attackingMythren;
                    }

                    setCameraMode(BattleCameraModes.RotateToSide);
                    recalculateSideVector(attackingMythren, defendingMythren);
                }
                else if (cameraMode == BattleCameraModes.LockOn)
                {
                    if (attackingMythren == player || defendingMythren == player)
                    {
                        if (attackingMythren == player)
                        {
                            rotationTarget = defendingMythren;
                            lockedOnSlot = defendingMythren;
                        }
                        else
                        {
                            rotationTarget = attackingMythren;
                        }
                        setCameraMode(BattleCameraModes.RotateToSide);
                        recalculateSideVector(attackingMythren, defendingMythren);
                    }
                }
                else if (cameraMode == BattleCameraModes.RotateToSide)
                {
                    if (previousCameraMode == BattleCameraModes.Solo)
                    {
                        if (attackingMythren == player)
                        {
                            rotationTarget = defendingMythren;
                        }
                        else
                        {
                            rotationTarget = attackingMythren;
                        }
                        rotateToSideTimer = 0f;
                        recalculateSideVector(attackingMythren, defendingMythren);
                    }
                    if (previousCameraMode == BattleCameraModes.LockOn && (lockedOnSlot == attackingMythren || lockedOnSlot == defendingMythren))
                    {
                        rotateToSideTimer = 0f;
                        recalculateSideVector(attackingMythren, defendingMythren);
                    }
                }
            }
        }

        public void setCameraMode(BattleCameraModes cameraMode)
        {
            previousCameraMode = this.cameraMode;
            this.cameraMode = cameraMode;

            if (cameraMode == BattleCameraModes.LockOn)
            {
                recalculateLockOnTarget();
            }
            else if (cameraMode == BattleCameraModes.RotateToSide)
            {
                rotateToSideTimer = 0f;
            }
        }

        public void recalculateLockOnTarget()
        {
            if (lockedOnSlot == null)
            {
                lockedOnSlot = battleManager.fighters[1];
            }
            else if (battleManager.fighters.Count <= 1)
            {
                setCameraMode(BattleCameraModes.Solo);
            }
        }

        public void initializeCameraPosition()
        {
            Vector3 position = Vector3.zero;
            BattleLoadData loadData = GameDataManager.Instance.battleLoadData;

            if (battleManager.fighters.Count > 1)
            {
                FocusTargetData focusTargetData = getFocusTargetData(battleManager.fighters[0], battleManager.fighters[1]);

                focusPoint = focusTargetData.focusPoint;

                position = Quaternion.AngleAxis(BATTLE_CAMERA_ROTATE_ANGLE, Vector3.up) * (loadData.battleEnvironment.spawnPositions[0].noY() - focusPoint.noY());
                position = position.normalized * focusTargetData.zoomAmount + BATTLE_CAMERA_HEIGHT * Vector3.up;
            }
            else
            {
                //float playerCenterHeight = GameData.getMythrenStaticData(battleManager.mythrenSlots[0].getActiveMythren().mythrenData.mythrenIndex).modelHeight / 2;
                focusPoint = battleManager.fighters[0].gameObject.transform.position /*+ playerCenterHeight * Vector3.up*/;

                position = soloCameraOffset + BATTLE_CAMERA_HEIGHT * Vector3.up;
            }

            transform.position = focusPoint + position;
            Vector3 look = focusPoint - transform.position;
            if (look != Vector3.zero)
            {
                transform.forward = look;
            }
        }

        // Modes
        private void lockOnMode()
        {
            if (battleManager.fighters[0])
            {
                FocusTargetData focusTargetData = getFocusTargetData(battleManager.fighters[0], lockedOnSlot);

                if (Time.timeScale != 0 && focusTargetData.player1 != null && focusTargetData.player2 != null)
                {
                    // Calculates cameraOffset
                    Vector3 closerPlayerPos = focusTargetData.p1Pos.noY();
                    if ((focusTargetData.p2Pos.noY() - transform.position.noY()).magnitude < (focusTargetData.p1Pos.noY() - transform.position.noY()).magnitude)
                    {
                        closerPlayerPos = focusTargetData.p2Pos.noY();
                    }

                    Vector3 cameraFocusVector = transform.position.noY() - focusPoint.noY();
                    Vector3 goalCameraOffset = cameraFocusVector;
                    Vector3 closerPlayerFocusVector = closerPlayerPos.noY() - focusPoint.noY();
                    float angleBetweenClosestPlayerFocusVectorAndCameraFocusVector = Vector3.Angle(cameraFocusVector, closerPlayerFocusVector);
                    if ((focusTargetData.distanceBetween > BATTLE_CAMERA_CENTER_DIAMETER) &&
                        angleBetweenClosestPlayerFocusVectorAndCameraFocusVector > BATTLE_CAMERA_ROTATE_ANGLE)
                    {
                        float signedAngleBetweenClosestPlayerFocusVectorAndCameraFocusVector = Vector3.SignedAngle(cameraFocusVector, closerPlayerFocusVector, Vector3.up);
                        if (signedAngleBetweenClosestPlayerFocusVectorAndCameraFocusVector > 0)
                        {
                            goalCameraOffset = Quaternion.AngleAxis(-BATTLE_CAMERA_ROTATE_ANGLE, Vector3.up) * closerPlayerFocusVector;
                        }
                        else if (signedAngleBetweenClosestPlayerFocusVectorAndCameraFocusVector < 0)
                        {
                            goalCameraOffset = Quaternion.AngleAxis(BATTLE_CAMERA_ROTATE_ANGLE, Vector3.up) * closerPlayerFocusVector;
                        }
                    }
                    if (focusTargetData.distanceBetween < BATTLE_CAMERA_CENTER_DIAMETER)
                    {
                        focusTargetData.distanceBetween = BATTLE_CAMERA_CENTER_DIAMETER;
                    }

                    if (getActiveEnemySlotCount() > 1)
                    {
                        goalCameraOffset = goalCameraOffset.normalized * (focusTargetData.zoomAmount * 1.3f);
                    }
                    else
                    {
                        goalCameraOffset = goalCameraOffset.normalized * focusTargetData.zoomAmount;
                    }

                    Vector3 currentCameraOffset = transform.position.noY() - focusPoint.noY();

                    float focusPointLerpTime = (focusTargetData.focusPoint - focusPoint).magnitude / zoomLerpSpeed;
                    float zoomLerpTime = Mathf.Abs(goalCameraOffset.magnitude - currentCameraOffset.magnitude) / zoomLerpSpeed;
                    float rotationLerpTime = Vector3.Angle(currentCameraOffset, goalCameraOffset) * Mathf.PI / 180 / lockOnCameraRotateSpeed;

                    float maxTime = Mathf.Max(focusPointLerpTime, zoomLerpTime, rotationLerpTime);

                    float relativeFocusLerp = zoomLerpSpeed;
                    float relativeZoomLerp = zoomLerpSpeed;
                    float relativeRotationLerp = lockOnCameraRotateSpeed;
                    if (maxTime != 0)
                    {
                        relativeFocusLerp = zoomLerpSpeed * focusPointLerpTime / maxTime;
                        relativeZoomLerp = zoomLerpSpeed * zoomLerpTime / maxTime;
                        relativeRotationLerp = lockOnCameraRotateSpeed * rotationLerpTime / maxTime;
                    }

                    focusPoint = Functions.interpolateVector3(focusPoint, focusTargetData.focusPoint, relativeFocusLerp);
                    Vector3 cameraOffset = Vector3.Slerp(currentCameraOffset, goalCameraOffset, 9f * Time.deltaTime);
                    if (getActiveEnemySlotCount() > 1)
                    {
                        cameraOffset.y = BATTLE_CAMERA_HEIGHT * 1.5f;
                    }
                    else
                    {
                        cameraOffset.y = BATTLE_CAMERA_HEIGHT;
                    }

                    // Shortens cameraOffset so it can't go through objects aside from water
                    //RaycastHit hit;
                    //if (Physics.Raycast(focusPoint, -look, out hit, cameraOffset.magnitude, envOnly))
                    //{
                    //    cameraOffset *= hit.distance / cameraOffset.magnitude;
                    //}

                    // Sets camera position and forward
                    transform.position = focusPoint + cameraOffset;
                    Vector3 look = focusPoint - transform.position;
                    if (look != Vector3.zero)
                    {
                        transform.forward = look;
                    }
                }
                else
                {
                    recalculateLockOnTarget();
                }
            }
        }

        private FocusTargetData getFocusTargetData(Fighter slot1, Fighter slot2)
        {
            FocusTargetData focusTargetData = new FocusTargetData();
            focusTargetData.player1 = slot1.gameObject;
            //focusTargetData.player1CenterHeight = GameData.getMythrenStaticData(slot1.getActiveMythren().mythrenData.mythrenIndex).modelHeight / 2;
            focusTargetData.p1Pos = focusTargetData.player1.transform.position + focusTargetData.player1CenterHeight * Vector3.up;

            focusTargetData.player2 = null;
            focusTargetData.player2CenterHeight = 0;
            focusTargetData.p2Pos = Vector3.zero;

            focusTargetData.player2 = slot2.gameObject;
            //focusTargetData.player2CenterHeight = GameData.getMythrenStaticData(slot2.getActiveMythren().mythrenData.mythrenIndex).modelHeight / 2;
            focusTargetData.p2Pos = focusTargetData.player2.transform.position + focusTargetData.player2CenterHeight * Vector3.up;

            focusTargetData.vectorBetween = focusTargetData.p2Pos - focusTargetData.p1Pos;
            focusTargetData.distanceBetween = focusTargetData.vectorBetween.magnitude;

            focusTargetData.focusPoint = (focusTargetData.p1Pos + focusTargetData.p2Pos) / 2;

            focusTargetData.zoomAmount = BATTLE_CAMERA_ZOOM_MODIFIER_DEFAULT;
            if (focusTargetData.distanceBetween > BATTLE_CAMERA_CENTER_DIAMETER)
            {
                float distanceToCenterWallFromPlayer = focusTargetData.distanceBetween / 2 - BATTLE_CAMERA_CENTER_DIAMETER / 2;
                focusTargetData.zoomAmount *= (BATTLE_CAMERA_ZOOM_MODIFIER_NUMERATOR - distanceToCenterWallFromPlayer) / BATTLE_CAMERA_ZOOM_MODIFIER_NUMERATOR;
            }
            if (focusTargetData.zoomAmount < BATTLE_CAMERA_ZOOM_MODIFIER_MINIMUM)
            {
                focusTargetData.zoomAmount = BATTLE_CAMERA_ZOOM_MODIFIER_MINIMUM;
            }
            if (focusTargetData.distanceBetween < BATTLE_CAMERA_CENTER_DIAMETER)
            {
                focusTargetData.distanceBetween = BATTLE_CAMERA_CENTER_DIAMETER;
            }
            focusTargetData.zoomAmount *= focusTargetData.distanceBetween;

            return focusTargetData;
        }

        private void recalculateSideVector(Fighter slot1, Fighter slot2)
        {
            FocusTargetData focusTargetData = getFocusTargetData(slot1, slot2);

            sideVector = Vector3.Cross(focusTargetData.vectorBetween.noY(), Vector3.up).normalized;
            if ((focusTargetData.focusPoint + sideVector - transform.position.noY()).magnitude > (focusTargetData.focusPoint - sideVector - transform.position.noY()).magnitude)
            {
                sideVector *= -1;
            }
        }

        private void rotateToSideMode()
        {
            FocusTargetData focusTargetData = getFocusTargetData(battleManager.fighters[0], rotationTarget);

            if (Time.timeScale != 0 && focusTargetData.player1 != null && focusTargetData.player2 != null)
            {
                Vector3 goalCameraOffset = sideVector.normalized * focusTargetData.zoomAmount;

                Vector3 currentCameraOffset = transform.position.noY() - focusPoint.noY();

                float focusPointLerpTime = (focusTargetData.focusPoint - focusPoint).magnitude / zoomLerpSpeed;
                float zoomLerpTime = Mathf.Abs(goalCameraOffset.magnitude - currentCameraOffset.magnitude) / zoomLerpSpeed;
                float rotationLerpTime = Vector3.Angle(currentCameraOffset, goalCameraOffset) * Mathf.PI / 180 / rotationLerpSpeed;

                float maxTime = Mathf.Max(focusPointLerpTime, zoomLerpTime, rotationLerpTime);

                float relativeFocusLerp = zoomLerpSpeed;
                float relativeZoomLerp = zoomLerpSpeed;
                float relativeRotationLerp = rotationLerpSpeed;
                if (maxTime != 0)
                {
                    relativeFocusLerp = zoomLerpSpeed * focusPointLerpTime / maxTime;
                    relativeZoomLerp = zoomLerpSpeed * zoomLerpTime / maxTime;
                    relativeRotationLerp = rotationLerpSpeed * rotationLerpTime / maxTime;
                }

                focusPoint = Functions.interpolateVector3(focusPoint, focusTargetData.focusPoint, relativeFocusLerp);
                Vector3 cameraOffset = Vector3.RotateTowards(currentCameraOffset, goalCameraOffset, relativeRotationLerp, relativeZoomLerp);
                cameraOffset.y = BATTLE_CAMERA_HEIGHT;

                rotateToSideTimer += Time.deltaTime;

                if (cameraOffset.noY() == goalCameraOffset.noY() && rotateToSideTimer >= minRotateToSideTime)
                {
                    setCameraMode(previousCameraMode);
                }

                // Sets camera position and forward
                transform.position = focusPoint + cameraOffset;
                Vector3 look = focusPoint - transform.position;
                if (look != Vector3.zero)
                {
                    transform.forward = look;
                }
            }
            else
            {
                setCameraMode(previousCameraMode);
            }
        }

        private void checkChangeLockedOnTarget()
        {
            List<Fighter> viableTargets = battleManager.fighters.Skip(1).ToList();
            int lockedOnSlotIndex = viableTargets.IndexOf(lockedOnSlot);

            //if (battleManager.fighters[0].activeInputHandler.getRightStickUpInput())
            //{
            //    lockedOnSlot = viableTargets[(lockedOnSlotIndex + 1) % viableTargets.Count];
            //}
            //else if (battleManager.fighters[0].activeInputHandler.getRightStickDownInput())
            //{
            //    lockedOnSlot = viableTargets[Functions.modulus(lockedOnSlotIndex - 1, viableTargets.Count)];
            //}
        }

        public int getActiveEnemySlotCount()
        {
            return battleManager.fighters.Skip(1).Count();
        }

        // Update
        private void Update()
        {
            if (cameraEnabled)
            {
                if (cameraMode == BattleCameraModes.Solo)
                {
                    //soloMode();
                }
                else if (cameraMode == BattleCameraModes.LockOn)
                {
                    lockOnMode();
                    checkChangeLockedOnTarget();
                }
                else if (cameraMode == BattleCameraModes.RotateToSide)
                {
                    rotateToSideMode();
                    checkChangeLockedOnTarget();
                }
            }
        }
    }
}