using System;
using OldManAndTheSea.Utilities;
using OldManAndTheSea.World;
using UnityEngine;

namespace OldManAndTheSea
{
    public class Ship : MonoBehaviour
    {
        [SerializeField] private float _movementSpeed = 5f;

        [SerializeField] private Vector2 _startingCoordinates = new Vector2(0.25f, 0.05f);

        private void Start()
        {
            //@TEMP: Starting position
            var startPosition = WorldManager.Instance.CoordinatesToWorldPoint(_startingCoordinates);
            this.transform.position = startPosition;
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
            var position = this.transform.position;
            var north = WorldManager.Instance.GetNorth_Normalized(position);
            var west = WorldManager.Instance.EastToWest_Normalized;
            
            var northMove = north * direction.y;
            var westMove = west * direction.x;

            var northWestMove = (northMove + westMove).normalized;

            position += northWestMove * (_movementSpeed * Time.deltaTime);
            this.transform.position = position;
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