using Unity.VisualScripting.FullSerializer;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public CharacterController _player;
    public Transform _playerObj;


    [Header("Settings")]
    [SerializeField] private float turnSmoothing = 0.25f;
    [SerializeField] private float speed = 6f;

    private float turnSmoothingVelocity;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Player rotation
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        if (inputDir.magnitude >= 0.1f)
        {
            float cameraYaw = Camera.main.transform.eulerAngles.y;
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + cameraYaw;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothingVelocity, turnSmoothing);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            _player.Move(moveDir.normalized * speed * Time.deltaTime);
        }
    }
}