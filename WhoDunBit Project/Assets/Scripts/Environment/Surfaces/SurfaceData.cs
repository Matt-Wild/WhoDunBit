using System;
using UnityEngine;

namespace WhoDunBit.Surfaces
{
    [CreateAssetMenu(fileName = "Surface Data", menuName = "WhoDunBit/Surface Data")]
    public class SurfaceData : ScriptableObject
    {
        [Header("Properties")]
        [SerializeField] private SurfaceID _id;

        [Header("Footsteps")]
        [SerializeField] private FootstepSettings _walking = new();
        [SerializeField] private FootstepSettings _running = new();
        [SerializeField] private FootstepSettings _sprinting = new();
        [SerializeField] private FootstepSettings _jumping = new();
        [SerializeField] private LandingSettings _landing = new();

        public SurfaceID ID => _id;

        public FootstepSettings Walking => _walking;
        public FootstepSettings Running => _running;
        public FootstepSettings Sprinting => _sprinting;
        public FootstepSettings Jumping => _jumping;
        public LandingSettings Landing => _landing;
    }

    [Serializable]
    public class FootstepSettings
    {
        [SerializeField] private AudioClip[] _clips = Array.Empty<AudioClip>();

        [Tooltip("Minimum and maximum playback volume, between 0 and 1.")]
        [SerializeField] private Vector2 _volumeRange = new(0.8f, 1f);

        [Tooltip("Minimum and maximum playback pitch. 1 is the original pitch.")]
        [SerializeField] private Vector2 _pitchRange = new(0.95f, 1.05f);

        [Tooltip("Maximum audible distance in metres when using linear 3D rolloff.")]
        [SerializeField, Min(0.1f)] private float _range = 3.6f;

        public AudioClip[] Clips => _clips;
        public Vector2 VolumeRange => _volumeRange;
        public Vector2 PitchRange => _pitchRange;
        public float Range => _range;
    }

    [Serializable]
    public class LandingSettings : FootstepSettings
    {
        [Tooltip("Audible distance for the gentlest landing. Range sets the distance for the hardest landing.")]
        [SerializeField, Min(0.1f)] private float _minimumRange = 2f;

        public float MinimumRange => _minimumRange;
    }
}
