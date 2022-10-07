using UnityEngine;

namespace OldManAndTheSea.World
{
    public class DebugTargetFOV : MonoBehaviour
    {
        //@TEMP:
        [SerializeField] private Transform target = null;
        [SerializeField] private float targetFov = 60f;

        private Camera c => Camera.main;
        
        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;

            // draw line and sphere towards object, real fov
            Gizmos.DrawWireSphere(target.position, 0.02f);
            Gizmos.DrawLine(target.position, c.transform.position);

            Gizmos.color = Color.red;

            float distance = c.nearClipPlane;
            float halfFOV = c.fieldOfView * 0.5f * Mathf.Deg2Rad;
            float aspect = c.aspect;

            float height = Mathf.Tan(halfFOV) * distance;
            float width = height * aspect;

            // draw small red spheres for screen perimeter and center
            Gizmos.DrawWireSphere(c.transform.position + c.transform.forward * distance + c.transform.right * width,
                0.015f);
            Gizmos.DrawWireSphere(c.transform.position + c.transform.forward * distance - c.transform.right * width,
                0.015f);
            Gizmos.DrawWireSphere(c.transform.position + c.transform.forward * distance + c.transform.up * height,
                0.015f);
            Gizmos.DrawWireSphere(c.transform.position + c.transform.forward * distance - c.transform.up * height,
                0.015f);
            Gizmos.DrawWireSphere(c.transform.position + c.transform.forward * distance, 0.015f);

            Vector3 targetViewportPoint = c.WorldToViewportPoint(target.position);
            targetViewportPoint.x = targetViewportPoint.x * 2f - 1f;
            targetViewportPoint.y = targetViewportPoint.y * 2f - 1f;

            // draw red sphere, real fov, on near clip plane
            Gizmos.DrawWireSphere(
                c.transform.position + c.transform.forward * distance +
                c.transform.right * width * targetViewportPoint.x + c.transform.up * height * targetViewportPoint.y,
                0.02f);

            ////// this is the magic sauce

            Matrix4x4 normalToScreen = Matrix4x4.Perspective(targetFov, c.aspect, c.nearClipPlane, c.farClipPlane) *
                                       c.worldToCameraMatrix;
            Matrix4x4 modifiedToScreen =
                Matrix4x4.Perspective(c.fieldOfView, c.aspect, c.nearClipPlane, c.farClipPlane) * c.worldToCameraMatrix;

            Matrix4x4 m = Matrix4x4.Inverse(modifiedToScreen) * normalToScreen;

            Vector3 pos = m.MultiplyPoint(target.position);
            Gizmos.color = Color.magenta;

            // draw line and sphere towards object, desired fov
            Gizmos.DrawLine(c.transform.position, pos);
            Gizmos.DrawWireSphere(pos, 0.02f);

            targetViewportPoint = c.WorldToViewportPoint(pos);
            targetViewportPoint.x = targetViewportPoint.x * 2f - 1f;
            targetViewportPoint.y = targetViewportPoint.y * 2f - 1f;

            // draw white sphere, desired fov, on near clip plane
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(
                c.transform.position + c.transform.forward * distance +
                c.transform.right * width * targetViewportPoint.x + c.transform.up * height * targetViewportPoint.y,
                0.02f);
        }
    }
}