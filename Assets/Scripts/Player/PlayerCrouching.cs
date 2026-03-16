using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Player))]
public class PlayerCrouching : MonoBehaviour {
    [Header("Crouching Settings")] [SerializeField]
    private float crouchHeight = 1f;

    [SerializeField] private float crouchTransitionSpeed = 5f;
    [SerializeField] private float crouchSpeedMultiplier = 0.5f;
    private CharacterController controller;
    private InputAction crouchAction;

    private bool crouchToggled;

    private float currentHeight;
    private float originalHeight;
    private Player player;
    private PlayerInput playerInput;

    public bool isCrouching => originalHeight - currentHeight > 0.1f;

    private void Awake() {
        player = GetComponent<Player>();
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        crouchAction = playerInput.actions["Crouch"];
    }

    private void Start() {
        originalHeight = currentHeight = player.Height;
        controller.center = new Vector3(0, 1, 0);
        crouchToggled = false;
    }

    private void OnEnable() {
        player.OnBeforeMove += OnBeforeMove;
    }

    private void OnDisable() {
        player.OnBeforeMove -= OnBeforeMove;
    }

    private void OnBeforeMove() {
        var requestCrouch = crouchAction.WasPressedThisFrame();
        if (requestCrouch) crouchToggled = !crouchToggled;

        var heightTarget = crouchToggled ? crouchHeight : originalHeight;

        if (isCrouching && !crouchToggled) {
            var castOrigin = transform.position + new Vector3(0, currentHeight / 2, 0);
            if (Physics.Raycast(castOrigin, Vector3.up, out var hit, 0.2f)) {
                var distanceToCeiling = hit.point.y - castOrigin.y;
                heightTarget = Mathf.Max(heightTarget + distanceToCeiling - 0.1f, crouchHeight);
            }
        }

        if (!Mathf.Approximately(heightTarget, currentHeight)) {
            var crouchDelta = Time.deltaTime * crouchTransitionSpeed;
            currentHeight = Mathf.Lerp(currentHeight, heightTarget, crouchDelta);

            player.Height = currentHeight;
        }

        controller.center = isCrouching
            ? new Vector3(0, currentHeight / 2, 0)
            : new Vector3(0, 1, 0);

        if (isCrouching) player.movementSpeedMultiplier = crouchSpeedMultiplier;
    }
}