using Unity.Netcode;
using UnityEngine;

namespace WhoDunBit
{
    public class SimpleRotatingProp : NetworkBehaviour
    {
        [Header("Rotation")]
        [SerializeField] private Vector3 _direction;
        [SerializeField] private float _speed;

        private struct MotionState : INetworkSerializeByMemcpy
        {
            public double StartTime;
            public Quaternion StartRotation;
            public float Speed;
            public Vector3 Direction;
        }
        private NetworkVariable<MotionState> _motion = new(new MotionState { StartTime = -1d, StartRotation = Quaternion.identity });

        public override void OnNetworkSpawn()
        {
            if (!IsServer)
            {
                return;
            }

            _motion.Value = new MotionState
            {
                StartTime = NetworkManager.ServerTime.Time,
                StartRotation = transform.localRotation,
                Speed = _speed,
                Direction = _direction
            };
        }

        private void Update()
        {
            if (!IsSpawned || _motion.Value.StartTime < 0d)
            {
                return;
            } 

#if UNITY_EDITOR
            // Allows changing speed and direction using the inspector at runtime
            if (IsServer && (_speed != _motion.Value.Speed || _direction != _motion.Value.Direction))
            {
                SetMotion(_speed, _direction);
            }
#endif

            transform.localRotation = RotationAt(_motion.Value, NetworkManager.ServerTime.Time);
        }

        private static Quaternion RotationAt(MotionState motion, double now)
        {
            if (motion.Direction.sqrMagnitude < 0.0001f)
            {
                return motion.StartRotation;
            }

            double elapsed = System.Math.Max(0d, now - motion.StartTime);
            float angle = (float)((elapsed * motion.Speed) % 360d);

            return motion.StartRotation * Quaternion.AngleAxis(angle, motion.Direction.normalized);
        }

        public void SetMotion(float newSpeed, Vector3 newDirection)
        {
            if (!IsSpawned || !IsServer || _motion.Value.StartTime < 0d)
            {
                return;
            }

            double now = NetworkManager.ServerTime.Time;
            Quaternion currentRotation = RotationAt(_motion.Value, now);

            // Update values for inspector
            _speed = newSpeed;
            _direction = newDirection;

            _motion.Value = new MotionState
            {
                StartTime = now,
                StartRotation = currentRotation,
                Speed = newSpeed,
                Direction = newDirection
            };
        }

        public void SetSpeed(float newSpeed) => SetMotion(newSpeed, _motion.Value.Direction);

        public void SetDirection(Vector3 newDirection) => SetMotion(_motion.Value.Speed, newDirection);
    }
}
