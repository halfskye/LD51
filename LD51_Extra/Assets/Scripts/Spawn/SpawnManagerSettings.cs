using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace OldManAndTheSea.Spawn
{
    [CreateAssetMenu(fileName = "SpawnManagerSettings", menuName = "OldManAndTheSea/Spawn/New SpawnManagerSettings", order = 0)]
    public class SpawnManagerSettings : ScriptableObject
    {
        [ListDrawerSettings(DraggableItems = true)]
        [SerializeField] private List<ObjectSpawnData> _objectsSpawnData = null;
        public List<ObjectSpawnData> ObjectsSpawnData => _objectsSpawnData;

        [Serializable]
        public class ObjectSpawnData
        {
            [SerializeField] private Transform _objectPrefab = null;
            public Transform ObjectPrefab => _objectPrefab;

            [SerializeField, MinMaxSlider(1f,60f)] private Vector2 _spawnTimeRange = new Vector2(1f, 10f);
            public float RandomSpawnTime => Random.Range(_spawnTimeRange.x, _spawnTimeRange.y);
        }
    }
}