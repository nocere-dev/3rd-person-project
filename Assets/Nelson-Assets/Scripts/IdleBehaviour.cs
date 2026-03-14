using UnityEngine;

public class IdleBehaviour : StateMachineBehaviour {
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        Debug.Log("Entered Idle State!");
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        Debug.Log("Entered Idle State!");
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (Input.GetKey(KeyCode.W)) animator.SetBool("isWalkng", true);
        if (Input.GetKey(KeyCode.S)) animator.SetBool("isWalkng", true);
        if (Input.GetKey(KeyCode.D)) animator.SetBool("isWalkng", true);
        if (Input.GetKey(KeyCode.A)) animator.SetBool("isWalkng", true);
        if (Input.GetKeyDown(KeyCode.Space)) animator.SetTrigger("isJumping");
        if (Input.GetKey(KeyCode.C)) animator.SetBool("isCrouched", true);
        // if (Input.GetKeyDown(KeyCode.Mouse0))
        // {
        // animator.SetTrigger("Attack 0");
        // }
    }

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