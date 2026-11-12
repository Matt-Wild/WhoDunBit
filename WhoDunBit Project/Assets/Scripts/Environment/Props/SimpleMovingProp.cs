using Unity.Netcode;
using UnityEngine;

namespace WhoDunBit
{
    public class SimpleMovingProp : NetworkBehaviour
    {
        [Header("Movement Points")]
        [SerializeField] private Transform _point1;
        [SerializeField] private Transform _point2;

        [Header("Movement")]
        [SerializeField] private Transform _target;
        [SerializeField] private float _speed = 1f;
        [SerializeField] private float _reachedThreshold = 0.005f;

        private void Awake()
        {
            // Assume point 1 is first target if target isn't assigned
            if (!_target)
            {
                _target = _point1;
            }
        }

        private void Update()
        {
            // Movement is server authoritative
            if (!IsServer)
            {
                return;
            }

            // Check if reached target
            if ((transform.position - _target.position).sqrMagnitude < _reachedThreshold * _reachedThreshold)
            {
                // Switch target
                _target = _target == _point1 ? _point2 : _point1;
            }

            // Move towards target
            transform.position = Vector3.MoveTowards(transform.position, _target.position, _speed * Time.deltaTime);
        }
    }
}
