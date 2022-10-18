using UnityEngine;

namespace OldManAndTheSea.Utilities
{
    public static class InterfaceUtils
    {
        public static bool IsNullOrDestroyed(this object anObject) => anObject is Component ? (Object) (anObject as Component) == (Object) null : anObject == null;
    }
}