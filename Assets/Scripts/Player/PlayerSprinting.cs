using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Player))]
public class PlayerSprinting : MonoBehaviour
{
    [SerializeField] private float sprintMultiplier = 1.5f;

    Player player;
    PlayerInput playerInput;
    InputAction sprintAction;
    void Awake()
    {
        player = GetComponent<Player>();
        playerInput = GetComponent<PlayerInput>();
        sprintAction = playerInput.actions["Sprint"];
    }

    void OnEnable() => player.OnBeforeMove += OnBeforeMove;
    void OnDisable() => player.OnBeforeMove -= OnBeforeMove;

    void OnBeforeMove()
    {
        var sprintInput = sprintAction.ReadValue<float>();
        if (sprintInput == 0) return;

        var forwardMovementFactor = Mathf.Clamp01(Vector3.Dot(player.transform.forward, player.CurrentVelocity.normalized));
        var multiplier = Mathf.Lerp(1f, sprintMultiplier, forwardMovementFactor);

        player.movementSpeedMultiplier *= multiplier;
    }
}
