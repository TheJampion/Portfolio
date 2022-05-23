using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "BattleEnvironmentData", menuName = "MythrenFighter/BattleEnvironmentData")]
public class BattleEnvironmentData : ScriptableObject
{
    public string name;
    public Sprite portrait;
    public List<Vector3> spawnPositions;
}
