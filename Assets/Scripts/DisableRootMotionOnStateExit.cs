using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableRootMotionOnStateExit : StateMachineBehaviour {
    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
          animator.applyRootMotion = false; //ToDo: set when in air --> so take off still works 
    }
}