using DarkTonic.PoolBoss;
using OldManAndTheSea.World;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace OldManAndTheSea.Spawn
{
    [RequireComponent(typeof(Renderer))]
    public class RendererVisibilitySpawnController : MonoBehaviour
    {
        private const float MAX_INVISIBLE_TIME = 30f;

        private const float MAX_DEPTH_BELOW_SEA = 20f;
        private const float MAX_DEPTH_BELOW_SEA_SQR = MAX_DEPTH_BELOW_SEA*MAX_DEPTH_BELOW_SEA;
        
        public bool HasBecomeVisible { get; set; } = false;
        private float _invisibleTimer = 0f;

        [RequiredIn(PrefabKind.Variant)]
        private Renderer[] _renderers = null;
        
        private void Awake()
        {
            _renderers = this.GetComponentsInChildren<Renderer>();
        }

        private void OnSpawn()
        {
            HasBecomeVisible = false;
            _invisibleTimer = 0f;
        }
        
        private void OnBecameVisible()
        {
            HasBecomeVisible = true;
            
            // var ships = this.GetComponentsInParent<OnVisibleSpawnTracker>();
            // ships.ForEach(x => x.OnBecameVisible());
        }
        
        private void OnBecameInvisible()
        {
            Despawn();
        }

        private void Update()
        {
            UpdateInvisibleSafetyCheck();
        }

        private void UpdateInvisibleSafetyCheck()
        {
            if (!HasBecomeVisible)
            {
                _invisibleTimer += Time.deltaTime;
                if (_invisibleTimer > MAX_INVISIBLE_TIME)
                {
                    Despawn();
                }
            }
            else
            {
                //@TODO: Candidate for throttling:
                _renderers.ForEach(x =>
                {
                    var shouldDespawn = false;
                    
                    if (!GeometryUtility.TestPlanesAABB(WorldManager.Instance.Data.CameraFrustumPlanes, x.bounds))
                    {
                        shouldDespawn = true;
                    }
                    else
                    {
                        var projectToSea = Vector3.ProjectOnPlane(x.transform.position, WorldManager.Instance.Data.Sea_Up);
                        var roughSeaDepthSqr = (x.transform.position - projectToSea).sqrMagnitude;
                        if (roughSeaDepthSqr > MAX_DEPTH_BELOW_SEA_SQR)
                        {
                            shouldDespawn = true;
                        }
                    }

                    if (shouldDespawn)
                    {
                        Despawn();
                    }
                });
            }
        }
        
        private void Despawn()
        {
            var poolInfos = this.GetComponentsInParent<PoolableInfo>();
            poolInfos.ForEach(x => PoolBoss.Despawn(x.transform));
        }
    }
}