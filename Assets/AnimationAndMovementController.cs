using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationAndMovementController : MonoBehaviour
{
    PlayerInput playerInput;
    CharacterController characterController;

    Animator animator;

    int isWalkingHash;
    int isRunningHash;

    Vector2 currentMovementInput;
    Vector3 currentMovement;

    Vector3 currentRunMovement;

    bool isMovementPressed;
    bool isRunPressed;
    float rotationFactorPerFrame = 15f;

    float runMultiplier = 5.0f;

 // gravity Variables

    float gravity = -9.81f;
    float groundedGravity = -0.5f;

 // jump variables

    bool isJumpPressed = false;
    float initialJumpVelocity;
    float maxJumpHeight = 4.0f;
    float maxJumpTime = 0.5f;
    bool isJumping;


    void Awake()
    {
        playerInput = new PlayerInput();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");

        playerInput.CharacterControlls.Move.started += onMovementInput;
        playerInput.CharacterControlls.Move.canceled += onMovementInput;
        playerInput.CharacterControlls.Move.performed += onMovementInput;

        playerInput.CharacterControlls.Run.started += onRun;
        playerInput.CharacterControlls.Run.canceled += onRun;

        playerInput.CharacterControlls.Jump.started += onJump;
        playerInput.CharacterControlls.Jump.canceled += onJump;
        setupJumpVariables();

    }

    void setupJumpVariables() 
    {
        gravity = (-2 * maxJumpHeight) / Mathf.Pow(maxJumpTime / 2, 2);
        initialJumpVelocity = (2 * maxJumpHeight) / (maxJumpTime / 2);
        Debug.Log(initialJumpVelocity);
    }

    void handleJump()
    {
        
        if (!isJumping && characterController.isGrounded && isJumpPressed) {
            isJumping = true;
            currentMovement.y = initialJumpVelocity * .5f;
            currentRunMovement.y = initialJumpVelocity * .5f;

            Debug.Log(currentMovement);

        } else if (!isJumpPressed && isJumping && characterController.isGrounded) {
            isJumping = false;
        }
    }

    void onJump(InputAction.CallbackContext context)
    {
        isJumpPressed = context.ReadValueAsButton();
        

    }

    void onRun(InputAction.CallbackContext context)
    {
        isRunPressed = context.ReadValueAsButton();
    }

    void handleRotation()
    {
        Vector3 positionToLookAt;

        positionToLookAt.x = currentMovement.x;
        positionToLookAt.y = 0.0f;
        positionToLookAt.z = currentMovement.z;


        Quaternion currentRotation = transform.rotation;

        if (isMovementPressed)
        {
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPerFrame * Time.deltaTime);
        }



    }

    void onMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
        currentMovement.x = currentMovementInput.x;
        currentMovement.z = currentMovementInput.y;

        currentRunMovement.x = currentMovementInput.x * runMultiplier;
        currentRunMovement.z = currentMovementInput.y * runMultiplier;

        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
    }


    void Start()
    {

    }

    void handleAnimation()
    {
        bool isWalking = animator.GetBool(isWalkingHash);
        bool isRunning = animator.GetBool(isRunningHash);

        if (isMovementPressed && !isWalking)
        {
            animator.SetBool(isWalkingHash, true);
        }
        else if (!isMovementPressed && isWalking)
        {
            animator.SetBool(isWalkingHash, false);
        }

        if ((isMovementPressed && isRunPressed) && !isRunning){
            animator.SetBool(isRunningHash, true);
        }
        else if ((!isMovementPressed || !isRunPressed) && isRunning) {
            animator.SetBool(isRunningHash, false);
        }
    }

    void handleGravity() {
        if (characterController.isGrounded) {

            currentMovement.y = groundedGravity;
            currentRunMovement.y = groundedGravity;
        } else {

            float previousYVelocity = currentMovement.y;
            float newYVelocity = currentMovement.y + (gravity * Time.deltaTime);
            float nextYVelocity = (previousYVelocity + newYVelocity) * .5f;

            currentMovement.y = nextYVelocity;
            currentRunMovement.y = nextYVelocity;
        }
    }

    void Update()
    {
        
        handleRotation();
        handleAnimation();

        if (isRunPressed)
        {
            characterController.Move(currentRunMovement * Time.deltaTime);
        }
        else
        {
            characterController.Move(currentMovement * Time.deltaTime);
        }

        handleGravity();
        handleJump();

    }

    void OnEnable()
    {
        playerInput.CharacterControlls.Enable();
    }

    void OnDisable()
    {
        playerInput.CharacterControlls.Disable();
    }

}
