using UnityEngine;

namespace WhoDunBit.Player
{
    public class PlayerAnimation : PlayerBehaviour
    {
        [Header("Fetched")]
        [SerializeField] private Animator _animator;

        [Header("Positioning")]
        [SerializeField] private bool _compensateForSkinWidth = true;

        // Animator parameter hashes
        private static readonly int ForwardAxisHash = Animator.StringToHash("ForwardAxis");
        private static readonly int RightAxisHash = Animator.StringToHash("RightAxis");

        private Vector2 _smoothedHorizontalVelocity;

        private void Awake()
        {
            EnsureAnimator();
        }

        public override void Init(PlayerMaster master)
        {
            base.Init(master);

            CompensateForSkinWidth();
        }

        private void OnValidate()
        {
            EnsureAnimator();
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
            // This requires the animator, player master, and player movement to be valid
            if (!_animator || !Master || !Master.Movement)
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

            // Calculate horizontal axes in relation to walk speed
            Vector2 axes = Vector2.ClampMagnitude(_smoothedHorizontalVelocity / walkSpeed, 1f);

            // Treat very small movements as being still
            if (axes.sqrMagnitude < 0.0001f)
            {
                axes = Vector2.zero;
            }

            // Update locomotion parameters
            _animator.SetFloat(RightAxisHash, axes.x);
            _animator.SetFloat(ForwardAxisHash, axes.y);
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
