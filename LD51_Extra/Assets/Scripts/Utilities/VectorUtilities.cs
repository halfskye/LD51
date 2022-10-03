using UnityEngine;

namespace OldManAndTheSea.Utilities
{
    public static class VectorUtilities
    {
        public static bool Approximately(this Vector3 a, Vector3 b)
        {
            return Mathf.Approximately(a.x, b.x) &&
                   Mathf.Approximately(a.y, b.y) &&
                   Mathf.Approximately(a.z, b.z);
        }
    }
}