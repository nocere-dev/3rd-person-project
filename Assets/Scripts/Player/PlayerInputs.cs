using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
{
  [Header("References")]
  [SerializeField] private PlayerInput playerInput;

  private InputAction move;
  private InputAction look;
  private InputAction jump;
  private InputAction crouch;
  private InputAction sprint;
  private InputAction interact;

  public Vector2 Move { get; private set; }
  public Vector2 Look { get; private set; }
  public bool JumpPressed { get; private set; }
  public bool CrouchPressed { get; private set; }
  public bool SprintHeld { get; private set; }
  public bool InteractPressed { get; private set; }

  private void Awake() {
      if (!playerInput) playerInput = GetComponent<PlayerInput>();

      move = playerInput.actions["Move"];
      look = playerInput.actions["Look"];
      jump = playerInput.actions["Jump"];
      crouch = playerInput.actions["Crouch"];
      sprint = playerInput.actions["Sprint"];
      interact = playerInput.actions["Interact"];
  }

  private void OnEnable() {
      move.Enable();
      look.Enable();
      jump.Enable();
      crouch.Enable();
      sprint.Enable();
      interact.Enable();
  }

  private void OnDisable() {
      move.Disable();
      look.Disable();
      jump.Disable();
      crouch.Disable();
      sprint.Disable();
      interact.Disable();

  }

  private void Update() {
      // Continuous controls
      Move = move.ReadValue<Vector2>();
      Look = move.ReadValue<Vector2>();
      SprintHeld = sprint.IsPressed();

      //triggered inputs
      JumpPressed = jump.WasPressedThisFrame();
      CrouchPressed = crouch.WasPressedThisFrame();
  }

  public void ConsumeOneFrameButtons()
    {
        JumpPressed = false;
        CrouchPressed = false;
    }
}
