using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour {
    public enum State {
        Walking,
        Climbing
    }

    public Transform cam;

    [Header("Player Settings")] [SerializeField]
    private float mouseSensitivity = 1f;

    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float jumpSpeed = 5f;
    [SerializeField] private float climbingSpeed = 2f;
    [SerializeField] private float mass = 1f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float turnSmoothTime = 0.1f;
    [SerializeField] private float turnSmoothVelocity;

    public bool isHidden;

    private State _state;

    private CharacterController controller;

    // Inputs
    private PlayerInput input;
    private InputAction jumpAction;

    private Vector2 look;
    private InputAction lookAction;
    private InputAction moveAction;

    internal float movementSpeedMultiplier;
    internal Vector3 velocity;

    public float Height {
        get => controller.height;
        set => controller.height = value;
    }

    public State CurrentState {
        get => _state;
        set {
            _state = value;
            velocity = Vector3.zero;
        }
    }

    public Vector3 CurrentVelocity { get; private set; }

    private void Awake() {
        controller = GetComponent<CharacterController>();
        if (!input) input = GetComponent<PlayerInput>();
        moveAction = input.actions["Move"];
        lookAction = input.actions["Look"];
        jumpAction = input.actions["Jump"];
    }

    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    private void Update() {
        movementSpeedMultiplier = 1f;
        switch (CurrentState) {
            case State.Walking:
                UpdateGravity();
                UpdateMovement();
                UpdateLook();
                break;
            case State.Climbing:
                UpdateClimbing();
                UpdateLook();
                break;
        }
    }

    public event Action OnBeforeMove;

    private void UpdateGravity() {
        if (CurrentState == State.Climbing) return;
        var gravity = Physics.gravity * (mass * Time.deltaTime);
        velocity.y = controller.isGrounded ? -1f : velocity.y + gravity.y;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void UpdateMovement() {
        movementSpeedMultiplier = 1f;
        OnBeforeMove?.Invoke();

        var moveInput = moveAction.ReadValue<Vector2>();
        var input = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        input *= movementSpeed * movementSpeedMultiplier;

        if (input.magnitude > 0.1f) {
            var targetAngle = Mathf.Atan2(input.x, input.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity,
                turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            var moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            var targetVelocity = moveDir * (movementSpeed * movementSpeedMultiplier);
            CurrentVelocity = Vector3.Lerp(CurrentVelocity, targetVelocity, acceleration * Time.deltaTime);

            if (jumpAction.WasPressedThisFrame() && controller.isGrounded) velocity.y += jumpSpeed;
        }
        else {
            CurrentVelocity = Vector3.Lerp(CurrentVelocity, Vector3.zero, acceleration * Time.deltaTime);
        }

        controller.Move((CurrentVelocity + velocity) * Time.deltaTime);
    }

    private void UpdateClimbing() {
        var moveInput = moveAction.ReadValue<Vector2>();

        if ((controller.isGrounded && moveInput.y < 0f) || jumpAction.WasPressedThisFrame()) {
            CurrentState = State.Walking;
            return;
        }

        var climbDirection = new Vector3(0f, moveInput.y, 0f).normalized;
        climbDirection *= climbingSpeed;

        var factor = acceleration * Time.deltaTime;
        velocity = Vector3.Lerp(velocity, climbDirection, factor);

        controller.Move(velocity * Time.deltaTime);
    }

    private void UpdateLook() {
        look.x += lookAction.ReadValue<Vector2>().x * mouseSensitivity;
        look.y += lookAction.ReadValue<Vector2>().y * mouseSensitivity;

        look.y = Mathf.Clamp(look.y, -90f, 90f);
    }
}