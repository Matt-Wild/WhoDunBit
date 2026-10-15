using Unity.Netcode;
using UnityEngine;

namespace WhoDunBit.Player
{
    public class Player : NetworkBehaviour
    {
        [Header("Components")]
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private AudioListener _audioListener;

        private void Awake()
        {
            SetLocallyControlled(false);
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
            _playerCamera.enabled = locallyControlled;
            _audioListener.enabled = locallyControlled;
        }
    }
}
