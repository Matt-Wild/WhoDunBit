using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using WhoDunBit.Utilities;

namespace WhoDunBit.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : NetworkBehaviour
    {
        [Header("Looking")]
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private float _mouseSensitivity = 0.1f;

        [Header("Horizontal Movement")]
        [SerializeField] private float _moveSpeed = 5f;

        [Header("Vertical Movement")]
        [SerializeField, Min(0f)] private float _groundStickSpeed = 2f;
        [SerializeField] private float _gravity = -20f;

        [Header("Input")]
        [SerializeField] private InputActionReference _moveAction;
        [SerializeField] private InputActionReference _lookAction;

        private CharacterController _controller;
        private float _verticalSpeed;
        private float _cameraPitch;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                return;
            }

            _moveAction.action.Enable();
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
            _lookAction.action.Disable();

            CursorUtility.SetLocked(false);
        }

        private void Update()
        {
            // Each machine only controls its own player
            if (!IsSpawned || !IsOwner)
            {
                return;
            }

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

            Vector3 velocity = (transform.right * input.x + transform.forward * input.y) * _moveSpeed;

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