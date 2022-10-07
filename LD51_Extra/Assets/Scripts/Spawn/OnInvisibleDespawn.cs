using DarkTonic.PoolBoss;
using Sirenix.Utilities;
using UnityEngine;

namespace OldManAndTheSea.Spawn
{
    [RequireComponent(typeof(Renderer))]
    public class OnInvisibleDespawn : MonoBehaviour
    {
        private void OnBecameVisible()
        {
            var ships = this.GetComponentsInParent<Ship>();
            ships.ForEach(x => x.OnBecameVisible());
        }
        
        private void OnBecameInvisible()
        {
            var poolInfos = this.GetComponentsInParent<PoolableInfo>();
            poolInfos.ForEach(x => PoolBoss.Despawn(x.transform));
        }
    }
}