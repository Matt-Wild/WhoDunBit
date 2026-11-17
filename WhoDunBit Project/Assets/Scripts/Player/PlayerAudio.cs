using System.Collections.Generic;
using UnityEngine;
using WhoDunBit.Surfaces;
using SurfaceLookup = WhoDunBit.Surfaces.Surfaces;

namespace WhoDunBit.Player
{
    [DefaultExecutionOrder(128)]
    public class PlayerAudio : PlayerBehaviour
    {
        [Header("Components")]
        [SerializeField] private Animator _animator;
        [SerializeField] private AudioSource[] _audioSources = new AudioSource[16];

        [Header("Footsteps")]
        [SerializeField, Min(0f)] private float _sameFootCooldown = 0.12f;

        [Header("Landing")]
        [Tooltip("Downward impact speed in metres per second that produces maximum landing volume and range.")]
        [SerializeField, Min(0.1f)] private float _maxLandingSpeed = 16f;

        private readonly Transform[] _footBones = new Transform[2];
        private readonly FootstepSettings[] _pendingFootsteps = new FootstepSettings[2];
        private readonly List<AnimatorClipInfo> _footstepClips = new();
        private readonly float[] _lastFootstepTimes = new float[2];

        private ulong[] _audioPlayOrder;
        private ulong _nextAudioPlayOrder;

        private MovementState _previousState;
        private bool _hasPreviousState;

        public override void Init(PlayerMaster master)
        {
            base.Init(master);

            if (!_animator || !_animator.isHuman)
            {
                return;
            }

            _footBones[0] = _animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            _footBones[1] = _animator.GetBoneTransform(HumanBodyBones.RightFoot);
        }

        private void OnValidate()
        {
            if (!_animator)
            {
                _animator = GetComponentInChildren<Animator>();
            }
        }

        private void OnEnable()
        {
            _hasPreviousState = false;

            _lastFootstepTimes[0] = float.NegativeInfinity;
            _lastFootstepTimes[1] = float.NegativeInfinity;

            _pendingFootsteps[0] = null;
            _pendingFootsteps[1] = null;
        }

        private void Update()
        {
            if (!Master || !Master.Movement || !Master.Movement.IsSpawned || !Master.Movement.TryGetAnimationState(out MovementState state))
            {
                _hasPreviousState = false;
                return;
            }

            // Remember the first state without playing spawn sounds
            if (!_hasPreviousState)
            {
                _previousState = state;
                _hasPreviousState = true;
                return;
            }

            bool jumped = state.JumpSequence != _previousState.JumpSequence && !state.Grounded;
            bool landed = !_previousState.Grounded && state.Grounded;
            _previousState = state;

            if (!jumped && !landed)
            {
                return;
            }

            SurfaceData data = SurfaceLookup.GetData(state.SurfaceID);
            if (!data)
            {
                return;
            }

            if (jumped)
            {
                PlaySound(Master.Movement.transform.position, data.Jumping);
            }
            else if (landed)
            {
                LandingSettings settings = data.Landing;

                float intensity = Mathf.InverseLerp(0f, _maxLandingSpeed, state.LandingSpeed);
                float volume = Mathf.Lerp(settings.VolumeRange.x, settings.VolumeRange.y, intensity);
                float range = Mathf.Lerp(settings.MinimumRange, settings.Range, intensity);

                PlaySound(Master.Movement.transform.position, settings, volume, range);
            }
        }

        private void LateUpdate()
        {
            for (int foot = 0; foot < _pendingFootsteps.Length; foot++)
            {
                FootstepSettings settings = _pendingFootsteps[foot];
                _pendingFootsteps[foot] = null;

                if (settings == null || !_footBones[foot])
                {
                    continue;
                }

                PlaySound(_footBones[foot].position, settings);
            }
        }

        private void PlaySound(Vector3 position, FootstepSettings settings, float? volume = null, float? range = null)
        {
            if (settings == null || settings.Clips == null || settings.Clips.Length == 0)
            {
                return;
            }

            AudioClip clip = settings.Clips[Random.Range(0, settings.Clips.Length)];
            if (!clip)
            {
                return;
            }

            AudioSource source = GetAudioSource();
            if (!source)
            {
                return;
            }

            // Apply the selected surface's playback settings
            source.transform.position = position;
            source.clip = clip;
            source.volume = volume ?? Random.Range(settings.VolumeRange.x, settings.VolumeRange.y);
            source.pitch = Random.Range(settings.PitchRange.x, settings.PitchRange.y);

            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 0.1f;
            source.maxDistance = range ?? settings.Range;
            source.dopplerLevel = 0f;
            source.loop = false;

            source.Play();
        }

        public void OnFootstep(AnimationEvent footstep)
        {
            if (!_animator || !Master || !Master.Movement || !Master.Movement.IsSpawned || !Master.Movement.TryGetAnimationState(out MovementState state))
            {
                return;
            }

            if (!state.Grounded || !state.Moving)
            {
                return;
            }

            int foot = footstep.intParameter;
            if (foot < 0 || foot > 1)
            {
                return;
            }

            // Find the strongest clip in the current blend
            _animator.GetCurrentAnimatorClipInfo(0, _footstepClips);

            if (_footstepClips.Count == 0)
            {
                return;
            }

            AnimatorClipInfo strongest = _footstepClips[0];

            for (int i = 1; i < _footstepClips.Count; i++)
            {
                if (_footstepClips[i].weight > strongest.weight)
                {
                    strongest = _footstepClips[i];
                }
            }

            if (footstep.animatorClipInfo.clip != strongest.clip)
            {
                return;
            }

            // Prevent duplicate contacts when the strongest clip changes
            if (Time.time - _lastFootstepTimes[foot] < _sameFootCooldown)
            {
                return;
            }

            _lastFootstepTimes[foot] = Time.time;

            SurfaceData data = SurfaceLookup.GetData(state.SurfaceID);
            if (!data)
            {
                return;
            }

            float speed = new Vector2(state.LocalVelocity.x, state.LocalVelocity.z).magnitude;

            // Choose the nearest movement speed
            float runningThreshold = (Master.Movement.WalkSpeed + Master.Movement.RunSpeed) * 0.5f;
            float sprintingThreshold = (Master.Movement.RunSpeed + Master.Movement.SprintSpeed) * 0.5f;

            FootstepSettings settings = data.Walking;

            if (speed >= sprintingThreshold)
            {
                settings = data.Sprinting;
            }
            else if (speed >= runningThreshold)
            {
                settings = data.Running;
            }

            _pendingFootsteps[foot] = settings;
        }

        private AudioSource GetAudioSource()
        {
            // Rebuild playback order if the pool has changed
            if (_audioPlayOrder == null || _audioPlayOrder.Length != _audioSources.Length)
            {
                _audioPlayOrder = new ulong[_audioSources.Length];
                _nextAudioPlayOrder = 0;
            }

            // Use an idle source, or replace the oldest sound when the pool is full
            int selected = -1;

            for (int i = 0; i < _audioSources.Length; i++)
            {
                AudioSource source = _audioSources[i];
                if (!source)
                {
                    continue;
                }

                if (!source.isPlaying)
                {
                    selected = i;
                    break;
                }

                if (selected < 0 || _audioPlayOrder[i] < _audioPlayOrder[selected])
                {
                    selected = i;
                }
            }

            if (selected < 0)
            {
                return null;
            }

            _audioPlayOrder[selected] = ++_nextAudioPlayOrder;
            return _audioSources[selected];
        }
    }
}
