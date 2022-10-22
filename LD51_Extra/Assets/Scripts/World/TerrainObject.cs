using UnityEngine;

namespace OldManAndTheSea.World
{
    public class TerrainObject : MonoBehaviour
    {
        [SerializeField] private Transform _visual = null;

        private void OnSpawned()
        {
            var visualTransform = _visual.transform;
            visualTransform.localScale = WorldManager.Instance.Settings.TerrainObjectScale;
            visualTransform.localPosition = WorldManager.Instance.Settings.TerrainObjectPosition;
        }
    }
}
