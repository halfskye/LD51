using System;
using OldManAndTheSea.Utilities;
using UnityEngine;

namespace OldManAndTheSea.World
{
    public class WorldPositionTester : MonoBehaviour
    {
        [SerializeField] private float _movementSpeed = 1f;
        [SerializeField] private GameObject _visual = null;
        [SerializeField] private float _visualScale = 0.25f;

        [SerializeField] private bool _useOverride = false;
        [SerializeField] private Vector3 _worldPosition = Vector3.zero;
        
        private void Update()
        {
            UpdatePossibleOverride();
            
            UpdateInput();

            UpdateVisual();
        }

        private void UpdatePossibleOverride()
        {
            if (_useOverride)
            {
                var thisTransform = this.transform;
                if (!thisTransform.position.Approximately(_worldPosition))
                {
                    var screenPosition = WorldManager.Instance.WorldToScreenPosition(_worldPosition);
                    var worldPosition = ScreenUtilities.ScreenToWorldPointMono(screenPosition);
                    worldPosition.z = 0f;
                    thisTransform.position = worldPosition;
                }
            }
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
            this.transform.position += direction * (_movementSpeed * Time.deltaTime);
        }

        private void UpdateVisual()
        {
            if (_visual != null)
            {
                _visual.transform.localScale = Vector3.one * _visualScale;
            }
        }

        private void LogPosition()
        {
            var worldPosition = this.transform.position;
            var screenPosition = Camera.main.WorldToScreenPoint(worldPosition, Camera.MonoOrStereoscopicEye.Mono);
            var seaWorldPosition = WorldManager.Instance.ScreenToWorldPosition(screenPosition);
            DebugLog($"Screen Position: {screenPosition}\n" +
                     $"Sea World Position: {seaWorldPosition}\n" +
                     $"World Position: {worldPosition}");
        }

        private void DebugLog(string message)
        {
            DebugLogUtilities.LogError(DebugLogUtilities.DebugLogType.UTILITIES, message, this);
        }
    }
}