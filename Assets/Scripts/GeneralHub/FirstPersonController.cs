using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{

    // Variable to make sure that the user is able to move:
    public bool CanMove { get; private set; } = true;
    // Checking if the player is sprinting or not
    private bool IsSprinting => canSprint && Input.GetKey(sprintKey);
    // Checking if the player is able to jump (On ground and pressing space)
    private bool ShouldJump => Input.GetKey(jumpKey) && characterController.isGrounded;
    // Checking if the player is crouching
    private bool IsCrouching => canCrouch && Input.GetKey(crouchKey);
    // Checking if the player is able to crouch
    private bool ShouldCrouch => Input.GetKey(crouchKey) && characterController.isGrounded && !duringCrouchAnmation;

    // Setting up the movement parameters:
    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float slopeSpeed = 8.0f;

    // Setting up the player's control on their field of view
    [Header("Look Parameters")]
    [SerializeField, Range(1, 10)] private float lookSpeedX = 2.0f; // mouse sensitivity to look around in the x direction
    [SerializeField, Range(1, 10)] private float lookSpeedY = 2.0f; // mouse sensitivity to look around in the y direction
    [SerializeField, Range(1, 180)] private float upperLookLimit = 80.0f; // How much we can look up
    [SerializeField, Range(1, 180)] private float lowerLookLimit = 80.0f; // How much we can look down

    // Just to help us choose between the features we want
    [Header("Functional Options")]
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool canCrouch = true;
    [SerializeField] private bool canUseHeadbob = true;
    [SerializeField] private bool WillSlideOnSlopes = true;

    // Mapping Keyboard buttons to player functionality
    [Header("Controls")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;

    // Setting up player's jumping
    [Header("Jumping Parameters")]
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float gravity = 30.0f;

    // Setting up a crouching mechanic
    [Header("Crouch Parameters")]
    [SerializeField] private float crouchHeight = 0.5f; // Crouch Height
    [SerializeField] private float standingHeight = 2.0f; // Stand Height
    [SerializeField] private float timeToCrouch = 0.25f; // How long it takes to crouch and stand
    [SerializeField] private Vector3 crouchingCenter = new Vector3(0,0.5f,0); // center of the character when they're crouching
    [SerializeField] private Vector3 standingCenter = new Vector3(0, 0, 0); // center of the character when they're standing
    private bool isCrouching; 
    private bool duringCrouchAnmation; // Moving from crouching to standing

    // Setting up the headbob (Was in the tutorial I followed and it adds some immersive factor to the movement)
    [Header("Headbob Parameters")]
    [SerializeField] private float walkBobSpeed = 14f; // Speed of the bob while walking
    [SerializeField] private float walkBobAmount = 0.05f; // Intensity of the bob while walking
    [SerializeField] private float sprintBobSpeed = 18f; // Speed of the bob while sprinting
    [SerializeField] private float sprintBobAmount = 0.11f; // Intensity of the bob while sprinting
    [SerializeField] private float crouchBobSpeed = 8f; // Speed of the bob while crouching
    [SerializeField] private float crouchBobAmount = 0.025f; // Intensity of the bob while crouching
    private float defaultYPos = 0; // The vertical camera position
    private float timer;

    // Setting up sliding parameters
    private Vector3 hitPointNormal;
    private bool IsSliding
    {
        get
        {
            // Check if the player is sliding on a steep slope
            if (characterController.isGrounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, 1.5f))
            {
                hitPointNormal = slopeHit.normal; // Get the slope's normal vector

                return Vector3.Angle(hitPointNormal, Vector3.up) > characterController.slopeLimit; // the slope steepness
            }
            else
            {
                return false;
            }
        }
    }

    private Camera playerCamera;
    private CharacterController characterController;

    private Vector3 moveDirection; // The direction we're moving in
    private Vector2 currentInput; // Keyboard input for the movement

    private float rotationX = 0;

    void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        defaultYPos = playerCamera.transform.localPosition.y; // This is for the headbob
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if(CanMove)
        {
            HandleMovementInput();
            HandleMouseLook();

            if (canJump)
            {
                HandleJump();
            }

            if (canCrouch)
            {
                HandleCrouch();
            }
            if (canUseHeadbob)
            {
                HandleHeadBob();
            }
            ApplyFinalMovement();

        }
    }

    // This function will map the keyboard keys to the actual movement functions that we need to link them to
    private void HandleMovementInput()
    {
        currentInput = new Vector2((isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Vertical"), (isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Horizontal"));

        float moveDirectionY = moveDirection.y;
        moveDirection = (transform.TransformDirection(Vector3.forward) * currentInput.x) + (transform.TransformDirection(Vector3.right) * currentInput.y);
        if (IsSprinting)
        {
            moveDirection = moveDirection.normalized * Mathf.Clamp(moveDirection.magnitude, 0, sprintSpeed);
        }
        else if (IsCrouching)
        {
            moveDirection = moveDirection.normalized * Mathf.Clamp(moveDirection.magnitude, 0, crouchSpeed);
        }
        else 
        {
            moveDirection = moveDirection.normalized * Mathf.Clamp(moveDirection.magnitude, 0, walkSpeed);
        }
        moveDirection.y = moveDirectionY;
    }
    
    private void HandleJump()
    {
        if(ShouldJump)
        {
            moveDirection.y = jumpForce;
        }
    }

    private void HandleCrouch()
    {
        if (ShouldCrouch)
        {
            StartCoroutine(CrouchStand()); // change between standing and crouching
        }
    }

    private void HandleHeadBob()
    {
        if (!characterController.isGrounded)
        {
            return;
        }

        if(Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.z) > 0.1f)
        {
            timer += Time.deltaTime * (isCrouching ? crouchBobSpeed : IsSprinting ? sprintBobSpeed : walkBobSpeed);
            playerCamera.transform.localPosition = new Vector3(
                playerCamera.transform.localPosition.x,
                defaultYPos + Mathf.Sin(timer) * (isCrouching ? crouchBobAmount : IsSprinting ? sprintBobAmount: walkBobAmount),
                playerCamera.transform.localPosition.z);
        }
    }

    // Mapping the mouse movement to player's rotation
    private void HandleMouseLook()
    {
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedX, 0);
    }

    private void ApplyFinalMovement()
    {
        //if(characterController.velocity.y < -1 && characterController.isGrounded)
        //{
        //    moveDirection.y = 0;
        //}

        if (!characterController.isGrounded) // Handling gravity
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        if (WillSlideOnSlopes && IsSliding) // Sliding on the slope
        {
            moveDirection += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slopeSpeed;
            //moveDirection += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * Mathf.Clamp(Vector3.Angle(hitPointNormal, Vector3.up) / characterController.slopeLimit, 1, 2) * slopeSpeed;
        }
        characterController.Move(moveDirection * Time.deltaTime);
    }

    // To transition between crouching nad standing
    private IEnumerator CrouchStand()
    {
        if (isCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up, 1f))
        {
            yield break;
        }
        duringCrouchAnmation = true;

        float timeElapsed = 0;
        float targetHeight = isCrouching ? standingHeight : crouchHeight;
        float currentHeight = characterController.height;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = characterController.center;
        
        // This is to ensure that the player is changing from crouching to standing smoother
        while(timeElapsed < timeToCrouch)
        {
            characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch);
            characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed/ timeToCrouch);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        characterController.height = targetHeight;
        characterController.center = targetCenter;

        isCrouching = !isCrouching;

        duringCrouchAnmation = false;
    }

    // Returns the position of the character
    private Vector3 GetCharacterPosition()
    {
        return characterController.transform.position;
    }

    // Checks for collisions with the player
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if(hit.gameObject.CompareTag("Enemy"))
        {
            
        }
    }
}


