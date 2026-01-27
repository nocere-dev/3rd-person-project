using Unity.VisualScripting.FullSerializer;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    [Header("References")]
    public Transform _orientation;
    public Transform _player;
    public Transform _playerObj;
    public Rigidbody _playerPhys;

    public float rotationSpeed;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Orientation rotation
        Vector3 viewDir = _player.position - new Vector3(transform.position.x, _player.position.y, transform.position.z);
        _orientation.forward = viewDir.normalized;

        // Player rotation
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 inputDir = _orientation.forward * verticalInput + _orientation.right * horizontalInput;

        if (inputDir != Vector3.zero)
            _playerObj.forward = Vector3.Slerp(_playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
    }
}
