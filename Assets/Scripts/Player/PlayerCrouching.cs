using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Player))]
public class PlayerCrouching : MonoBehaviour
{
    Player player;
    CharacterController controller;
    PlayerInput playerInput;
    InputAction crouchAction;

    [Header("Crouching Settings")]
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchTransitionSpeed = 5f;
    [SerializeField] private float crouchSpeedMultiplier = 0.5f;

    private float currentHeight;
    private float originalHeight;

    private bool crouchToggled;

    bool IsCrouching => originalHeight - currentHeight > 0.1f;

    void Awake()
    {
        player = GetComponent<Player>();
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        crouchAction = playerInput.actions["Crouch"];
    }

    void Start()
    {
        originalHeight = currentHeight = player.Height;
        controller.center = new Vector3(0, 1, 0);
        crouchToggled = false;
    }

    void OnEnable() => player.OnBeforeMove += OnBeforeMove;
    void OnDisable() => player.OnBeforeMove -= OnBeforeMove;

    void OnBeforeMove()
    {
        var requestCrouch = crouchAction.WasPressedThisFrame();
        if (requestCrouch)
        {
            crouchToggled = !crouchToggled;
        }

        var heightTarget = crouchToggled ? crouchHeight : originalHeight;

        if (IsCrouching && !crouchToggled)
        {
            var castOrigin = transform.position + new Vector3(0, currentHeight / 2, 0);
            if (Physics.Raycast(castOrigin, Vector3.up, out var hit, 0.2f))
            {
                var distanceToCeiling = hit.point.y - castOrigin.y;
                heightTarget = Mathf.Max(heightTarget + distanceToCeiling - 0.1f, crouchHeight);
            }
        }

        if (!Mathf.Approximately(heightTarget, currentHeight))
        {
            var crouchDelta = Time.deltaTime * crouchTransitionSpeed;
            currentHeight = Mathf.Lerp(currentHeight, heightTarget, crouchDelta);

            player.Height = currentHeight;
        }

        controller.center = IsCrouching
            ? new Vector3(0, currentHeight / 2, 0)
            : new Vector3(0, 1, 0);

        if(IsCrouching)
        {
            player.movementSpeedMultiplier = crouchSpeedMultiplier;
        }
    }
}
