using System;
using DarkTonic.PoolBoss;
using OldManAndTheSea.Utilities;
using OldManAndTheSea.World;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace OldManAndTheSea
{
    public class Ship : MonoBehaviour
    {
        [SerializeField] private bool _isPlayer = false;
        
        [Title("Speeds")]
        [SerializeField] private float _movementSpeed = 5f;
        [SerializeField] private float _rotationSpeed = 10f;

        private const float COORDS_EXTENT_LOWER = -0.25f;
        private const float COORDS_EXTENT_UPPER = 1.25f;
        [TitleGroup("@STARTING_COORD_TITLE")]
        [SerializeField, TitleGroup("@STARTING_COORD_TITLE"), LabelText("Use Random")] private bool _useRandomCoords = true;
        [HorizontalGroup("Coords")]
        [SerializeField, BoxGroup("Coords/x"), HideIf("@_useRandomCoords"), Range(COORDS_EXTENT_LOWER,COORDS_EXTENT_UPPER), HideLabel] private float xCoord = 0.25f;
        [SerializeField, BoxGroup("Coords/y"), HideIf("@_useRandomCoords"), Range(COORDS_EXTENT_LOWER,COORDS_EXTENT_UPPER), HideLabel] private float yCoord = 0.05f;
        [SerializeField, BoxGroup("Coords/x"), HideIf("@!_useRandomCoords"), MinMaxSlider(COORDS_EXTENT_LOWER,COORDS_EXTENT_UPPER), HideLabel] private Vector2 xCoordRandom = Vector2.zero;
        [SerializeField, BoxGroup("Coords/y"), HideIf("@!_useRandomCoords"), MinMaxSlider(COORDS_EXTENT_LOWER,COORDS_EXTENT_UPPER), HideLabel] private Vector2 yCoordRandom = Vector2.zero;
        
        private const float RANDOM_ANGLE_EXTENT = 0.5f;
        [SerializeField, MinMaxSlider(-RANDOM_ANGLE_EXTENT, RANDOM_ANGLE_EXTENT)] private Vector2 _randomRotationRange = Vector2.zero;
        private float _randomRotation = 0f;

        private void Start()
        {
            SetInitialPositionAndRotation();
            
            // if (!_useRandomCoords)
            // {
            //     var worldManager = WorldManager.Instance;
            //
            //     //@TEMP: Starting position
            //     var startPosition = worldManager.CoordinatesToWorldPoint(new Vector2(xCoord, yCoord));
            //     this.transform.position = startPosition;
            //
            //     // Rotate to align w/ water
            //     this.transform.rotation = Quaternion.LookRotation(worldManager.Water_Right, worldManager.Water_Up);
            // }
        }

        private void Update()
        {
            var moveAxis = _isPlayer ? UpdateInput() : new Vector2( + _randomRotation,1f);
            
            UpdateMovement(moveAxis);
        }

        private Vector2 UpdateInput()
        {
            var axisPosition = Vector2.zero;

            axisPosition.x = Input.GetAxis("Horizontal");
            axisPosition.y = Input.GetAxis("Vertical");

            UpdateMovement(axisPosition);

            if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                LogPosition();
            }

            return axisPosition;
        }

        private void UpdateMovement(Vector2 direction)
        {
            var shipTransform = this.transform;

            var rotationSpeed = direction.x * _rotationSpeed * Time.deltaTime;
            var rotateBy = Quaternion.Euler(0f, rotationSpeed, 0f);
            shipTransform.rotation *= rotateBy;

            var movementSpeed = Mathf.Max(0f, direction.y * _movementSpeed * Time.deltaTime);
            shipTransform.position += shipTransform.forward * movementSpeed;
        }

        private void OnBecameInvisible()
        {
            PoolBoss.Despawn(this.transform);
        }

        #region SPAWN POOL

        private void OnSpawned()
        {
            SetInitialPositionAndRotation();
        }

        private void OnDespawned()
        {
        }

        private void SetInitialPositionAndRotation()
        {
            var worldManager = WorldManager.Instance;
            var coords = Vector2.zero;

            //@TODO: Set position.
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
            this.transform.position = position;

            var rotation = _isPlayer ? worldManager.Data.WestToEast_Normalized : worldManager.Data.EastToWest_Normalized;
            this.transform.rotation = quaternion.LookRotation(rotation, worldManager.Data.Sea_Up);

            _randomRotation = Random.Range(_randomRotationRange.x, _randomRotationRange.y);
        }
        
        #endregion SPAWN POOL
        
        #region DEBUG
        
        private void LogPosition()
        {
            // var worldPosition = this.transform.position;
            // var screenPosition = Camera.main.WorldToScreenPoint(worldPosition, Camera.MonoOrStereoscopicEye.Mono);
            // var seaWorldPosition = WorldManager.Instance.ScreenToWorldPosition(screenPosition);
            // DebugLog($"Screen Position: {screenPosition}\n" +
            //          $"Sea World Position: {seaWorldPosition}\n" +
            //          $"World Position: {worldPosition}");
            
            var worldPosition = this.transform.position;
            DebugLog($"World Position: {worldPosition}");
        }

        private void DebugLog(string message)
        {
            DebugLogUtilities.LogError(DebugLogUtilities.DebugLogType.SHIP, message, this);
        }
        
        #endregion DEBUG
    }
}