using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableRootMotionOnStateEnter : StateMachineBehaviour {
    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("IsUsingRootMotion", false); //ToDo: set when in air --> so take off still works 
    }
}