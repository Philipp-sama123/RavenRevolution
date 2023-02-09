using UnityEngine;

[RequireComponent(typeof( Animator ))]
public class AnimatorManager : MonoBehaviour {
    public Animator Animator { get; private set; }
    private RavenManager ravenManager;
    private LocomotionManager locomotionManager;

    private float rootMotionMultiplicator = 2f;
    private bool addRootMotionForce = true;

    private void Awake()
    {
        Animator = GetComponent<Animator>();
        ravenManager = GetComponent<RavenManager>();
        locomotionManager = GetComponent<LocomotionManager>();
    }

    public void HandleAnimatorValues(float horizontalMovement, float verticalMovement, bool isSprinting)
    {
        if ( isSprinting == false )
        {
            Animator.SetFloat("Horizontal", SnapValue(horizontalMovement), 0.1f, Time.deltaTime);
            Animator.SetFloat("Vertical", SnapValue(verticalMovement), 0.1f, Time.deltaTime);
        }
        else
        {
            Animator.SetFloat("Horizontal", SnapValue(horizontalMovement) * 2, 0.1f, Time.deltaTime);
            Animator.SetFloat("Vertical", SnapValue(verticalMovement) * 2, 0.1f, Time.deltaTime);
        }
    }

    public void PlayTargetAnimation(string targetAnimation, bool isUsingRootMotion, bool canRotate = false)
    {
        Animator.applyRootMotion = isUsingRootMotion;
        Animator.SetBool("CanRotate", canRotate); // maybe remove 
        Animator.SetBool("IsUsingRootMotion", isUsingRootMotion);
        Animator.CrossFade(targetAnimation, 0.2f);
    }

    public void HandleUpAndDown(float upDownInput)
    {
        Animator.SetFloat("UpDown", SnapValue(upDownInput), 0.1f, Time.deltaTime);
    }

    private void OnAnimatorMove()
    {
        if ( ravenManager.isUsingRootMotion == false )
            return;

        float delta = Time.deltaTime;
        locomotionManager.rigidbody.drag = 0;
        Vector3 deltaPosition = Animator.deltaPosition;
        if ( addRootMotionForce && deltaPosition.y > 0 )
        {
            deltaPosition.y *= rootMotionMultiplicator;
            Debug.Log("addRootMotionForce velocity" + deltaPosition);
        }
        Vector3 velocity = deltaPosition / delta;
        locomotionManager.rigidbody.velocity = velocity;
    }

    private static float SnapValue(float valueToSnap)
    {
        float snappedValue = 0;
        switch ( valueToSnap )
        {
            case > 0 and < 0.30f :
                snappedValue = 0.25f;
                break;
            case > 0.30f and < 0.55f :
                snappedValue = 0.5f;
                break;
            case > 0.55f and < 0.80f :
                snappedValue = 0.75f;
                break;
            case > 0.80f :
                snappedValue = 1f;
                break;
            case < 0 and > -0.30f :
                snappedValue = -0.25f;
                break;
            case < -0.30f and > -0.55f :
                snappedValue = -0.5f;
                break;
            case < -0.55f and > -0.80f :
                snappedValue = -0.75f;
                break;
            case < -0.80f :
                snappedValue = -1f;
                break;
            default :
                snappedValue = 0;
                break;
        }
        return snappedValue;
    }

}