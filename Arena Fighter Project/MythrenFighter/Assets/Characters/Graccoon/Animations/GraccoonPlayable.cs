using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

[System.Serializable]
public class GraccoonPlayable : MonoBehaviour
{
    public PlayableGraph playableGraph;
    public AnimationClipPlayable currentPlayable;
    public AnimationClip currentAnimation;
    private Animator animator;
    public float currentPlaySpeed;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        currentPlayable = AnimationPlayableUtilities.PlayClip(animator, currentAnimation, out playableGraph);
        currentPlayable.SetSpeed(currentPlaySpeed);
    }

    private void Update()
    {
        currentPlayable.SetSpeed(currentPlaySpeed);
    }

}
