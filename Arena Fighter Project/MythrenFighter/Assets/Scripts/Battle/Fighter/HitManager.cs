using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
namespace MythrenFighter
{
    public class HitManager : MonoBehaviour
    {

        // Variables
        private static int nextKey;
        [SerializeField]
        public static Dictionary<int, HitHandler> activeHitHandlers;

        // Dependencies - found with GetComponent or similar

        private void Awake()
        {
            nextKey = 0;
            activeHitHandlers = new Dictionary<int, HitHandler>();
        }

        private void OnDestroy()
        {
            activeHitHandlers.Clear();
        }


        public static int registerHitHandler(HitHandler hitHandler)
        {
            if (!activeHitHandlers.ContainsValue(hitHandler))
            {
                activeHitHandlers.Add(nextKey, hitHandler);
                nextKey++;
                return (nextKey - 1);
            }
            else
            {
                return (activeHitHandlers.FirstOrDefault(x => x.Value == hitHandler).Key);
            }
        }

        public static void unregisterHitHandler(int key)
        {
            activeHitHandlers.Remove(key);
        }

        public static void reportHit(int attackerIndex, int defenderIndex, AttackData attack)
        {
            activeHitHandlers[attackerIndex].performHit(attack);
            activeHitHandlers[defenderIndex].getHit(attack, activeHitHandlers[attackerIndex]);
            if (attack.rotatesCameraToSide)
            {
                if (activeHitHandlers.ContainsKey(defenderIndex) && activeHitHandlers.ContainsKey(attackerIndex))
                {
                    //Rotate the Camera to its side view
                }
            }
        }

        public static Transform getNearbyEnemyTransform(int myHitIndex, float maxTargetDistance, float maxTargetAngle)
        {
            Transform returnValue = null;
            float minDistance = Mathf.Infinity;

            //foreach (KeyValuePair<int, HitHandler> kvp in activeHitHandlers)
            //{
            //    if (kvp.Key != myHitIndex && kvp.Value.gameObject.activeInHierarchy && kvp.Value.enabled && activeHitHandlers[myHitIndex].mythrenSlot.tamer.teamIndex != kvp.Value.mythrenSlot.tamer.teamIndex)
            //    {
            //        Vector3 vectorBetween = (kvp.Value.transform.position - activeHitHandlers[myHitIndex].transform.position).noY();
            //        if (Vector3.Angle(activeHitHandlers[myHitIndex].transform.forward, vectorBetween) < maxTargetAngle && vectorBetween.magnitude < maxTargetDistance && vectorBetween.magnitude < minDistance)
            //        {
            //            returnValue = kvp.Value.transform;
            //            minDistance = vectorBetween.magnitude;
            //        }
            //    }
            //}

            return returnValue;
        }
    }
}