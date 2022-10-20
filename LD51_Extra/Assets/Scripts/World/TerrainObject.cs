using DarkTonic.PoolBoss;
using UnityEngine;

namespace OldManAndTheSea.World
{
    public class TerrainObject : MonoBehaviour
    {
        [SerializeField] private Transform _visual = null;

        private void OnBecameInvisible()
        {
            Despawn();
        }
        
        private void Despawn()
        {
            PoolBoss.Despawn(this.transform);
        }
    }
}