using Unity.Netcode;
using UnityEngine;

namespace WhoDunBit.Player
{
    public class PlayerMaster : PlayerNetworkBehaviour
    {
        [Header("Behaviours")]
        [SerializeField] private PlayerMovement _movement;
        [SerializeField] private PlayerAnimation _animation;
        [SerializeField] private PlayerCamera _camera;

        // Fetched player behaviours
        private PlayerAvatar _avatar;

        [Header("Components")]
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private AudioListener _audioListener;

        private bool _isLocallyControlled = false;

        // Behaviour accessors
        public PlayerMovement Movement => _movement;
        public PlayerAnimation Animation => _animation;
        public PlayerCamera Camera => _camera;
        public PlayerAvatar Avatar => _avatar;

        public bool IsLocallyControlled => _isLocallyControlled;

        private void Awake()
        {
            InitialiseBehaviours();

            SetLocallyControlled(false);
        }

        private void InitialiseBehaviours()
        {
            // Attempt to find player avatar
            if (!_avatar)
            {
                _avatar = GetComponentInChildren<PlayerAvatar>();
            }

            // Set as own master
            Init(this);

            // Initialise player behaviours and assign this as master
            _movement.Init(this);
            _animation.Init(this);
            _camera.Init(this);

            // Avatar is fetched so guard against null reference
            if (_avatar)
            {
                _avatar.Init(this);
            }
            else
            {
                Debug.LogWarning("Player avatar not found!", this);
            }
        }

        public override void OnNetworkSpawn()
        {
            SetLocallyControlled(IsOwner);
        }

        public override void OnNetworkDespawn()
        {
            SetLocallyControlled(false);
        }

        private void SetLocallyControlled(bool locallyControlled)
        {
            _isLocallyControlled = locallyControlled;

            _playerCamera.enabled = locallyControlled;
            _audioListener.enabled = locallyControlled;

            if (_avatar)
            {
                _avatar.UpdateRenderers();
            }
        }
    }
}
