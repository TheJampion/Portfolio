using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using MythrenFighter;

public class AttackEditor : OdinMenuEditorWindow
{
    public static void OpenWindowSpecificAttack(AttackData attackData)
    {
        GetWindow<AttackEditor>().Show();
    }

    private CreateNewAttackData createNewAttackData;

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if(createNewAttackData != null)
        {
            DestroyImmediate(createNewAttackData.attackData);
        }
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree();
        //This needs to change to have you select the fighter's path
        createNewAttackData = new CreateNewAttackData();
        tree.Add("Create New", createNewAttackData);
        tree.AddAllAssetsAtPath("Fighters", "ScriptableObjects/AttackData", typeof(ScriptableObject), true);

        return tree;
    }

    public class CreateNewAttackData
    {
        public CreateNewAttackData()
        {
            attackData = ScriptableObject.CreateInstance<AttackData>();
            attackData.attackName = "New Attack Data";
        }
        public AttackData attackData;

        [Button("Add New Attack Data")]
        private void CreateNewData()
        {
            AssetDatabase.CreateAsset(attackData, "Assets/Scripts/ScriptableObjects" + attackData.attackName + ".asset");
            AssetDatabase.SaveAssets();

            attackData = ScriptableObject.CreateInstance<AttackData>();
            attackData.attackName = "New Attack Data";
        }
    }
}
