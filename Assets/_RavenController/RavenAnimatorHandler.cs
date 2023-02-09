using UnityEngine;

namespace _RavenController {
    public class RavenAnimatorHandler : MonoBehaviour {
        public Animator Animator { get; private set; }
        public RavenController ravenController;

        public readonly int Jump = 2;
        public readonly int Locomotion = 1;
        public readonly int Fall = 3;
        public readonly int Fly = 6;
        public readonly int Idle = 0;
        private static readonly int Horizontal = Animator.StringToHash("Horizontal");
        private static readonly int Vertical = Animator.StringToHash("Vertical");
        private static readonly int UpDown = Animator.StringToHash("UpDown");

        [SerializeField] private float velocityMultiplicator = 1.5f;

        private void Awake()
        {
            Animator = GetComponent<Animator>();
            ravenController = GetComponent<RavenController>();
        }

        public void HandleAnimatorValues(float horizontalMovement, float verticalMovement, float upDownMovement, bool isSprinting)
        {
            if ( isSprinting == false )
            {
                Animator.SetFloat(Horizontal, SnapValue(horizontalMovement), 0.1f, Time.deltaTime);
                Animator.SetFloat(Vertical, SnapValue(verticalMovement), 0.1f, Time.deltaTime);
                Animator.SetFloat(UpDown, SnapValue(upDownMovement), 0.1f, Time.deltaTime);
            }
            else
            {
                Animator.SetFloat(Horizontal, SnapValue(horizontalMovement) * 2, 0.1f, Time.deltaTime);
                Animator.SetFloat(Vertical, SnapValue(verticalMovement) * 2, 0.1f, Time.deltaTime);
                Animator.SetFloat(UpDown, SnapValue(upDownMovement), 0.1f, Time.deltaTime);
            }
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

        private void OnAnimatorMove()
        {
            if ( ravenController.isUsingRootMotion == false)
                return;

            // Animator.ApplyBuiltinRootMotion();

            float delta = Time.deltaTime;
            ravenController.rigidbody.drag = 0;
            Vector3 deltaPosition = Animator.deltaPosition;
            Quaternion deltaRotation = Animator.deltaRotation;
            
            Vector3 velocity = deltaPosition * velocityMultiplicator / delta;
            Quaternion rotation = ravenController.rigidbody.rotation * deltaRotation;

            ravenController.rigidbody.velocity = velocity;
            ravenController.rigidbody.rotation = rotation;
        }

    }
}