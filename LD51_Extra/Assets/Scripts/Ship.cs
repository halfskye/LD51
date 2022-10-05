using System;
using OldManAndTheSea.Utilities;
using OldManAndTheSea.World;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OldManAndTheSea
{
    public class Ship : MonoBehaviour
    {
        [Title("Speeds")]
        [SerializeField] private float _movementSpeed = 5f;
        [SerializeField] private float _rotationSpeed = 10f;
        
        [HorizontalGroup("Split", Title = "Starting Coordinates")]
        [SerializeField, BoxGroup("Split/x"), Range(0f,1f), HideLabel] private float xCoord = 0.25f;
        [SerializeField, BoxGroup("Split/y"), Range(0f,1f), HideLabel] private float yCoord = 0.05f;

        private void Start()
        {
            var worldManager = WorldManager.Instance;
            
            //@TEMP: Starting position
            var startPosition = worldManager.CoordinatesToWorldPoint(new Vector2(xCoord, yCoord));
            this.transform.position = startPosition;
            
            // Rotate to align w/ water
            this.transform.rotation = Quaternion.LookRotation(-worldManager.Water_Right, worldManager.Water_Up);
        }

        private void Update()
        {
            UpdateInput();
        }

        private void UpdateInput()
        {
            var axisPosition = Vector3.zero;

            axisPosition.x = Input.GetAxis("Horizontal");
            axisPosition.y = Input.GetAxis("Vertical");

            UpdateMovement(axisPosition);

            if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                LogPosition();
            }
        }

        private void UpdateMovement(Vector3 direction)
        {
            var rotationSpeed = direction.x * _rotationSpeed * Time.deltaTime;
            var rotateBy = Quaternion.Euler(0f, rotationSpeed, 0f);
            var transform1 = this.transform;
            transform1.rotation *= rotateBy;

            var movementSpeed = Mathf.Max(0f, direction.y * _movementSpeed * Time.deltaTime);
            transform1.position += transform1.forward * movementSpeed;
        }
        
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
            DebugLogUtilities.LogError(DebugLogUtilities.DebugLogType.UTILITIES, message, this);
        }
    }
}