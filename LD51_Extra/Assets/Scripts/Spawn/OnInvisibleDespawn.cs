using DarkTonic.PoolBoss;
using Sirenix.Utilities;
using UnityEngine;

namespace OldManAndTheSea.Spawn
{
    [RequireComponent(typeof(Renderer))]
    public class OnInvisibleDespawn : MonoBehaviour
    {
        private void OnBecameInvisible()
        {
            var poolInfos = this.GetComponentsInParent<PoolableInfo>();
            poolInfos.ForEach(x => PoolBoss.Despawn(x.transform));
        }
    }
}