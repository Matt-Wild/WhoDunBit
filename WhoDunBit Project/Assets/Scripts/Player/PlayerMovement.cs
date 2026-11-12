using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using WhoDunBit.Utilities;

namespace WhoDunBit.Player
{
    [DefaultExecutionOrder(-128)]
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : PlayerNetworkBehaviour
    {
        [Header("Components")]
        [SerializeField] private CharacterController _controller;

        [Header("Looking")]
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private float _mouseSensitivity = 0.1f;

        [Header("Horizontal Movement")]
        [SerializeField, Range(0f, 1f)] private float _groundControl = 1f;
        [SerializeField, Range(0f, 1f)] private float _airControl = 0.6f;
        [SerializeField, Range(0f, 1f)] private float _airDrag = 0.1f;
        [SerializeField, Min(0f)] private float _acceleration = 1f;
        [SerializeField, Min(0f)] private float _walkSpeed = 2f;
        [SerializeField, Min(0f)] private float _runSpeed = 4f;
        [SerializeField, Min(0f)] private float _sprintSpeed = 6f;

        [Header("Vertical Movement")]
        [SerializeField, Min(0f)] private float _groundStickSpeed = 2f;
        [SerializeField] private float _gravity = -20f;
        [SerializeField, Min(0f)] private float _jumpStrength = 4f;
        [SerializeField, Min(0f)] private float _heavyLandingMinSpeed = 10f;

        [Header("Input")]
        [SerializeField] private InputActionReference _moveAction;
        [SerializeField] private InputActionReference _walkAction;
        [SerializeField] private InputActionReference _sprintAction;
        [SerializeField] private InputActionReference _jumpAction;
        [SerializeField] private InputActionReference _lookAction;
        [SerializeField, Min(0f)] private float _moveInputDeadzone = 0.05f;

        private Vector3 _previousPosition;
        private Vector3 _worldVelocity;
        private Vector3 _horizontalVelocity;
        private float _verticalSpeed;
        private float _cameraPitch;

        public CharacterController Controller => _controller;

        public event System.Action Jumped;
        public event System.Action<float> Landed;

        // Variables that need to be synced across clients
        private NetworkVariable<MovementState> _networkMovement = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private Transform _supportSurface;
        private Vector3 _supportPoint;

        public MovementState MovementState => _networkMovement.Value;
        public Vector3 WorldVelocity => Master.IsLocallyControlled ? _controller.velocity : _worldVelocity;
        public Vector3 LocalVelocity => transform.InverseTransformDirection(WorldVelocity);
        public float WalkSpeed => _walkSpeed;
        public float RunSpeed => _runSpeed;
        public float SprintSpeed => _sprintSpeed;
        public float VerticalSpeed => Master.IsLocallyControlled ? _verticalSpeed : _worldVelocity.y;
        public bool IsGrounded => Master.IsLocallyControlled ? _controller.isGrounded : _networkMovement.Value.Grounded;

        private void Awake()
        {
            EnsureCharacterController();
        }

        private void OnValidate()
        {
            EnsureCharacterController();
        }

        public override void OnNetworkSpawn()
        {
            _previousPosition = transform.position;
            _worldVelocity = Vector3.zero;
            _horizontalVelocity = Vector3.zero;

            if (!IsOwner)
            {
                return;
            }

            SetActionsEnabled(true);

            CursorUtility.SetLocked(true);
        }

        public override void OnNetworkDespawn()
        {
            if (!IsOwner)
            {
                return;
            }

            SetActionsEnabled(false);

            CursorUtility.SetLocked(false);
        }

        private void Update()
        {
            if (!IsSpawned)
            {
                return;
            }

            if (IsOwner)
            {
                Vector2 movementInput = Vector2.zero;
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    movementInput = _moveAction.action.ReadValue<Vector2>();
                    Vector2 lookInput = _lookAction.action.ReadValue<Vector2>();

                    Look(lookInput);
                }

                // Movement should happen even if the cursor is unlocked
                Move(movementInput);
            }

            UpdatePositionAndVelocity();
        }

        private void UpdatePositionAndVelocity()
        {
            Vector3 currentPosition = transform.position;
            _worldVelocity = Time.deltaTime > 0f ? (currentPosition - _previousPosition) / Time.deltaTime : Vector3.zero;
            _previousPosition = currentPosition;
        }

        private void EnsureCharacterController()
        {
            // Fetch character controller if not known
            if (!_controller)
            {
                _controller = GetComponent<CharacterController>();
            }

            // Warn is character controller not found
            if (!_controller)
            {
                Debug.LogWarning($"Could not find CharacterController!", this);
            }
        }

        private void Look(Vector2 mouseDelta)
        {
            float yaw = mouseDelta.x * _mouseSensitivity;
            transform.Rotate(Vector3.up, yaw);

            _cameraPitch -= mouseDelta.y * _mouseSensitivity;
            _cameraPitch = Mathf.Clamp(_cameraPitch, -85f, 85f);

            _cameraTransform.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
        }

        private void Move(Vector2 input)
        {
            // Prevent diagonal movement from being faster
            input = Vector2.ClampMagnitude(input, 1f);
            bool hasMoveInput = input.sqrMagnitude > _moveInputDeadzone * _moveInputDeadzone;
            if (!hasMoveInput)
            {
                input = Vector2.zero;
            }

            // Can't sprint backwards and run by default
            float speed = _sprintAction.action.IsPressed() && input.y >= 0f ? _sprintSpeed : _walkAction.action.IsPressed() ? _walkSpeed : _runSpeed;

            bool wasGrounded = IsGrounded;
            float control = wasGrounded ? _groundControl : hasMoveInput ? _airControl : _airDrag;
            float acceleration = _acceleration * speed * control;

            Vector3 currentVelocity = _horizontalVelocity;

            // Calculate target velocity
            Vector3 targetVelocity = (transform.right * input.x + transform.forward * input.y) * speed;
            if (hasMoveInput && !wasGrounded)
            {
                Vector3 direction = targetVelocity.normalized;
                float speedAlongInput = Vector3.Dot(currentVelocity, direction);
                targetVelocity = direction * Mathf.Max(targetVelocity.magnitude, speedAlongInput);
            }

            // Control is the fraction of the difference closed in _controlInterval seconds
            Vector3 velocity = Vector3.MoveTowards(currentVelocity, targetVelocity, acceleration * Time.deltaTime);

            // Jumping and landing logic
            bool jumped = false;
            if (wasGrounded && _verticalSpeed < 0f)
            {
                if (_jumpAction.action.WasPressedThisFrame())
                {
                    _verticalSpeed = _jumpStrength;
                    jumped = true;
                }

                // Make the player stick to the ground a bit if not jumping
                else
                {
                    _verticalSpeed = -_groundStickSpeed;
                }
            }

            _verticalSpeed += _gravity * Time.deltaTime;
            velocity.y = _verticalSpeed;

            float impactSpeed = Mathf.Max(0f, -_verticalSpeed);

            CollisionFlags collisions = _controller.Move(velocity * Time.deltaTime);
            if ((collisions & CollisionFlags.Above) != 0 && _verticalSpeed > 0f)
            {
                _verticalSpeed = 0f;
            }

            Vector3 requestedHorizontal = new(velocity.x, 0f, velocity.z);
            Vector3 actualHorizontal = _controller.velocity;
            actualHorizontal.y = 0f;

            // Collisions may remove momentum, but cannot add speed
            _horizontalVelocity = Vector3.ClampMagnitude(actualHorizontal, requestedHorizontal.magnitude);

            MovementState movementState = _networkMovement.Value;
            movementState.Grounded = _controller.isGrounded;

            Vector3 velocityAfterMove = _controller.velocity;
            float horizontalSpeed = new Vector2(velocityAfterMove.x, velocityAfterMove.z).magnitude;
            movementState.Moving = hasMoveInput && horizontalSpeed > (movementState.Moving ? 0.5f : 0.75f);  // Velocity difference avoids flickery movement state

            // Fire movement events (these should be sent to other clients too)
            if (jumped)
            {
                NotifyJumpedRpc();
            }
            else if (!wasGrounded && IsGrounded)
            {
                movementState.HeavyLanding = impactSpeed >= _heavyLandingMinSpeed;

                NotifyLandedRpc(impactSpeed);
            }

            _networkMovement.Value = movementState;
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            bool walkable = angle <= _controller.slopeLimit;

            if (walkable)
            {
                _supportSurface = hit.transform;
                _supportPoint = hit.transform.InverseTransformPoint(hit.point);
            }

            Debug.DrawRay(hit.point, hit.normal * 0.4f, walkable ? Color.green : Color.red, 0.1f);
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Owner)]
        private void NotifyJumpedRpc()
        {
            Jumped?.Invoke();
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Owner)]
        private void NotifyLandedRpc(float impactSpeed)
        {
            Landed?.Invoke(impactSpeed);
        }

        private void SetActionsEnabled(bool enabled)
        {
            if (enabled)
            {
                _moveAction.action.Enable();
                _walkAction.action.Enable();
                _sprintAction.action.Enable();
                _jumpAction.action.Enable();
                _lookAction.action.Enable();
            }
            else
            {
                _moveAction.action.Disable();
                _walkAction.action.Disable();
                _sprintAction.action.Disable();
                _jumpAction.action.Disable();
                _lookAction.action.Disable();
            }
        }
    }

    public struct MovementState : INetworkSerializable
    {
        public bool Grounded;
        public bool HeavyLanding;
        public bool Moving;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
        {
            serializer.SerializeValue(ref Grounded);
            serializer.SerializeValue(ref HeavyLanding);
            serializer.SerializeValue(ref Moving);
        }
    }
}
