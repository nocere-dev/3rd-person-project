using UnityEngine;

public class WheelAnimationControl : MonoBehaviour
{
    public Rigidbody rb;
    public Animator animator;

    void Update()
    {
        float speed = rb.linearVelocity.magnitude;

        if (speed < 0.01f)
        {
            animator.speed = 0f;   
        }
        else
        {
            animator.speed = 1f;   
        }
    }
}