using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using WhoDunBit.Utilities;

namespace WhoDunBit.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : PlayerNetworkBehaviour
    {
        [Header("Components")]
        [SerializeField] private CharacterController _controller;

        [Header("Looking")]
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private float _mouseSensitivity = 0.1f;

        [Header("Horizontal Movement")]
        [SerializeField] private float _walkSpeed = 2f;
        [SerializeField] private float _runSpeed = 4f;
        [SerializeField] private float _sprintSpeed = 6f;

        [Header("Vertical Movement")]
        [SerializeField, Min(0f)] private float _groundStickSpeed = 2f;
        [SerializeField] private float _gravity = -20f;

        [Header("Input")]
        [SerializeField] private InputActionReference _moveAction;
        [SerializeField] private InputActionReference _walkAction;
        [SerializeField] private InputActionReference _sprintAction;
        [SerializeField] private InputActionReference _lookAction;

        private Vector3 _previousPosition;
        private Vector3 _worldVelocity;
        private float _verticalSpeed;
        private float _cameraPitch;

        public CharacterController Controller => _controller;

        public Vector3 WorldVelocity => Master.IsLocallyControlled ? _controller.velocity : _worldVelocity;
        public Vector3 LocalVelocity => transform.InverseTransformDirection(WorldVelocity);
        public float WalkSpeed => _walkSpeed;
        public float RunSpeed => _runSpeed;
        public float SprintSpeed => _sprintSpeed;

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

            if (!IsOwner)
            {
                return;
            }

            _moveAction.action.Enable();
            _walkAction.action.Enable();
            _sprintAction.action.Enable();
            _lookAction.action.Enable();

            CursorUtility.SetLocked(true);
        }

        public override void OnNetworkDespawn()
        {
            if (!IsOwner)
            {
                return;
            }

            _moveAction.action.Disable();
            _walkAction.action.Disable();
            _sprintAction.action.Disable();
            _lookAction.action.Disable();

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

            // Can't sprint backwards and run by default
            float speed = _sprintAction.action.IsPressed() && input.y >= 0f ? _sprintSpeed : _walkAction.action.IsPressed() ? _walkSpeed : _runSpeed;

            Vector3 velocity = (transform.right * input.x + transform.forward * input.y) * speed;

            // Sticks the player to the ground while grounded
            if (_controller.isGrounded && _verticalSpeed < 0f)
            {
                _verticalSpeed = -_groundStickSpeed;
            }

            _verticalSpeed += _gravity * Time.deltaTime;
            velocity.y = _verticalSpeed;

            _controller.Move(velocity * Time.deltaTime);
        }
    }
}