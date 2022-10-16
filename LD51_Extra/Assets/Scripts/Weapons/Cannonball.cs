using System.Linq;
using DarkTonic.PoolBoss;
using OldManAndTheSea.Utilities;
using UnityEngine;

namespace OldManAndTheSea.Weapons
{
    public class Cannonball : MonoBehaviour
    {
        [SerializeField] private float _damage = 10f;
        
        private Renderer[] _renderers = null;
        private Rigidbody _rigidbody = null;

        private Ship _owner = null;
        
        private bool _checkVisibility = false;

        private void Awake()
        {
            _renderers = this.GetComponentsInChildren<Renderer>();
            _rigidbody = this.GetComponent<Rigidbody>();
        }

        private void OnSpawned()
        {
            _checkVisibility = false;
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }
        
        private void Update()
        {   
            // DebugLog("Update");
            if (_checkVisibility && _renderers.All(x => !x.isVisible))
            {
                Despawn();
            }
            _checkVisibility = true;
        }

        public void Fire(Ship owner, Vector3 fireForce)
        {
            _owner = owner;
            _rigidbody.AddForce(fireForce, ForceMode.Impulse);
        }

        private void OnTriggerEnter(Collider other)
        {
            DebugLog($"Cannonball hit something! {other.name} - {other.tag}");
            var owner = other.GetComponentInParent<Ship>();
            if (owner != null && owner != _owner)
            {
                owner.TakeDamage(_damage);
            }
        }

        private void Despawn()
        {
            // DebugLog("Cannonball is despawning...");
            PoolBoss.Despawn(this.transform);
        }

        private void DebugLog(string message)
        {
            DebugLogUtilities.LogError(DebugLogUtilities.DebugLogType.WEAPONS, message, this);
        }
    }
}