using UnityEngine;

public class ResetAnimatorBool : StateMachineBehaviour {
    public string useRootMotionBool = "IsUsingRootMotion";
    public bool useRootMotionStatus = false;

    public string canRotateBool = "CanRotate";
    public bool canRotateStatus = true;


    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(useRootMotionBool, useRootMotionStatus);
        animator.SetBool(canRotateBool, canRotateStatus);
    }
}