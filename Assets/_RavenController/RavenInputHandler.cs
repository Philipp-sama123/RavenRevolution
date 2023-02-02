using UnityEngine;

namespace _RavenController {
    public class RavenInputHandler : MonoBehaviour
    {
   private PlayerControls playerControls;
    public float horizontalMovementInput { get; private set; }
    public float verticalMovementInput { get; private set; }
    public float horizontalCameraInput { get; private set; }
    public float verticalCameraInput { get; private set; }
    public float upDownInput { get; private set; }
    public float moveAmount { get; private set; }

    private Vector2 movementInput;
    private Vector2 cameraInput;
    private Vector2 airControlInput;


    [Header("Camera Rotation")]
    public bool sprintFlag;
    public bool jumpInput;
    public bool jumpHoldInput;
    public bool takeOffInput;

    private void OnEnable()
    {
        if ( playerControls == null )
        {
            playerControls = new PlayerControls();

            playerControls.PlayerMovement.Movement.performed += i => movementInput = i.ReadValue<Vector2>();
            playerControls.PlayerMovement.Camera.performed += i => cameraInput = i.ReadValue<Vector2>();
            playerControls.PlayerActions.AirControl.performed += i => airControlInput = i.ReadValue<Vector2>();
            playerControls.PlayerActions.Jump.performed += i => jumpInput = true;
            playerControls.PlayerActions.TakeOff.performed += i => takeOffInput = true;
            playerControls.PlayerActions.JumpHold.performed += i => jumpHoldInput = true;
            // playerControls.PlayerActions.Jump.canceled += i => jumpInput = false;
            playerControls.PlayerActions.Roll.performed += i => sprintFlag = true;
            playerControls.PlayerActions.Roll.canceled += i => sprintFlag = false;
        }
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    public void HandleAllInputs()
    {
        HandleMovementInput();
        HandleCameraInput();
        HandleAirControlInput();
    }

    private void HandleMovementInput()
    {
        horizontalMovementInput = movementInput.x;
        verticalMovementInput = movementInput.y;
        moveAmount = Mathf.Clamp01(Mathf.Abs(horizontalMovementInput) + Mathf.Abs(verticalMovementInput));
    }

    private void HandleCameraInput()
    {
        horizontalCameraInput = cameraInput.x;
        verticalCameraInput = cameraInput.y;
    }

    private void HandleAirControlInput()
    {
        upDownInput = airControlInput.y;
    }
    }
}
