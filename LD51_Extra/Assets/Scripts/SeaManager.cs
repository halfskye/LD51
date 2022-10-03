using OldManAndTheSea.Utilities;
using OldManAndTheSea.World;
using UnityEngine;

namespace OldManAndTheSea
{
    public class SeaManager : SingletonMonoBehaviour<SeaManager>
    {
        [SerializeField] private WorldManager _worldManager = null;

        private void DebugLogError(string message, Object context)
        {
            context = context != null ? context : this;
            DebugLogUtilities.LogError(DebugLogUtilities.DebugLogType.SEA_MANAGER, message, context);
        }
    }
}