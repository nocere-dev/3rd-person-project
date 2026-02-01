using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
{
    private PlayerControls _inputActions;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _inputActions = new PlayerControls();
        _inputActions.Enable();

    }

    // Update is called once per frame
    void Update()
    {
        var input = _inputActions.Gameplay;

        var characterInput = new CharacterInput
        {
            Move = input.Move.ReadValue<Vector2>(),
            Jump = input.Jump.WasPressedThisFrame(),
            Crouch = input.Crouch.WasPressedThisFrame(),
        }; 
    }
}
