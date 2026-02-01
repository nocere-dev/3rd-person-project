using Unity.VisualScripting.FullSerializer;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCam : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform _playerCam;
    [SerializeField] PlayerInputManager _input;

    [Header("Settings")]
    [SerializeField] private float cameraSensitivity = 1f;
    [SerializeField] private float maxPitch = 80f;

    private float pitch;
    private float yaw;

    public float Yaw => yaw;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
        if (pitch > 180f) pitch -= 360f;
    }

    private void Update()
    {
        float mouseX = Input.GetAxisRaw
    }
}
