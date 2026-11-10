using UnityEngine;

namespace WhoDunBit.Player
{
    public class PlayerCamera : PlayerBehaviour
    {
        private void LateUpdate()
        {
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            // Requires locally controlled master with avatar
            if (!Master || !Master.IsLocallyControlled || !Master.Avatar)
            {
                return;
            }

            // Check for camera target
            Transform target = Master.Avatar.CameraTarget;
            if (!target)
            {
                Debug.LogWarning("Cannot update camera position - Avatar's CameraTarget is null!", this);
                return;
            }

            // Follow camera target
            transform.position = target.position;
        }
    }
}
