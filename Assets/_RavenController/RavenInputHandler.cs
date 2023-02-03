using UnityEngine;

namespace _RavenController {
    public class RavenInputHandler : MonoBehaviour
    {
   private PlayerControls playerControls;
    public float HorizontalMovementInput { get; private set; }
    public float VerticalMovementInput { get; private set; }
    public float HorizontalCameraInput { get; private set; }
    public float VerticalCameraInput { get; private set; }
    public float UpDownInput { get; private set; }
    public float MoveAmount { get; private set; }

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
        HorizontalMovementInput = movementInput.x;
        VerticalMovementInput = movementInput.y;
        MoveAmount = Mathf.Clamp01(Mathf.Abs(HorizontalMovementInput) + Mathf.Abs(VerticalMovementInput));
    }

    private void HandleCameraInput()
    {
        HorizontalCameraInput = cameraInput.x;
        VerticalCameraInput = cameraInput.y;
    }

    private void HandleAirControlInput()
    {
        UpDownInput = airControlInput.y;
    }
    }
}
