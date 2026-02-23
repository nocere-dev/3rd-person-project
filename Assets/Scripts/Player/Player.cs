using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Player : MonoBehaviour
{
    public Transform cam;

    [SerializeField] private float mouseSensitivity = 1f;
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float jumpSpeed = 5f;
    [SerializeField] private float mass = 1f;
    [SerializeField] private PlayerInputs input;

    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    CharacterController controller;

    Vector2 look;
    Vector3 velocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (!input) input = GetComponent<PlayerInputs>();
    }
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateGravity();
        UpdateLook();
        UpdateMovement();
    }

    void UpdateGravity()
    {
        var gravity = Physics.gravity * mass * Time.deltaTime;
        velocity.y = controller.isGrounded ? -1f : velocity.y + gravity.y;
    }

    void UpdateMovement()
    {
        // var x = Input.GetAxisRaw("Horizontal");
        // var y = Input.GetAxisRaw("Vertical");
        var moveInput = input.move();
        Vector3 input = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        if (input.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(input.x, input.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            
            // if (input.GetButtonDown("Jump") && controller.isGrounded){
            //   velocity.y += jumpSpeed;
            // }
            controller.Move((moveDir * movementSpeed + velocity) * Time.deltaTime);
        }
    }

    void UpdateLook()
    {
        look.x += Input.GetAxis("Mouse X");
        look.y += Input.GetAxis("Mouse Y");

        look.y = Mathf.Clamp(look.y, -90f, 90f);
    }
}
