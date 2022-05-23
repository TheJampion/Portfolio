using FixedPoint;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MythrenFighter
{
    public class FighterAnimationController
    {
        public AnimationClipPlayable CurrentPlayable { get; private set; }
        public AnimationMixerPlayable CurrentMixerPlayable { get; private set; }
        public AnimationPlayableOutput PlayableOutput { get; private set; }
        public PlayableGraph PlayableGraph;

        public FighterAnimationController(Animator _animator )
        {
            PlayableGraph = PlayableGraph.Create();
            PlayableOutput = AnimationPlayableOutput.Create(PlayableGraph, "Animation", _animator);
            PlayableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
        }
        public void SetupAnimations(FighterState state)
        {
            //Setup whether or not to use a blend tree for the playable.  
            if (state.animationClips.Count == 1)
            {
                CurrentPlayable = AnimationClipPlayable.Create(PlayableGraph, state.animationClips[0]);
                PlayableOutput.SetSourcePlayable(CurrentPlayable);
                if (!CurrentMixerPlayable.IsNull()) CurrentMixerPlayable.Destroy();
                CurrentMixerPlayable = AnimationMixerPlayable.Null;
            }
            //This setup uses mixerPlayable to create a blend tree if there is more than one animation clip. Currently only supports a blend tree with 2 states
            else if (state.animationClips.Count > 1)
            {
                if (!CurrentMixerPlayable.IsNull()) CurrentMixerPlayable.Destroy();
                CurrentMixerPlayable = AnimationMixerPlayable.Create(PlayableGraph, state.animationClips.Count); //May need to destroy if its not being used(?)
                PlayableOutput.SetSourcePlayable(CurrentMixerPlayable);
                var clipPlayable0 = AnimationClipPlayable.Create(PlayableGraph, state.animationClips[0]);

                var clipPlayable1 = AnimationClipPlayable.Create(PlayableGraph, state.animationClips[1]);
                PlayableGraph.Connect(clipPlayable0, 0, CurrentMixerPlayable, 0);
                PlayableGraph.Connect(clipPlayable1, 0, CurrentMixerPlayable, 1);

                PlayableGraph.Play();
            }
        }

        public void PlayPlayable(float time)
        {
            if (!CurrentPlayable.IsNull())
            {
                CurrentPlayable.SetTime(time);
            }
            PlayableGraph.Evaluate((float)(fp._1 / RollbackManager.FRAMERATE));
        }

        public void PlayMixerPlayable(float time, float weight)
        {
            if (!CurrentMixerPlayable.IsNull())
            {
                CurrentMixerPlayable.SetTime(time);
                CurrentMixerPlayable.SetInputWeight(0, 1.0f - weight);
                CurrentMixerPlayable.SetInputWeight(1, weight);
            }
            PlayableGraph.Evaluate((float)(fp._1 / RollbackManager.FRAMERATE));
        }

        public void SetAnimationSpeed(float speed)
        {
            if (!CurrentPlayable.IsNull())
            {
                CurrentPlayable.SetSpeed(speed);
            }
            else
            {
                CurrentMixerPlayable.SetSpeed(speed);
            }
        }
    }
}
