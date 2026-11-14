using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using WhoDunBit.Utilities;

namespace WhoDunBit.Player
{
    [DefaultExecutionOrder(64)]
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

        [Header("Support Surface Detection")]
        [SerializeField] private LayerMask _ignoredSupportLayers;

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
        private Vector3 _supportVelocity;
        private Vector3 _previousSupportPosition;
        private Quaternion _previousSupportRotation;
        private bool _needsSupportVelocityConversion;

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
            _needsSupportVelocityConversion = false;

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

        private bool IsSupportNearby()
        {
            if (!_supportSurface)
            {
                return false;
            }

            Vector3 feet = _controller.transform.TransformPoint(_controller.center - Vector3.up * (_controller.height * 0.5f));
            float radius = _controller.radius + _controller.skinWidth;

            foreach (Collider collider in Physics.OverlapSphere(feet, radius, ~_ignoredSupportLayers.value, QueryTriggerInteraction.Ignore))
            {
                if (collider.transform == _supportSurface)
                {
                    return true;
                }
            }

            return false;
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
            bool wasSupported = wasGrounded && _supportSurface;
            float control = wasGrounded ? _groundControl : hasMoveInput ? _airControl : _airDrag;
            float acceleration = _acceleration * speed * control;
            Transform startingSupport = wasSupported ? _supportSurface : null;

            if (wasGrounded && _supportSurface)
            {
                // Move with support surface
                Quaternion rotationDelta = _supportSurface.rotation * Quaternion.Inverse(_previousSupportRotation);
                Vector3 displacement = GetSupportDisplacement();
                _supportVelocity = Time.deltaTime > 0f ? displacement / Time.deltaTime : Vector3.zero;

                // Account for inherited velocity upon landing on a new support surface
                if (_needsSupportVelocityConversion)
                {
                    _horizontalVelocity -= new Vector3(_supportVelocity.x, 0f, _supportVelocity.z);
                    _needsSupportVelocityConversion = false;
                }

                ApplySupportRotation(rotationDelta);
                _controller.Move(displacement);
            }

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

            // Clear support surface if it's controller isn't grounded or support surface is too far away
            if (_supportSurface && (jumped || !_controller.isGrounded || !IsSupportNearby()))
            {
                _supportSurface = null;
                _supportPoint = Vector3.zero;
                _previousSupportPosition = Vector3.zero;
                _previousSupportRotation = Quaternion.identity;
            }

            if (_supportSurface)
            {
                _previousSupportPosition = _supportSurface.position;
                _previousSupportRotation = _supportSurface.rotation;
            }

            // If leaving a support surface, account for it's velocity
            bool supportChanged = startingSupport != _supportSurface;
            Vector3 outgoingSupportVelocity = wasSupported ? new Vector3(_supportVelocity.x, 0f, _supportVelocity.z) : Vector3.zero;
            Vector3 departureVelocity = supportChanged ? outgoingSupportVelocity : Vector3.zero;
            if (supportChanged)
            {
                _needsSupportVelocityConversion = _supportSurface != null;
            }

            Vector3 requestedHorizontal = new(velocity.x, 0f, velocity.z);
            Vector3 actualHorizontal = _controller.velocity;
            actualHorizontal.y = 0f;

            // Collisions may remove momentum, but cannot add speed
            _horizontalVelocity = Vector3.ClampMagnitude(actualHorizontal, requestedHorizontal.magnitude);
            _horizontalVelocity += departureVelocity;   // Velocity maintained after leaving a support surface

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
            if ((_ignoredSupportLayers.value & (1 << hit.collider.gameObject.layer)) != 0)
            {
                return;
            }

            float angle = Vector3.Angle(hit.normal, Vector3.up);
            bool walkable = angle <= _controller.slopeLimit;

            if (walkable)
            {
                if (_supportSurface != hit.transform)
                {
                    _supportSurface = hit.transform;
                    _previousSupportPosition = hit.transform.position;
                    _previousSupportRotation = hit.transform.rotation;
                }
                
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

        private void ApplySupportRotation(Quaternion rotationDelta)
        {
            Vector3 forward = Vector3.ProjectOnPlane(rotationDelta * transform.forward, Vector3.up);

            if (forward.sqrMagnitude < 0.0001f)
            {
                return;
            }
                

            float yaw = Vector3.SignedAngle(transform.forward, forward, Vector3.up);

            Quaternion yawRotation = Quaternion.AngleAxis(yaw, Vector3.up);

            transform.rotation = yawRotation * transform.rotation;
            _horizontalVelocity = yawRotation * _horizontalVelocity;
        }

        private Vector3 GetSupportDisplacement()
        {
            if (!_supportSurface)
            {
                return Vector3.zero;
            }

            Quaternion rotationDelta = _supportSurface.rotation * Quaternion.Inverse(_previousSupportRotation);

            Vector3 previousOffset = transform.position - _previousSupportPosition;
            Vector3 carriedPosition = _supportSurface.position + rotationDelta * previousOffset;

            return carriedPosition - transform.position;
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
