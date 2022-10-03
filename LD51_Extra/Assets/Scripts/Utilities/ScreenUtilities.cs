using UnityEngine;

namespace OldManAndTheSea.Utilities
{
    public static class ScreenUtilities
    {
        public static float ScreenHeight => Screen.height;
        public static float ScreenHeightHalf => ScreenHeight / 2f; 
        public static float ScreenWidth => Screen.width;
        public static float ScreenWidthHalf => ScreenWidth / 2f;

        public static Vector3 ScreenToWorldPointMono(Vector3 screenPosition) =>
            Camera.main.ScreenToWorldPoint(screenPosition, Camera.MonoOrStereoscopicEye.Mono);
        
        public static Vector3 GetNormalizedScreenPosition(Vector3 screenPosition)
        {
            var xNormalized = screenPosition.x + ScreenWidthHalf;
            var yNormalized = screenPosition.y + ScreenHeightHalf;
            
            return new Vector3(xNormalized, yNormalized, screenPosition.z);
        }

        public static Vector3 GetScreenPositionFromNormalized(Vector3 normalized)
        {
            var xReset = normalized.x - ScreenWidthHalf;
            var yReset = normalized.y - ScreenHeightHalf;

            return new Vector3(xReset, yReset, normalized.z);
        }
    }
}