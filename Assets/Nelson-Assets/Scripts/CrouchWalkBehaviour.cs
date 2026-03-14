using UnityEngine;

public class CrouchWalkBehaviour : StateMachineBehaviour {
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state


    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        {
            if (Input.GetKeyUp(KeyCode.W)) animator.SetBool("isCrouchWalking", false);
            if (Input.GetKeyUp(KeyCode.A)) animator.SetBool("isCrouchWalking", false);
            if (Input.GetKeyUp(KeyCode.S)) animator.SetBool("isCrouchWalking", false);
            if (Input.GetKeyUp(KeyCode.D)) animator.SetBool("isCrouchWalking", false);
            if (Input.GetKeyDown(KeyCode.Space)) animator.SetTrigger("isJumping");
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}