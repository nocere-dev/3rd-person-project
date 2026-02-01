using UnityEngine;

///----------------------------------------------
/// 
/// TODO:
/// CONTINUE DEVELOPMENT OF NEW INPUT SYSTEM TO REPLACE OLD ONE
/// FIX HEAD COLLISION CHECK WHEN CROUCHING UNDER OBSTACLES
/// 
///----------------------------------------------

// Add new stances here like prone
public enum Stance
{
    Standing,
    Crouching
}

public struct CharacterInput
{
    public Vector2 Move;
    public bool Jump;
    public bool Crouch;
}

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public CharacterController _player;
    public Transform _playerObj;


    [Header("Settings")]
    [SerializeField] private float turnSmoothing = 0.25f;

    [Header("Stance Settings")]
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchHeight = 1.2f;

    [SerializeField] private float speed = 6f;
    [SerializeField] private float crouchSpeed = 0.5f;

    private Stance stance = Stance.Standing;

    private float turnSmoothingVelocity;


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _player.center = new Vector3(0f, 0f, 0f);
    }

    private void Update()
    {
        // Player rotation
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        // Will update to newer input system, just want to get it working for now.
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (stance == Stance.Standing) EnterCrouch();
            else ExitCrouch();
        }

        if (inputDir.magnitude >= 0.1f)
        {
            float cameraYaw = Camera.main.transform.eulerAngles.y;
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + cameraYaw;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothingVelocity, turnSmoothing);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            float currentSpeed = speed * (stance == Stance.Crouching ? crouchSpeed : 1);
            _player.Move(moveDir.normalized * currentSpeed * Time.deltaTime);
        }


    }

    // Setting the players stance and modifying the character based on the info for each stance, can be expanded with a new case and new stance in the enum
    private void EnterCrouch()
    {
        stance = Stance.Crouching;
        _player.height = crouchHeight;
        _player.center = new Vector3(0f, -0.4f, 0f);

    }

    private void ExitCrouch()
    {
        if (!CanStandUp()) return;

        stance = Stance.Standing;
        _player.height = standingHeight;
        _player.center = new Vector3(0f, 0f, 0f);
 
    }

    private bool CanStandUp()
    {
        float requiredHeadroom = standingHeight - _player.height;

        if (requiredHeadroom <= 0f) return true;

        Vector3 castOrigin = transform.position + Vector3.up * _player.height;

        float castRadius = _player.radius * 0.95f;

        bool blocked = Physics.SphereCast(castOrigin, castRadius, Vector3.up, out _, requiredHeadroom);

        return !blocked;
    }
}