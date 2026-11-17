using UnityEngine;

namespace WhoDunBit.Surfaces
{
    public class Surface : MonoBehaviour
    {
        [Header("Properties")]
        [SerializeField] private SurfaceData _data;

        public SurfaceData Data => _data;
    }
}
