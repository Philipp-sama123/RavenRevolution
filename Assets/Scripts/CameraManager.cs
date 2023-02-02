using UnityEngine;

public class CameraManager : MonoBehaviour {
    public Transform cameraPivotTransform;
    public Transform cameraRotationTransform;
    public Transform targetTransform;

    private float lookAngle;
    private float pivotAngle;

    public float lookSpeed = 0.05f;
    public float followSpeed = 0.1f;
    public float pivotSpeed = 0.03f;

    public float minimumPivot = -45f;
    public float maximumPivot = 45f;


    Vector3 cameraFollowVelocity = Vector3.zero;
    Vector3 targetPosition;
    Vector3 cameraRotation;
    Quaternion targetRotation;

    float lookAmountVertical;
    float lookAmountHorizontal;

    public void RotateCamera(float mouseXInput, float mouseYInput)
    {
        lookAngle += (mouseXInput * lookSpeed);
        pivotAngle -= (mouseYInput * pivotSpeed);
        pivotAngle = Mathf.Clamp(pivotAngle, minimumPivot, maximumPivot);

        Vector3 rotation = Vector3.zero;
        rotation.y = lookAngle;
        Quaternion targetRotation = Quaternion.Euler(rotation);
        cameraRotationTransform.rotation = targetRotation;

        rotation = Vector3.zero;
        rotation.x = pivotAngle;
        
        targetRotation = Quaternion.Euler(rotation);
        cameraPivotTransform.localRotation = targetRotation;
    }


    public void FollowTarget()
    {
        Vector3 targetPositionTemp = Vector3.SmoothDamp(
            transform.position,
            targetTransform.position,
            ref cameraFollowVelocity,
            followSpeed
        );
        transform.position = targetPositionTemp;

        //   HandleCameraCollisions();
    }
}