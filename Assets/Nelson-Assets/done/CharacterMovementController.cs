using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharacterMovementController : MonoBehaviour
{
    //Gravity Constant Value
    private const float Gravity = 9.5f;

    //Get Character Controller
    private CharacterController characterController;

    //Character's Velocity
    private Vector3 characterVelocity; //The value we'll be using to move our player by applying it to the Character Controller.
    
    //Get Camera Object
    private Transform cam;

    [Header("Movement Variables")]
    public float moveSpeed = 5f;
    public float jumpSpeed = 10f;
    public float gravityMultiplier = 2f;
    public float playerRotationSpeed = 0.05f;

    [Header("Ground Check Variables")]
    public GameObject groundChecker;
    public float distance = 0.1f;
    public bool isGrounded;
    public LayerMask groundMask;

    // Start is called before the first frame update
    void Start()
    {
        //Find the transform of the camera object in the scene
        cam = GameObject.FindGameObjectWithTag("MainCamera").transform;
        
        //Get the Character Controller component on the player object
        characterController = GetComponent<CharacterController>();
        
        //Lock and hide the cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        //Raycast Stuff
        RaycastHit hit;

        //Fires a raycast each frame from our ground checker at a length equal to 'distance', checking for a particular layer identified as 'groundMask'...
        if (Physics.Raycast(groundChecker.transform.position, groundChecker.transform.TransformDirection(Vector3.down), out hit, distance, groundMask))
        {
            //...and sets our 'isGrounded' bool to true if the ray hits an object on the identified layer...
            isGrounded = true;
        }
        else
        {
            //...or sets it to false if the ray doesn't hit any objects on the identified layer
            isGrounded = false;
        }

        //Draws the 'groundChecker' ray in the scene view to give a visual aid
        Debug.DrawRay(groundChecker.transform.position, groundChecker.transform.TransformDirection(Vector3.down) * distance, Color.yellow);

        //Movement Stuff
        if (isGrounded)
        {
            //Grounded Movement
            HandleGroundedMovement();

            //Jump
            Jump();
        }
        else
        {
            //In Air Movement
            HandleInAirMovement();
        }

        //Move our character based on the value of 'character velocity', calculated in the two Movement methods.
        characterController.Move(characterVelocity * Time.deltaTime);

        if (!isGrounded)
        {
            //Applies a downwards force to our player if they're not grounded, acting as gravity.
            characterVelocity += Vector3.down * (Gravity * Time.deltaTime * gravityMultiplier);
        }
    }

    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Zeros out our player's velocity and applies an upwards force to it equal to our jumpSpeed value.
            //Note: if we wanted the player to be locked into the jump trajectory, we could simply not zero out the 'characterVelocity'
            characterVelocity = new Vector3(0, 0, 0);
            characterVelocity += Vector3.up * jumpSpeed; 
        }
    }
    
    //Deals with movement if the player 'isGrounded'
    private void HandleGroundedMovement()
    {
        //Get the camera's forward and right vectors (Z and X Axis)
        Vector3 camForward = cam.forward; 
        Vector3 camRight = cam.right; 
        
        //Set the vertical value for the vectors to zero, so we're only calculating horizontal orientation
        //Note: this is to prevent moving our character vertically when we apply movement
        camForward.y = 0; 
        camRight.y = 0;
        
        //Normalize the vector values to keep movement more predictable and controlled.
        //Note: Normalizing means that we can preserve the direction but limit the length to exactly 1, so that it doesn't affect multiplications when calculating movement.
        camForward.Normalize();
        camRight.Normalize();
        
        //Check for any directional inputs and store them as a Vector2 value.
        Vector2 inputAxis = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        //Convert and store those input values to a Vector3, so that it can be used for movement in local 3D space.
        Vector3 inputSpaceMovement = new Vector3(inputAxis.x, 0, inputAxis.y);

        //Converts the stored input coordinates to the camera's horizontal orientation in world space...
        Vector3 worldSpaceMovement = (camForward * inputSpaceMovement.z + camRight * inputSpaceMovement.x);

        //Normalize the newly calculated vector to keep movement more predictable.
        worldSpaceMovement.Normalize();
        
        //...and applies them to the 'characterVelocity' value, multiplied by our 'moveSpeed' value.
        characterVelocity = worldSpaceMovement * moveSpeed;
        
        //This will then be used to move the character by applying the value to the character controller (Line 82)
        
        //Checks for any input along the horizontal or vertical axis.
        if (inputAxis.magnitude != 0)
        {
            //Calculate the rotation that we want the player to be at - this will be equal to our 'worldSpaceMovement' value (based off of our forward movement)
            Quaternion targetRotation = Quaternion.LookRotation(worldSpaceMovement, Vector3.up);
            
            //Now that we know where we want the player to rotate to, we can smoothly rotate the player from one point to another at a set ratio.
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, playerRotationSpeed);
        }
    }

    //Deals with movement if the player '!isGrounded'
    private void HandleInAirMovement()
    {
        //Get the camera's forward and right vectors (Z and X Axis)
        Vector3 camForward = cam.forward; 
        Vector3 camRight = cam.right; 
        
        //Set the vertical value for the vectors to zero, so we're only calculating horizontal orientation
        //Note: this is to prevent moving our character vertically when we apply movement
        camForward.y = 0; 
        camRight.y = 0;
        
        //Normalize the vector values to keep movement more predictable and controlled.
        //Note: Normalizing means that we can preserve the direction but limit the length to exactly 1, so that it doesn't affect multiplications when calculating movement.
        camForward.Normalize();
        camRight.Normalize();
        
        //Check for any directional inputs and store them as a Vector2 value.
        Vector2 inputAxis = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        //Convert and store those input values to a Vector3, so that it can be used for movement in local 3D space.
        Vector3 inputSpaceMovement = new Vector3(inputAxis.x, 0, inputAxis.y);

        //Converts the stored input coordinates to the players position in world space...
        Vector3 worldSpaceMovement = (camForward * inputSpaceMovement.z + camRight * inputSpaceMovement.x);

        //...and applies them directly to the character controller.
        characterController.Move(worldSpaceMovement * moveSpeed * Time.deltaTime);
        
        if (inputAxis.magnitude != 0)
        {
            Quaternion targetRotation = Quaternion.LookRotation(worldSpaceMovement, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, playerRotationSpeed);
        }
    }
}