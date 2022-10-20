using System;
using System.Collections.Generic;
using DarkTonic.PoolBoss;
using OldManAndTheSea.Utilities;
using OldManAndTheSea.World;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace OldManAndTheSea.Spawn
{
    [CreateAssetMenu(fileName = "SpawnManagerSettings", menuName = "OldManAndTheSea/Spawn/New SpawnManagerSettings", order = 0)]
    public class SpawnManagerSettings : ScriptableObject
    {
        [SerializeField] private bool _useGlobalSpawnTimeRange = false;
        [SerializeField, MinMaxSlider(1f,60f), HideIf("@!_useGlobalSpawnTimeRange")] private Vector2 _spawnTimeRange = new Vector2(1f, 10f);
        public float RandomSpawnTime => Random.Range(_spawnTimeRange.x, _spawnTimeRange.y);
        
        [ListDrawerSettings(DraggableItems = true)]
        [SerializeField] private List<ObjectSpawnData> _objectsSpawnData = null;
        public List<ObjectSpawnData> ObjectsSpawnData => _objectsSpawnData;
        
        [SerializeField] private float _firstSpawnWaitTime = 3f;
        
        private const string STARTING_STATE_TITLE = "Starting State";
        private const float COORDS_EXTENT_LOWER = -0.25f;
        private const float COORDS_EXTENT_UPPER = 1.25f;

        [TitleGroup("@STARTING_STATE_TITLE")]
        [SerializeField, LabelText("Randomize by Range")] private bool _useRandomCoords = true;
        [HorizontalGroup("Coords")]
        [SerializeField, BoxGroup("Coords/x"), HideIf("@_useRandomCoords"), Range(COORDS_EXTENT_LOWER,COORDS_EXTENT_UPPER), HideLabel] private float xCoord = 0.25f;
        [SerializeField, BoxGroup("Coords/y"), HideIf("@_useRandomCoords"), Range(COORDS_EXTENT_LOWER,COORDS_EXTENT_UPPER), HideLabel] private float yCoord = 0.05f;
        [SerializeField, BoxGroup("Coords/x"), HideIf("@!_useRandomCoords"), MinMaxSlider(COORDS_EXTENT_LOWER,COORDS_EXTENT_UPPER), HideLabel] private Vector2 xCoordRandom = Vector2.zero;
        [SerializeField, BoxGroup("Coords/y"), HideIf("@!_useRandomCoords"), MinMaxSlider(COORDS_EXTENT_LOWER,COORDS_EXTENT_UPPER), HideLabel] private Vector2 yCoordRandom = Vector2.zero;
        
        private const float START_ANGLE_EXTENT = 180f;
        [SerializeField, MinMaxSlider(-START_ANGLE_EXTENT, START_ANGLE_EXTENT), LabelText("Starting Rotation Range")] private Vector2 _startRotationRange = Vector2.zero;
        
        private float _timer = 0f;
        private int _nextObjectIndex = 0;

        [Serializable]
        public class ObjectSpawnData
        {
            [SerializeField] private Transform _objectPrefab = null;
            public Transform ObjectPrefab => _objectPrefab;

            [SerializeField, MinMaxSlider(1f,60f)] private Vector2 _spawnTimeRange = new Vector2(1f, 10f);
            public float RandomSpawnTime => Random.Range(_spawnTimeRange.x, _spawnTimeRange.y);
        }

        public void Initialize()
        {
            SelectNextObject();
            _timer = _firstSpawnWaitTime;
        }
        
        public void UpdateTime(float deltaTime)
        {
            _timer -= deltaTime;
            if (_timer <= 0)
            {
                SpawnNextObject();
                SelectNextObject();
            }
        }

        private void SpawnNextObject()
        {
            var worldManager = WorldManager.Instance;
            var coords = Vector2.zero;
            
            // Set position.
            if (_useRandomCoords)
            {
                var x = Mathf.Approximately(xCoordRandom.x, xCoordRandom.y)
                    ? xCoordRandom.x
                    : Random.Range(xCoordRandom.x, xCoordRandom.y);
                
                var y = Mathf.Approximately(yCoordRandom.x, yCoordRandom.y)
                    ? yCoordRandom.x
                    : Random.Range(yCoordRandom.x, yCoordRandom.y);

                DebugLog($"Coords: {x}, {y}");
                
                coords = new Vector2(x, y);
            }
            else
            {
                coords = new Vector2(xCoord, yCoord);
            }

            var position = worldManager.CoordinatesToWorldPoint(coords);

            var startRotation = Random.Range(_startRotationRange.x, _startRotationRange.y);
            var baseRotation = Quaternion.LookRotation(worldManager.Data.Sea_Forward, worldManager.Data.Sea_Up);
            var rotation = baseRotation * Quaternion.Euler(0f, startRotation, 0f);
            
            var objectPrefab = ObjectsSpawnData[_nextObjectIndex].ObjectPrefab;
            if (PoolBoss.PrefabIsInPool(objectPrefab))
            {
                PoolBoss.SpawnInPool(objectPrefab, position, rotation);
            }
            else
            {
                Instantiate(objectPrefab, position, rotation);
            }
        }

        private void SelectNextObject()
        {
            _nextObjectIndex = Random.Range(0, ObjectsSpawnData.Count);
            _timer = _useGlobalSpawnTimeRange ? RandomSpawnTime : ObjectsSpawnData[_nextObjectIndex].RandomSpawnTime;
        }

        private void DebugLog(string message)
        {
            DebugLogUtilities.LogError(DebugLogUtilities.DebugLogType.SPAWN, message, this);
        }
    }
}