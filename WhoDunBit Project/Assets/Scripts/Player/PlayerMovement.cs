using System.Collections.Generic;
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

        [Header("Network Interpolation")]
        [SerializeField, Min(0f)] private float _interpolationDelay = 0.1f;

        private Vector3 _previousPosition;
        private Vector3 _worldVelocity;
        private Vector3 _horizontalVelocity;
        private Vector3 _remoteLocalVelocity;
        private float _verticalSpeed;
        private float _cameraPitch;

        public CharacterController Controller => _controller;

        public event System.Action Jumped;
        public event System.Action<float> Landed;

        // Movement state should be synced across clients
        private NetworkVariable<MovementState> _networkMovement = new(new MovementState { SampleTime = -1d }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private readonly List<MovementState> _receivedMovementStates = new();

        private SupportInfo _supportInfo = new();

        public MovementState MovementState => _networkMovement.Value;
        public Vector3 WorldVelocity => Master.IsLocallyControlled ? _controller.velocity : _worldVelocity;
        public Vector3 LocalVelocity => Master.IsLocallyControlled ? transform.InverseTransformDirection(_controller.velocity) : _remoteLocalVelocity;
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
            _remoteLocalVelocity = Vector3.zero;
            _supportInfo = new();

            _receivedMovementStates.Clear();

            if (!IsOwner)
            {
                _networkMovement.OnValueChanged += OnMovementStateChanged;

                // Include the state received during spawning
                OnMovementStateChanged(default, _networkMovement.Value);
                return;
            }

            SetActionsEnabled(true);

            CursorUtility.SetLocked(true);
        }

        public override void OnNetworkDespawn()
        {
            _networkMovement.OnValueChanged -= OnMovementStateChanged;
            _receivedMovementStates.Clear();

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
            else
            {
                UpdateRemoteMovement();
            }

            UpdatePositionAndVelocity();
        }

        private void UpdatePositionAndVelocity()
        {
            Vector3 currentPosition = transform.position;
            _worldVelocity = Time.deltaTime > 0f ? (currentPosition - _previousPosition) / Time.deltaTime : Vector3.zero;
            _previousPosition = currentPosition;
        }

        private void UpdateRemoteMovement()
        {
            if (!TryGetInterpolatedMovement(out MovementState movementState))
            {
                return;
            }

            Vector3 position = movementState.Position;
            Quaternion rotation = movementState.Rotation;

            if (movementState.HasNetworkedSupport)
            {
                // Wait until the referenced support is available locally
                if (!movementState.SupportReference.TryGet(out NetworkObject supportObject, NetworkManager))
                {
                    return;
                }

                Transform support = supportObject.transform;
                position = support.TransformPoint(position);
                rotation = support.rotation * rotation;
            }

            // Keep the player upright as the support tilts
            Vector3 forward = Vector3.ProjectOnPlane(rotation * Vector3.forward, Vector3.up);

            if (forward.sqrMagnitude > 0.0001f)
            {
                rotation = Quaternion.LookRotation(forward, Vector3.up);
            }
            else
            {
                rotation = transform.rotation;
            }

            transform.SetPositionAndRotation(position, rotation);

            _remoteLocalVelocity = movementState.LocalVelocity;
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
            if (!_supportInfo.Surface)
            {
                return false;
            }

            Vector3 feet = _controller.transform.TransformPoint(_controller.center - Vector3.up * (_controller.height * 0.5f));
            float radius = _controller.radius + _controller.skinWidth;

            foreach (Collider collider in Physics.OverlapSphere(feet, radius, ~_ignoredSupportLayers.value, QueryTriggerInteraction.Ignore))
            {
                if (collider.transform == _supportInfo.Surface)
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
            // Ensure colliders etc are synced with their transforms (helps with high speed moving objects)
            Physics.SyncTransforms();

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
            bool wasSupported = wasGrounded && _supportInfo.Surface;
            float control = wasGrounded ? _groundControl : hasMoveInput ? _airControl : _airDrag;
            float acceleration = _acceleration * speed * control;
            Transform startingSupport = wasSupported ? _supportInfo.Surface : null;

            if (wasGrounded && _supportInfo.Surface)
            {
                // Move with support surface
                Quaternion rotationDelta = _supportInfo.Surface.rotation * Quaternion.Inverse(_supportInfo.PreviousRotation);
                Vector3 displacement = GetSupportDisplacement();
                _supportInfo.Velocity = Time.deltaTime > 0f ? displacement / Time.deltaTime : Vector3.zero;

                // Account for inherited velocity upon landing on a new support surface
                if (_supportInfo.NeedsVelocityConversion)
                {
                    _horizontalVelocity -= new Vector3(_supportInfo.Velocity.x, 0f, _supportInfo.Velocity.z);
                    _supportInfo.NeedsVelocityConversion = false;
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
            if (_supportInfo.Surface && (jumped || !_controller.isGrounded || !IsSupportNearby()))
            {
                _supportInfo.Surface = null;
                _supportInfo.NetworkObject = null;

                _supportInfo.Point = Vector3.zero;
                _supportInfo.PreviousPosition = Vector3.zero;
                _supportInfo.PreviousRotation = Quaternion.identity;
            }

            if (_supportInfo.Surface)
            {
                _supportInfo.PreviousPosition = _supportInfo.Surface.position;
                _supportInfo.PreviousRotation = _supportInfo.Surface.rotation;
            }

            // If leaving a support surface, account for it's velocity
            bool supportChanged = startingSupport != _supportInfo.Surface;
            Vector3 outgoingSupportVelocity = wasSupported ? new Vector3(_supportInfo.Velocity.x, 0f, _supportInfo.Velocity.z) : Vector3.zero;
            Vector3 departureVelocity = supportChanged ? outgoingSupportVelocity : Vector3.zero;
            if (supportChanged)
            {
                _supportInfo.NeedsVelocityConversion = _supportInfo.Surface != null;
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

            // Record network identity after support assignment and clearing
            _supportInfo.IsNetworked = _supportInfo.Surface && _supportInfo.NetworkObject && _supportInfo.NetworkObject.IsSpawned;
            _supportInfo.Reference = _supportInfo.IsNetworked ? new NetworkObjectReference(_supportInfo.NetworkObject) : default;

            // Record player pose relative to the current networked support
            if (_supportInfo.IsNetworked)
            {
                _supportInfo.RelativePosition = _supportInfo.Surface.InverseTransformPoint(transform.position);
                _supportInfo.RelativeRotation = Quaternion.Inverse(_supportInfo.Surface.rotation) * transform.rotation;
            }
            else
            {
                _supportInfo.RelativePosition = Vector3.zero;
                _supportInfo.RelativeRotation = Quaternion.identity;
            }

            // Record player pose and its coordinate space together
            movementState.HasNetworkedSupport = _supportInfo.IsNetworked;
            movementState.SupportReference = _supportInfo.Reference;
            movementState.Position = _supportInfo.IsNetworked ? _supportInfo.RelativePosition : transform.position;
            movementState.Rotation = _supportInfo.IsNetworked ? _supportInfo.RelativeRotation : transform.rotation;

            // Record movement velocity relative to player facing
            movementState.LocalVelocity = transform.InverseTransformDirection(_controller.velocity);

            // Record sample time for interpolation
            movementState.SampleTime = NetworkManager.ServerTime.Time;

            // Assign new movement state (this gets synced)
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
                if (_supportInfo.Surface != hit.transform)
                {
                    _supportInfo.Surface = hit.transform;
                    _supportInfo.Surface.TryGetComponent(out _supportInfo.NetworkObject);

                    _supportInfo.PreviousPosition = hit.transform.position;
                    _supportInfo.PreviousRotation = hit.transform.rotation;
                }
                
                _supportInfo.Point = hit.transform.InverseTransformPoint(hit.point);
            }

            Debug.DrawRay(hit.point, hit.normal * 0.4f, walkable ? Color.green : Color.red, 0.1f);
        }

        private void OnMovementStateChanged(MovementState previous, MovementState current)
        {
            // Ignore state that hasn't had a pose captured yet
            if (current.SampleTime < 0d)
            {
                return;
            }

            // Ignore duplicate or older samples
            if (_receivedMovementStates.Count > 0 && current.SampleTime <= _receivedMovementStates[^1].SampleTime)
            {
                return;
            }

            _receivedMovementStates.Add(current);

            // Keep a short history for interpolation
            if (_receivedMovementStates.Count > 32)
            {
                _receivedMovementStates.RemoveAt(0);
            }
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


            Vector3 currentForward = transform.forward;
            float yaw = Mathf.Atan2(Vector3.Cross(currentForward, forward).y, Vector3.Dot(currentForward, forward)) * Mathf.Rad2Deg;

            Quaternion yawRotation = Quaternion.AngleAxis(yaw, Vector3.up);

            transform.rotation = yawRotation * transform.rotation;
            _horizontalVelocity = yawRotation * _horizontalVelocity;
        }

        private bool TryGetInterpolatedMovement(out MovementState movementState)
        {
            movementState = default;

            if (_receivedMovementStates.Count == 0)
            {
                return false;
            }

            double renderTime = NetworkManager.ServerTime.Time - _interpolationDelay;
            movementState = _receivedMovementStates[0];

            // Hold the first pose until render time reaches it
            if (renderTime <= movementState.SampleTime)
            {
                return true;
            }

            for (int i = 1; i < _receivedMovementStates.Count; i++)
            {
                MovementState next = _receivedMovementStates[i];

                if (next.SampleTime <= renderTime)
                {
                    movementState = next;
                    continue;
                }

                // Positions can only be blended within the same coordinate space
                bool sameSpace = movementState.HasNetworkedSupport == next.HasNetworkedSupport && (!movementState.HasNetworkedSupport || movementState.SupportReference.Equals(next.SupportReference));

                if (sameSpace)
                {
                    float blend = (float)((renderTime - movementState.SampleTime) / (next.SampleTime - movementState.SampleTime));

                    movementState.Position = Vector3.Lerp(movementState.Position, next.Position, blend);
                    movementState.Rotation = Quaternion.Slerp(movementState.Rotation, next.Rotation, blend);

                    movementState.LocalVelocity = Vector3.Lerp(movementState.LocalVelocity, next.LocalVelocity, blend);

                    movementState.SampleTime = renderTime;
                }

                return true;
            }

            // Hold the latest pose if no newer sample is available
            return true;
        }

        private Vector3 GetSupportDisplacement()
        {
            if (!_supportInfo.Surface)
            {
                return Vector3.zero;
            }

            Quaternion rotationDelta = _supportInfo.Surface.rotation * Quaternion.Inverse(_supportInfo.PreviousRotation);

            Vector3 previousOffset = transform.position - _supportInfo.PreviousPosition;
            Vector3 carriedPosition = _supportInfo.Surface.position + rotationDelta * previousOffset;

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

    public class SupportInfo
    {
        public Transform Surface;
        public NetworkObject NetworkObject;
        public NetworkObjectReference Reference;
        public Quaternion RelativeRotation = Quaternion.identity;
        public Quaternion PreviousRotation = Quaternion.identity;
        public Vector3 Point;
        public Vector3 RelativePosition;
        public Vector3 Velocity;
        public Vector3 PreviousPosition;
        public bool NeedsVelocityConversion;
        public bool IsNetworked;
    }

    public struct MovementState : INetworkSerializable
    {
        public bool Grounded;
        public bool HeavyLanding;
        public bool Moving;

        // Position and rotation based information
        public bool HasNetworkedSupport;
        public NetworkObjectReference SupportReference;
        public Vector3 Position;
        public Quaternion Rotation;

        // Movement velocity relative to player facing
        public Vector3 LocalVelocity;

        public double SampleTime;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
        {
            serializer.SerializeValue(ref Grounded);
            serializer.SerializeValue(ref HeavyLanding);
            serializer.SerializeValue(ref Moving);

            serializer.SerializeValue(ref HasNetworkedSupport);
            serializer.SerializeValue(ref SupportReference);
            serializer.SerializeValue(ref Position);
            serializer.SerializeValue(ref Rotation);

            serializer.SerializeValue(ref LocalVelocity);

            serializer.SerializeValue(ref SampleTime);
        }
    }
}
