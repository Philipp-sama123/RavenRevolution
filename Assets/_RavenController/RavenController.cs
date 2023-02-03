using System;
using UnityEngine;

namespace _RavenController {
    public class RavenController : MonoBehaviour {
        private RavenInputHandler ravenInputHandler;
        private LocomotionManager locomotionManagerManager;
        private RavenAnimatorHandler ravenAnimatorHandler;

        public new Rigidbody rigidbody;
        public Transform cameraObject;
        [Header("Movement Stats")][SerializeField]
        private float movementSpeed = 2.5f;
        private Vector3 moveDirection;

        [SerializeField] private float sprintSpeed = 1.5f;
        [SerializeField] private float walkingSpeed = 1f;
        [SerializeField] private float rotationSpeed = 2.5f;

        [Header("Ground and Air Detection Stats")][SerializeField]
        private float groundDetectionRayStartPoint = .2f;

        [SerializeField] private float minimumDistanceNeededToBeginFall = .25f;
        [SerializeField] private float groundDirectionRayDistance = 0.1f;
        [SerializeField] private LayerMask groundLayer;

        private float inAirTimer;

        // ToDo: maybe local for Falling
        private Vector3 normalVector;
        private Vector3 targetPosition;

        public bool isSprinting;
        public bool isGrounded;
        private CameraManager cameraManager;

        private static readonly int State = Animator.StringToHash("State");
        private static readonly int LastState = Animator.StringToHash("LastState");
        private static readonly int Grounded = Animator.StringToHash("Grounded");
        public int ActiveState { get; private set; }
        public int ActiveLastState { get; private set; }

        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
            ravenInputHandler = GetComponent<RavenInputHandler>();
            ravenAnimatorHandler = GetComponent<RavenAnimatorHandler>();
            cameraManager = FindObjectOfType<CameraManager>();
        }

        private void Update()
        {
            ravenInputHandler.HandleAllInputs();
            ActiveState = ravenAnimatorHandler.Animator.GetInteger(State);
            ActiveLastState = ravenAnimatorHandler.Animator.GetInteger(LastState);
            ActiveLastState = ravenAnimatorHandler.Animator.GetInteger(LastState);
            ravenAnimatorHandler.Animator.SetBool(Grounded, isGrounded);
        }

        private void FixedUpdate()
        {
            HandleMovement();
            GroundCheck();
            HandleJumping();
            HandleTakeOff();
        }

        private void LateUpdate()
        {
            cameraManager.FollowTarget();
            cameraManager.RotateCamera(ravenInputHandler.HorizontalCameraInput, ravenInputHandler.VerticalCameraInput);
        }

        private void HandleJumping()
        {
            if ( ravenInputHandler.jumpInput )
            {
                ravenInputHandler.jumpInput = false;

                ravenAnimatorHandler.Animator.SetInteger(State, ravenAnimatorHandler.Jump);

            }
        }

        private void HandleTakeOff()
        {
            if ( ravenInputHandler.takeOffInput )
            {
                ravenInputHandler.takeOffInput = false;
                if ( ravenAnimatorHandler.Animator.GetInteger(State) != ravenAnimatorHandler.Fly )
                {
                    ravenAnimatorHandler.Animator.SetInteger(LastState, ravenAnimatorHandler.Locomotion);
                    ravenAnimatorHandler.Animator.SetInteger(State, ravenAnimatorHandler.Fly);
                }
                else
                {
                    if ( !isGrounded )
                    {
                        ravenAnimatorHandler.Animator.SetInteger(State, ravenAnimatorHandler.Fall);
                        ravenAnimatorHandler.Animator.SetInteger(LastState, ravenAnimatorHandler.Fall);

                    }
                    else
                    {
                        ravenAnimatorHandler.Animator.SetInteger(State, ravenAnimatorHandler.Locomotion);

                    }

                }

            }
        }

        private void HandleMovement()
        {
            // If is Jumping - return
            if ( ravenAnimatorHandler.Animator.GetInteger(State) == ravenAnimatorHandler.Jump ) return;

            if ( isGrounded )
            {
                rigidbody.AddForce(transform.forward* ravenInputHandler.VerticalMovementInput * (isSprinting ? 20 : 10));
                rigidbody.AddForce(transform.up* ravenInputHandler.VerticalMovementInput);
                ravenAnimatorHandler.HandleAnimatorValues(
                    ravenInputHandler.HorizontalMovementInput,
                    ravenInputHandler.VerticalMovementInput,
                    ravenInputHandler.UpDownInput,
                    ravenInputHandler.sprintFlag
                );
            }
            else
            {
                // not flying -- > so falling
                if ( ravenAnimatorHandler.Animator.GetInteger(State) != ravenAnimatorHandler.Fly )
                {
                    rigidbody.useGravity = true;
                    ravenAnimatorHandler.Animator.applyRootMotion = true;

                    ravenAnimatorHandler.Animator.SetInteger(State, ravenAnimatorHandler.Fall);
                    ravenAnimatorHandler.Animator.SetInteger(LastState, ravenAnimatorHandler.Fall);
                }
                else
                {
                    // flying state
                    rigidbody.useGravity = false;
                    ravenAnimatorHandler.Animator.applyRootMotion = false; // todo find a better way to do this (!)


                    ravenAnimatorHandler.HandleAnimatorValues(
                        ravenInputHandler.HorizontalMovementInput,
                        ravenInputHandler.VerticalMovementInput,
                        ravenInputHandler.UpDownInput,
                        ravenInputHandler.sprintFlag
                    );
                    HandleFlyingRotation();
                    HandleFlyingMovement();

                }
            }
        }

        private void HandleFlyingRotation()
        {
            Vector3 targetDir = Vector3.zero;
            targetDir = cameraObject.forward * Math.Abs(ravenInputHandler.VerticalMovementInput); // For not rotating weird back! just for flying
            targetDir += cameraObject.right * ravenInputHandler.HorizontalMovementInput;
            targetDir.Normalize();
            targetDir.y = 0;

            if ( targetDir == Vector3.zero )
                targetDir = transform.forward;

            float rs = rotationSpeed;
            Quaternion tr = Quaternion.LookRotation(targetDir);
            Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, rs * Time.deltaTime);

            rigidbody.rotation = targetRotation;
        }

        private void HandleFlyingMovement()
        {
            // ravenAnimatorHandler.Animator.applyRootMotion = false;
            // FLYING MOVEMENT
            moveDirection = cameraObject.forward * ravenInputHandler.VerticalMovementInput;
            moveDirection += cameraObject.right * ravenInputHandler.HorizontalMovementInput;
            // moveDirection += cameraObject.up * ravenInputHandler.upDownInput * 10f;
            moveDirection.y = 0;
            moveDirection.Normalize();

            float speed = movementSpeed;

            if ( ravenInputHandler.sprintFlag && ravenInputHandler.MoveAmount > 0.75f )
            {
                speed = sprintSpeed;
                isSprinting = true;
                moveDirection *= speed;
            }
            else
            {
                moveDirection *= speed;
            }
            Debug.Log("VELOCITY: before - " + rigidbody.velocity);

            // moveDirection *= 2; // --> Flying multiplicator

            Vector3 projectedVelocity = Vector3.ProjectOnPlane(moveDirection, normalVector);
            rigidbody.velocity = Vector3.Lerp(rigidbody.velocity, projectedVelocity, .2f / Time.deltaTime); // TODO: Lerp or not lerp? 
            // rigidbody.AddForce(projectedVelocity*10);
            Vector3 upDownForce = Vector3.up * ravenInputHandler.UpDownInput * 25f;
            rigidbody.AddForce(upDownForce);
        }

        private void GroundCheck()
        {
            isGrounded = false;

            if ( ravenAnimatorHandler.Animator.GetInteger(State) != ravenAnimatorHandler.Fly &&
                 ravenAnimatorHandler.Animator.GetInteger(State) != ravenAnimatorHandler.Jump )
            {
                RaycastHit hit;
                Vector3 origin = transform.position;
                origin.y += groundDetectionRayStartPoint;

                Vector3 dir = Vector3.zero;
                dir.Normalize();
                origin = origin + dir * groundDirectionRayDistance;

                targetPosition = transform.position;

                Debug.DrawRay(origin, -Vector3.up * minimumDistanceNeededToBeginFall, Color.red);
                if ( Physics.Raycast(origin, -Vector3.up, out hit, minimumDistanceNeededToBeginFall, groundLayer) )
                {
                    normalVector = hit.normal;
                    Vector3 tp = hit.point;
                    targetPosition.y = tp.y;
                    isGrounded = true;

                    ravenAnimatorHandler.Animator.SetInteger(LastState, ravenAnimatorHandler.Fall);
                    ravenAnimatorHandler.Animator.SetInteger(State, ravenAnimatorHandler.Locomotion);

                    Debug.Log("[Info] Grounded" + targetPosition);

                    // transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime / .2f);

                }
                else
                {
                    Debug.Log("[Info] Not Grounded" + targetPosition);
                }
            }
        }

    }
}