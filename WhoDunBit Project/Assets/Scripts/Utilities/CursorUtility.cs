using UnityEngine;

namespace WhoDunBit.Utilities
{
    public static class CursorUtility
    {
        public static void SetLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}