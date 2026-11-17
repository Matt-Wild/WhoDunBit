using UnityEngine;

namespace WhoDunBit.Player
{
    public class PlayerAnimationEvents : MonoBehaviour
    {
        private PlayerAudio _audio;

        private void Awake()
        {
            // Find the audio behaviour on the owning player
            _audio = GetComponentInParent<PlayerAudio>();
        }

        public void OnFootstep(AnimationEvent footstep)
        {
            if (_audio && _audio.isActiveAndEnabled)
            {
                _audio.OnFootstep(footstep);
            }
        }
    }
}