using Unity.Netcode;
using UnityEngine;

namespace WhoDunBit
{
    public class SimpleRotatingProp : NetworkBehaviour
    {
        [Header("Rotation")]
        [SerializeField] private Vector3 _direction;
        [SerializeField] private float _speed;

        private void Update()
        {
            // Rotation is server authoritative
            if (!IsServer)
            {
                return;
            }

            transform.Rotate(_speed * Time.deltaTime * _direction.normalized);
        }
    }
}
