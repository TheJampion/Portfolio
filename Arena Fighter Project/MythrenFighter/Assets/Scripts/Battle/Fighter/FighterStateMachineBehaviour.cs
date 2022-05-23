using UnityEngine;

namespace MythrenFighter
{
    public abstract class FighterStateMachineBehaviour : ScriptableObject
    {
        [HideInInspector]
        public FighterStateMachine fighterStateMachine;
        public InputManager inputManager;
        public int fighterSlot;
        //Get reference to animator state machine when the state gets set(?)
        public virtual void OnStateEnter()
        {
            //Set all initial state parameters
        }

        public virtual void OnStateUpdate()
        {
            //Check for changes every frame
        }

        public virtual void OnStateExit()
        {
            //Reset parameters upon exiting state
            //Decide whether to wait one frame before exiting or just exit(?)
            //Enter other state from here(?)
        }
    }
}