using UnityEngine;

public class LocomotionManager : MonoBehaviour {
    private Transform cameraObject;

    private AnimatorManager animatorManager;
    private InputManager inputManager;
    private RavenManager ravenManager;

    public Transform myTransform;
    public new Rigidbody rigidbody;

    [Header("Ground and Air Detection Stats")][SerializeField]
    private float groundDetectionRayStartPoint = .2f;

    [SerializeField] private float minimumDistanceNeededToBeginFall = .25f;
    [SerializeField] private float groundDirectionRayDistance = 0.1f;

    [SerializeField] private LayerMask groundLayer;

    [Header("Movement Stats")][SerializeField]
    private float movementSpeed = .5f;
    private Vector3 moveDirection;

    [SerializeField] private float sprintSpeed = 1.5f;
    [SerializeField] private float walkingSpeed = 1f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float fallingSpeed = 10f;
    [SerializeField] private float leapingVelocity = 2f;
    [SerializeField] private float upDownForce = 2f;

    private float inAirTimer;

    // ToDo: maybe local for Falling
    private Vector3 normalVector;
    private Vector3 targetPosition;
    private static readonly int State = Animator.StringToHash("State");

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        inputManager = GetComponent<InputManager>();
        ravenManager = GetComponent<RavenManager>();

        animatorManager = GetComponentInChildren<AnimatorManager>();
    }

    private void Start()
    {
        if ( Camera.main != null ) cameraObject = Camera.main.transform;
        else Debug.LogWarning("No Main Camera in the Scene!");

        myTransform = transform;

        ravenManager.isGrounded = true;
    }

    public void HandleGroundMovement(float deltaTime)
    {
        if ( ravenManager.isGrounded )
            HandleMovement();
        HandleRotation(deltaTime);
        HandleFalling(deltaTime, moveDirection);

        HandleJumping();
        ToggleFlying();
    }

    public void HandleFlyingMovement(float deltaTime)
    {
        // ToDo: make depending on updown 
        // Glide--> -y slightly but more x
        // usal flying slightly up 
        // ground check?? 
        // ToDo: down movement rotate player down! 
        HandleMovement();
        HandleRotation(deltaTime);
        HandleFlyingGroundCheck();

        ToggleFlying();
        HandleAirControl();
    }

    private void HandleFlyingGroundCheck()
    {
        return;
        ravenManager.isGrounded = false;

        // TODO --> just play landing 

    }

    private void HandleAirControl()
    {
        Vector3 airForce = cameraObject.forward * inputManager.verticalMovementInput;
        airForce += cameraObject.right * inputManager.horizontalMovementInput;
        // go up
        airForce += cameraObject.up * inputManager.upDownInput * upDownForce;

        Vector3 projectedVelocity = Vector3.Project(airForce, normalVector);
        if ( inputManager.sprintFlag )
            projectedVelocity *= sprintSpeed;
        rigidbody.velocity += projectedVelocity;

        if ( !ravenManager.isUsingRootMotion )
            animatorManager.HandleUpAndDown(inputManager.upDownInput);
    }

    private void HandleRotation(float deltaTime)
    {
        Vector3 targetDir = Vector3.zero;
        targetDir = cameraObject.forward * inputManager.verticalMovementInput;
        targetDir += cameraObject.right * inputManager.horizontalMovementInput;
        targetDir.Normalize();
        targetDir.y = 0;

        if ( targetDir == Vector3.zero )
            targetDir = myTransform.forward;

        float rs = rotationSpeed;

        Quaternion tr = Quaternion.LookRotation(targetDir);
        Quaternion targetRotation = Quaternion.Slerp(myTransform.rotation, tr, rs * deltaTime);

        myTransform.rotation = targetRotation;
        //ToDo: rigidbody.rotation = targetRotation;
    }

    private void HandleMovement()
    {
        moveDirection = cameraObject.forward * inputManager.verticalMovementInput;
        moveDirection += cameraObject.right * inputManager.horizontalMovementInput;
        moveDirection.y = 0;
        moveDirection.Normalize();

        float speed = movementSpeed;

        if ( inputManager.sprintFlag && inputManager.moveAmount > 0.75f )
        {
            speed = sprintSpeed;
            ravenManager.isSprinting = true;
            moveDirection *= speed;
        }
        else
        {
            if ( inputManager.moveAmount < 0.75f )
            {
                moveDirection *= walkingSpeed;
                ravenManager.isSprinting = false;
            }
            else
            {
                moveDirection *= speed;
                ravenManager.isSprinting = false;
            }
        }

        Vector3 projectedVelocity = Vector3.ProjectOnPlane(moveDirection, normalVector);
        rigidbody.velocity = projectedVelocity;

        if ( !ravenManager.isUsingRootMotion )
            animatorManager.HandleAnimatorValues(0, inputManager.moveAmount, ravenManager.isSprinting);
    }

    private void HandleFalling(float deltaTime, Vector3 movementDirection)
    {
        ravenManager.isGrounded = false;

        RaycastHit hit;
        Vector3 origin = myTransform.position;
        origin.y += groundDetectionRayStartPoint;

        if ( Physics.Raycast(origin, myTransform.forward, out hit, 0.4f) )
        {
            movementDirection = Vector3.zero;
        }

        if ( ravenManager.isInAir )
        {
            Debug.Log("Raven Manager is in air" + rigidbody.velocity);
            inAirTimer++;
            rigidbody.AddForce(transform.forward * leapingVelocity, ForceMode.Acceleration);
            rigidbody.AddForce(Vector3.down * fallingSpeed * 9.8f * inAirTimer * deltaTime, ForceMode.Acceleration);
        }

        Vector3 dir = movementDirection;
        dir.Normalize();
        origin = origin + dir * groundDirectionRayDistance;

        targetPosition = myTransform.position;

        Debug.DrawRay(origin, -Vector3.up * minimumDistanceNeededToBeginFall, Color.red);
        if ( Physics.Raycast(origin, -Vector3.up, out hit, minimumDistanceNeededToBeginFall, groundLayer) )
        {
            normalVector = hit.normal;
            Vector3 tp = hit.point;
            ravenManager.isGrounded = true;
            targetPosition.y = tp.y;

            if ( ravenManager.isInAir )
            {
                if ( inAirTimer > 25f )
                {
                    animatorManager.PlayTargetAnimation("Land", false);
                    inAirTimer = 0;
                }
                else
                {
                    inAirTimer = 0;
                }

                ravenManager.isInAir = false;
            }
        }
        else
        {
            if ( ravenManager.isGrounded )
            {
                ravenManager.isGrounded = false;
            }

            if ( ravenManager.isInAir == false )
            {
                if ( ravenManager.isUsingRootMotion == false )
                {
                    animatorManager.Animator.SetInteger(State, 3);
                }

                Vector3 vel = rigidbody.velocity;
                vel.Normalize();
                rigidbody.velocity = vel * (movementSpeed / 2);
                Debug.LogWarning("Falling velocity applied! TODO REPLACE WITH ADDFORCE" + rigidbody.velocity);
                ravenManager.isInAir = true;
            }
        }

        if ( ravenManager.isGrounded )
        {
            if ( inputManager.moveAmount > 0 )
            {
                myTransform.position = Vector3.Lerp(myTransform.position, targetPosition, deltaTime / .2f);
            }
            else
            {
                myTransform.position = Vector3.Lerp(myTransform.position, targetPosition, deltaTime / .2f);
            }
        }
    }

    private void HandleJumping()
    {
        if ( inputManager.jumpInput )
        {
            inputManager.jumpInput = false;

            if ( ravenManager.isFlying == false && ravenManager.isGrounded )
            {
                animatorManager.Animator.SetInteger("State", 2);
                animatorManager.Animator.SetBool("IsUsingRootMotion", true);
            }
        }
    }

    private void ToggleFlying()
    {
        if ( inputManager.takeOffInput )
        {
            inputManager.takeOffInput = false;

            if ( ravenManager.isFlying == false )
            {
                animatorManager.Animator.SetBool("IsFlying", true);
                if ( ravenManager.isGrounded )
                {
                    animatorManager.Animator.SetInteger("State", 6);
                    animatorManager.Animator.SetBool("IsUsingRootMotion", true);
                    ravenManager.isGrounded = false;
                }

            }
            else
            {
                animatorManager.Animator.SetBool("IsFlying", false);
                inAirTimer = 0;
            }
        }
    }


}