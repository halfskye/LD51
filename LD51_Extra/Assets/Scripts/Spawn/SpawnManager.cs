using System;
using DarkTonic.PoolBoss;
using OldManAndTheSea.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace OldManAndTheSea.Spawn
{
    public class SpawnManager : SingletonMonoBehaviour<SpawnManager>
    {
        [SerializeField] private SpawnManagerSettings _settings = null;
        
        [SerializeField] private float _firstSpawnWaitTime = 3f;
        private float _timer = 0f;

        [SerializeField] private float _spawnSpeedOverride = 1f;

        private int _nextObjectIndex = 0;

        private void Awake()
        {
            SelectNextObject();
            _timer = _firstSpawnWaitTime;
        }
        
        private void Update()
        {
            _timer -= Time.deltaTime * _spawnSpeedOverride;
            if (_timer <= 0)
            {
                SpawnNextObject();
                SelectNextObject();
            }
        }

        private void SpawnNextObject()
        {
            PoolBoss.SpawnInPool(_settings.ObjectsSpawnData[_nextObjectIndex].ObjectPrefab, Vector3.zero, Quaternion.identity);
        }

        private void SelectNextObject()
        {
            _nextObjectIndex = Random.Range(0, _settings.ObjectsSpawnData.Count);
            _timer = _settings.ObjectsSpawnData[_nextObjectIndex].RandomSpawnTime;
        }
    }
}