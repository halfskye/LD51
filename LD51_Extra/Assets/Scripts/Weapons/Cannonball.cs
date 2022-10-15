using System.Linq;
using DarkTonic.PoolBoss;
using OldManAndTheSea.Utilities;
using UnityEngine;

namespace OldManAndTheSea.Weapons
{
    public class Cannonball : MonoBehaviour
    {
        private Renderer[] _renderers = null;
        private Rigidbody _rigidbody = null;

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

        private void OnCollisionEnter(Collision collision)
        {
            DebugLog("Cannonball hit something!");
        }

        private void Despawn()
        {
            DebugLog("Cannonball is despawning...");
            PoolBoss.Despawn(this.transform);
        }

        private void DebugLog(string message)
        {
            DebugLogUtilities.LogError(DebugLogUtilities.DebugLogType.WEAPONS, message, this);
        }
    }
}