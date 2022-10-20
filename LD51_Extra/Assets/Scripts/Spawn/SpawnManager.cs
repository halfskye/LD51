using System.Collections.Generic;
using OldManAndTheSea.Utilities;
using UnityEngine;

namespace OldManAndTheSea.Spawn
{
    public class SpawnManager : SingletonMonoBehaviour<SpawnManager>
    {
        [SerializeField] private List<SpawnManagerSettings> _settingsList = null;

        [SerializeField] private float _spawnSpeedOverride = 1f;

        private void Awake()
        {
            _settingsList.ForEach(x => x.Initialize());
        }
        
        private void Update()
        {
            var deltaTime = Time.deltaTime * _spawnSpeedOverride;
            _settingsList.ForEach(x => x.UpdateTime(deltaTime));
        }
    }
}