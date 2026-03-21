using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Player))]
public class PlayerSprinting : MonoBehaviour {
    [SerializeField] private float sprintMultiplier = 1.5f;
    private CharacterController controller;
    Animator animator;

    private Player player;
    private PlayerInput playerInput;
    private InputAction sprintAction;

    private void Awake() {
        player = GetComponent<Player>();
        playerInput = GetComponent<PlayerInput>();
        sprintAction = playerInput.actions["Sprint"];
        animator = GetComponent<Animator>();
        if (!animator) animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable() {
        player.OnBeforeMove += OnBeforeMove;
        
    }

    private void OnDisable() {
        player.OnBeforeMove -= OnBeforeMove;
    }

    private void OnBeforeMove() {
        var sprintInput = sprintAction.ReadValue<float>();

        if (sprintInput == 0)
        {
            animator.SetBool("isRunning", false);
            return;
        }

        var forwardMovementFactor =
            Mathf.Clamp01(Vector3.Dot(player.transform.forward, player.CurrentVelocity.normalized));
        var multiplier = Mathf.Lerp(1f, sprintMultiplier, forwardMovementFactor);

        player.movementSpeedMultiplier *= multiplier;

        // Set animation
        animator.SetBool("isRunning", true);
    }
}