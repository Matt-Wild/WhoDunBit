using UnityEngine;

namespace WhoDunBit.Player
{
    [DefaultExecutionOrder(128)]
    public class PlayerAnimation : PlayerBehaviour
    {
        [Header("Fetched")]
        [SerializeField] private Animator _animator;

        [Header("Positioning")]
        [SerializeField] private bool _compensateForSkinWidth = true;

        [Header("Falling")]
        [SerializeField, Min(0f)] private float _fallingDelay = 0.1f;

        // Animator parameter hashes
        private static readonly int ForwardAxisHash = Animator.StringToHash("ForwardAxis");
        private static readonly int RightAxisHash = Animator.StringToHash("RightAxis");
        private static readonly int JumpHash = Animator.StringToHash("Jump");
        private static readonly int MovingHash = Animator.StringToHash("Moving");
        private static readonly int HeavyLandingHash = Animator.StringToHash("HeavyLanding");
        private static readonly int GroundedHash = Animator.StringToHash("Grounded");

        private Vector2 _smoothedHorizontalVelocity;

        private float _lastGrounded;

        private bool _waitingForAirborne = false;

        private void Awake()
        {
            EnsureAnimator();
        }

        public override void Init(PlayerMaster master)
        {
            base.Init(master);

            // Subscribe to movement events
            if (Master && Master.Movement)
            {
                Master.Movement.Jumped += OnJumped;
                Master.Movement.Landed += OnLanded;
            }
            else
            {
                Debug.LogWarning("Could not subscribe to movement events - Master or Movement not found!", this);
            }

            CompensateForSkinWidth();
        }

        private void OnValidate()
        {
            EnsureAnimator();
        }

        private void OnDestroy()
        {
            if (!Master || !Master.Movement)
            {
                return;
            }

            Master.Movement.Jumped -= OnJumped;
            Master.Movement.Landed -= OnLanded;
        }

        private void Update()
        {
            UpdateAnimatorParams();
        }

        private void UpdateAnimatorParams()
        {
            UpdateLocomotionParams();
        }

        private void UpdateLocomotionParams()
        {
            // This requires the animator, player master, and network spawned player movement to be valid
            if (!_animator || !Master || !Master.Movement || !Master.Movement.IsSpawned)
            {
                return;
            }

            Vector3 localVelocity = Master.Movement.LocalVelocity;
            Vector2 horizontalVelocity = new(localVelocity.x, localVelocity.z);

            // Smooth the raw velocity before clamping
            float blend = 1f - Mathf.Exp(-Time.deltaTime / 0.1f);
            _smoothedHorizontalVelocity = Vector2.Lerp(_smoothedHorizontalVelocity, horizontalVelocity, blend);

            // Walk speed should never be 0 for animation purposes
            float walkSpeed = Mathf.Max(0.1f, Master.Movement.WalkSpeed);

            // Run speed should never be slower than walking
            float runSpeed = Mathf.Max(walkSpeed, Master.Movement.RunSpeed);

            // Sprint speed should never be slower than running
            float sprintSpeed = Mathf.Max(runSpeed, Master.Movement.SprintSpeed);

            // Get current speed
            float speed = _smoothedHorizontalVelocity.magnitude;

            float radius;
            if (speed <= walkSpeed)
            {
                // Stationary -> walking: radius 0 -> 1
                radius = speed / walkSpeed;
            }
            else if (speed <= runSpeed)
            {
                // Walking -> running: radius 1 -> 2
                radius = 1f + Mathf.InverseLerp(walkSpeed, runSpeed, speed);
            }
            else
            {
                // Running -> sprinting: radius 2 -> 3
                radius = 2f + Mathf.InverseLerp(runSpeed, sprintSpeed, speed);
            }

            Vector2 axes = _smoothedHorizontalVelocity.normalized * radius;

            // Treat very small movements as being still
            if (axes.sqrMagnitude < 0.0001f)
            {
                axes = Vector2.zero;
            }

            MovementState movementState = Master.Movement.MovementState;
            if (movementState.Grounded)
            {
                _lastGrounded = Time.time;
            }
            else
            {
                _waitingForAirborne = false;
            }

            // Determine if animation controller should treat player as grounded
            bool keepGrounded = _animator.GetBool(GroundedHash) && Time.time - _lastGrounded < _fallingDelay;
            bool grounded = !_waitingForAirborne && (movementState.Grounded || keepGrounded);

            // Update locomotion parameters
            _animator.SetFloat(RightAxisHash, axes.x);
            _animator.SetFloat(ForwardAxisHash, axes.y);
            _animator.SetBool(GroundedHash, grounded);
            _animator.SetBool(MovingHash, movementState.Moving);
            _animator.SetBool(HeavyLandingHash, movementState.HeavyLanding);
        }

        private void OnJumped()
        {
            // Fire trigger in animator and ensure not grounded
            if (_animator)
            {
                _animator.SetBool(GroundedHash, false);
                _animator.SetTrigger(JumpHash);

                // Ignore grounded signals until airborne detected
                _waitingForAirborne = true;
            }
        }

        private void OnLanded(float impactSpeed)
        {
            _waitingForAirborne = false;
        }

        private void CompensateForSkinWidth()
        {
            if (!_compensateForSkinWidth || !Master || !Master.Movement)
            {
                return;
            }

            // Move avatar position down to account for character controller skin width
            float skinWidth = Master.Movement.Controller.skinWidth;
            if (skinWidth > 0f)
            {
                transform.localPosition -= Vector3.up * skinWidth;
            }
        }

        private void EnsureAnimator()
        {
            // Fetch the animator if it's not already known
            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>();
            }

            // If we still don't have an animator, log a warning
            if (_animator == null)
            {
                Debug.LogWarning($"Could not find Animator!", this);
            }
        }
    }
}
