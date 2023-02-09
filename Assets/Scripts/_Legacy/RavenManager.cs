using UnityEngine;

public class RavenManager : MonoBehaviour {

    private CameraManager cameraManager;
    private InputManager inputManager;
    private LocomotionManager locomotionManagerManager;
    private AnimatorManager animatorManager;

    public bool isGrounded;
    public bool isInAir;
    public bool isSprinting;
    public bool isUsingRootMotion;
    public bool isFlying;

    private void Awake()
    {
        cameraManager = FindObjectOfType<CameraManager>();
        inputManager = GetComponent<InputManager>();
        locomotionManagerManager = GetComponent<LocomotionManager>();
        animatorManager = GetComponent<AnimatorManager>();
    }

    private void Update()
    {
        inputManager.HandleAllInputs();
        UpdateStateBoolValuesFromAnimator();
    }

    private void FixedUpdate()
    {
        var deltaTime = Time.deltaTime;

        if ( isFlying )
        {
            locomotionManagerManager.HandleFlyingMovement(deltaTime);
        }
        else
        {
            locomotionManagerManager.HandleGroundMovement(deltaTime);
        }
    }

    private void LateUpdate()
    {
        cameraManager.FollowTarget();
        cameraManager.RotateCamera(inputManager.horizontalCameraInput, inputManager.verticalCameraInput);
    }

    private void UpdateStateBoolValuesFromAnimator()
    {
        animatorManager.Animator.SetBool("Grounded", isGrounded);
        // animatorManager.Animator.SetBool("IsInAir", isInAir);
        isFlying = animatorManager.Animator.GetBool("IsFlying");
        isUsingRootMotion = animatorManager.Animator.GetBool("IsUsingRootMotion");
    }

}