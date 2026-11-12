using UnityEngine;

namespace WhoDunBit.Player
{
    public class PlayerCamera : PlayerBehaviour
    {
        [Header("Eye Smoothing")]
        [SerializeField, Min(0f)] private float _smoothTime = 0.08f;
        [SerializeField, Min(0f)] private float _maxEyeLag = 0.04f;

        private Transform _lastEyeTarget;
        private Vector3 _smoothedEyeOffset;

        private void LateUpdate()
        {
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            if (!Master || !Master.IsLocallyControlled || !Master.Avatar || !Master.Movement)
            {
                _lastEyeTarget = null;
                return;
            }

            Transform target = Master.Avatar.CameraTarget;
            if (!target)
            {
                _lastEyeTarget = null;
                Debug.LogWarning("Cannot update camera position - Avatar's CameraTarget is null!", this);
                return;
            }

            Transform body = Master.Movement.transform;
            Vector3 eyeOffset = body.InverseTransformPoint(target.position);

            if (target != _lastEyeTarget || _smoothTime <= 0f)
            {
                // Start at the correct eye height on spawn or avatar change
                _smoothedEyeOffset = eyeOffset;
                _lastEyeTarget = target;
            }
            else
            {
                float blend = 1f - Mathf.Exp(-Time.deltaTime / _smoothTime);
                _smoothedEyeOffset = Vector3.Lerp(_smoothedEyeOffset, eyeOffset, blend);

                // Large changes pulls the camera along once it reaches this gap
                Vector3 lag = _smoothedEyeOffset - eyeOffset;
                _smoothedEyeOffset = eyeOffset + Vector3.ClampMagnitude(lag, _maxEyeLag);
            }

            transform.position = body.TransformPoint(_smoothedEyeOffset);
        }
    }
}
