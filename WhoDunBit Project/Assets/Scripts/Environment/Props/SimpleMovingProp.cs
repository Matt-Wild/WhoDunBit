using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

namespace WhoDunBit
{
    public class SimpleMovingProp : NetworkBehaviour
    {
        [Header("Movement Points")]
        [SerializeField] private Transform _point1;
        [SerializeField] private Transform _point2;

        [Header("Movement")]
        [SerializeField] private float _speed = 1f;

        private struct MotionState : INetworkSerializeByMemcpy
        {
            public double StartTime;
            public double StartDistance;
            public float Speed;
        }
        private NetworkVariable<MotionState> _motion = new(new MotionState { StartTime = -1d });

        public override void OnNetworkSpawn()
        {
            if (!IsServer)
            {
                return;
            }

            Vector3 from = _point2.position;
            Vector3 path = _point1.position - from;
            float length = path.magnitude;
            if (length < 0.0001f)
            {
                return;
            }

            float startDistance = Mathf.Clamp(Vector3.Dot(transform.position - from, path / length), 0f, length);

            _motion.Value = new MotionState
            {
                StartDistance = startDistance,
                StartTime = NetworkManager.ServerTime.Time,
                Speed = _speed
            };
        }

        private void Update()
        {
            if (!IsSpawned || _motion.Value.StartTime < 0d)
            {
                return;
            }

#if UNITY_EDITOR
            // Allows changing speed via inspector
            if (IsServer && _speed != _motion.Value.Speed)
            {
                SetSpeed(_speed);
            }
#endif

            Vector3 from = _point2.position;
            Vector3 to = _point1.position;
            float length = Vector3.Distance(from, to);

            // Ignore if movement points are too close together
            if (length < 0.0001f)
            {
                return;
            }

            // Get distance and cycle based on server time and start time
            double distance = DistanceAt(_motion.Value, NetworkManager.ServerTime.Time);
            double roundTrip = 2d * length;
            double cycle = distance % roundTrip;
            if (cycle < 0d)
            {
                cycle += roundTrip;
            }

            // Distance increases towards point 1, then decreases towards point 2
            float alongPath = (float)(cycle <= length ? cycle : 2d * length - cycle);
            transform.position = Vector3.Lerp(from, to, alongPath / length);
        }

        private static double DistanceAt(MotionState motion, double now)
        {
            double elapsed = System.Math.Max(0d, now - motion.StartTime);
            return motion.StartDistance + elapsed * motion.Speed;
        }

        public void SetSpeed(float newSpeed)
        {
            if (!IsSpawned || !IsServer || _motion.Value.StartTime < 0d)
            {
                return;
            }

            // Update for inspector
            _speed = newSpeed;

            double now = NetworkManager.ServerTime.Time;
            _motion.Value = new MotionState
            {
                StartDistance = DistanceAt(_motion.Value, now),
                StartTime = now,
                Speed = newSpeed
            };
        }
    }
}
