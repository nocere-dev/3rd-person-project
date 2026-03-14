using UnityEngine;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(PlayerCrouching))]
public class PlayerAnimator : MonoBehaviour {
    // Cached hashes - matching your existing typo "isWalkng" so nothing breaks in the Animator
    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    private static readonly int IsCrouched = Animator.StringToHash("isCrouched");
    private static readonly int IsCrouchWalking = Animator.StringToHash("isCrouchWalking");
    private static readonly int IsJumping = Animator.StringToHash("isJumping");
    [SerializeField] private Animator animator;
    private CharacterController controller;
    private PlayerCrouching crouching;

    private Player player;

    private bool wasGrounded;

    private void Awake() {
        player = GetComponent<Player>();
        controller = GetComponent<CharacterController>();
        crouching = GetComponent<PlayerCrouching>();
    }

    private void Update() {
        var isGrounded = controller.isGrounded;
        var isMoving = controller.velocity.magnitude > 0.1f;
        var isCrouching = crouching.IsCrouching; // see note below

        animator.SetBool(IsWalking, isMoving && !isCrouching);
        animator.SetBool(IsCrouched, isCrouching);
        animator.SetBool(IsCrouchWalking, isMoving && isCrouching);

        // Fire the jump trigger on the frame the player leaves the ground going upward
        if (!isGrounded && wasGrounded && player.velocity.y > 0f) animator.SetTrigger(IsJumping);

        wasGrounded = isGrounded;
    }
}