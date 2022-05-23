using MythrenFighter;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

public class ChangeActiveHitboxesOnMultipleFrames : OdinEditorWindow
{
    [ReadOnly] public AttackData attackData;
    public int startFrame;
    public int endFrame;
    public bool active = true;
    public event System.Action OnFinish;

    [Button(ButtonHeight = 40)]
    public void Finish()
    {
        if (startFrame < 1 || startFrame > attackData.numberOfFrames - 1 || endFrame < 2 || endFrame > attackData.numberOfFrames)
        {
            Debug.Log("Frame Range did not fit in correct values, values will automatically be fixed to be between 1 and the number of frames for the attack");
        }
        Mathf.Clamp(startFrame, 1, attackData.numberOfFrames - 1);
        Mathf.Clamp(endFrame, 2, attackData.numberOfFrames);
        OnFinish.Invoke();
        Close();
    }
}

public class StateSetup : OdinEditorWindow
{
    [Title("Set new name here")]
    public string stateName;
    public AnimationClip animationToAdd;
    public event System.Action OnFinish;

    [Button(ButtonHeight = 40)]
    public void Finish()
    {
        OnFinish.Invoke();
        Close();
    }
}


public class AttackSetup : OdinEditorWindow
{
    [Title("Set new name here")]
    public string attackName;
    public event System.Action OnFinish;

    [Button(ButtonHeight = 40)]
    public void Finish()
    {
        OnFinish.Invoke();
        Close();
    }
}
