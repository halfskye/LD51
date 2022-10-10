using UnityEngine;

namespace OldManAndTheSea.Utilities
{
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : class
    {
        private static T _instance = default(T);
        public static T Instance => _instance;

        protected virtual void Awake()
        {
            if (_instance != null)
            {
                DebugLogError("SeaManager already exists. There should only be one.", this);
                Destroy(this.gameObject);
                return;
            }

            _instance = this as T;
        }

        private void DebugLogError(string message, Object context)
        {
            context = context != null ? context : this;
            DebugLogUtilities.LogError(DebugLogUtilities.DebugLogType.CORE, message, context);
        }
    }
}