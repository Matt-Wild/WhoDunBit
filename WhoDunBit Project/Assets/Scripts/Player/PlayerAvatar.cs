using UnityEngine;

namespace WhoDunBit.Player
{
    public class PlayerAvatar : PlayerBehaviour
    {
        [Header("Renderers")]
        [SerializeField] private Renderer _headRenderer;

        [Header("Targets")]
        [SerializeField] private Transform _cameraTarget;

        public Transform CameraTarget => _cameraTarget;

        public void UpdateRenderers()
        {
            // Requires master
            if (!Master)
            {
                return;
            }

            // Update head renderer if it exists
            if (_headRenderer)
            {
                _headRenderer.shadowCastingMode = Master.IsLocallyControlled ? UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly : UnityEngine.Rendering.ShadowCastingMode.On;
            }
            else
            {
                Debug.LogWarning("Cannot update head renderer - head renderer not found!", this);
            }

            // Locally controlled body skinned mesh renderers should always update
            if (Master.IsLocallyControlled)
            {
                foreach (SkinnedMeshRenderer renderer in GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    renderer.updateWhenOffscreen = true;
                }
            }
        }
    }
}
