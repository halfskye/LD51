using System.Collections.Generic;
using OldManAndTheSea.Utilities;
using UnityEngine;
using static OldManAndTheSea.World.WorldCameraManagerSettings;

namespace OldManAndTheSea.World
{
    public class WorldCameraManager : MonoBehaviour
    {
        [SerializeField] private WorldCameraManagerSettings _settings = null;

        [SerializeField] private float _drawLength = 3f;

        [SerializeField] private float _dampenScalar = 10f;
        [SerializeField] private float _dampenPow = 2f;
        [SerializeField] private float _acceleratePow = 2f;
        [SerializeField] private float _accelerateScalar = 1f;

        private List<CameraEdgeData> _cameraEdgeSettings = null;

        private CameraEdgeData _activeCameraEdgeData = null;
        private float _previousCameraAcceleration = 0f;
        
        private float _cameraVelocity = 0f;
        private float _cameraAcceleration = 0f;

        private class CameraEdgeRecurseData
        {
            public Vector3 PlayerPosition { get; private set; } = Vector3.zero;

            public float DeltaToMiddle { get; private set; } = 0f;
            
            // public bool IsPlayerToLeft { get; private set; } = false;
            // public bool IsPlayerToRight => !IsPlayerToLeft;

            private CameraEdgeData _cameraEdgeData = null;
            public CameraEdgeData CameraEdgeData => _cameraEdgeData;
            private float _bestDotProduct = float.MaxValue;
            
            public CameraEdgeRecurseData(Vector3 playerPosition, float deltaToMiddle)
            {
                PlayerPosition = playerPosition;
                DeltaToMiddle = deltaToMiddle;
            }

            public void AddCameraEdgeData(CameraEdgeData cameraEdgeData, float dotProduct)
            {
                var dotProductAbs = Mathf.Abs(dotProduct);
                if (dotProductAbs < _bestDotProduct)
                {
                    _bestDotProduct = dotProductAbs;
                    _cameraEdgeData = cameraEdgeData;
                }
            }
        }

        private void Awake()
        {
            _cameraEdgeSettings = _settings.GetCameraEdgeSettings();
        }

        private void Update()
        {
            UpdateFollowCamera();
        }

        private void UpdateFollowCamera()
        {
            var playerPosition = Player.Instance.Position;
            var deltaToMiddle = WorldManager.Instance.Data.Sea_Middle_Middle - playerPosition;
            var deltaToMiddleX = deltaToMiddle.x;
            Debug.DrawRay(WorldManager.Instance.Data.Sea_Middle_Bottom,
                (WorldManager.Instance.Data.Sea_Middle_Top - WorldManager.Instance.Data.Sea_Middle_Bottom) * 1.5f,
                Color.blue
            );
            DebugExtension.DebugArrow(playerPosition, deltaToMiddle * _drawLength, Color.cyan);
            // var isPlayerToLeft = deltaToMiddle > 0f;
            var recurseData = new CameraEdgeRecurseData(playerPosition, deltaToMiddleX);

            CheckCameraEdges_Recurse(ref recurseData, _cameraEdgeSettings);

            var sign = -Mathf.Sign(deltaToMiddleX);
            var cameraEdgeData = recurseData.CameraEdgeData;
            if (cameraEdgeData != null)
            {
                if (cameraEdgeData != _activeCameraEdgeData)
                {
                    // _previousCameraAcceleration = _activeCameraEdgeData?.Acceleration * sign ?? 0f;
                    _previousCameraAcceleration = _cameraAcceleration;
                    _activeCameraEdgeData = cameraEdgeData;

                    DebugLog($"NEW Edge - Previous acceleration: {_previousCameraAcceleration}, Sign {sign}");
                }
                // var accelerationDelta = sign * _activeCameraEdgeData.Acceleration - _previousCameraAcceleration;
                var accelerationDelta = sign * _activeCameraEdgeData.Acceleration - sign * Mathf.Lerp(
                        Mathf.Pow(Mathf.Abs(_previousCameraAcceleration), _acceleratePow),
                        _activeCameraEdgeData.Acceleration,
                        Time.deltaTime / _accelerateScalar
                    );
                _cameraAcceleration += accelerationDelta * Time.deltaTime / Mathf.Pow(deltaToMiddleX, 2f);

                _cameraVelocity += (_cameraAcceleration * Time.deltaTime);
                
                // _previousCameraAcceleration = sign * Mathf.Lerp(
                //     Mathf.Pow(Mathf.Abs(_previousCameraAcceleration), _dampenPow),
                //     _activeCameraEdgeData.Acceleration,
                //     Time.deltaTime / _dampenScalar
                // );
            }
            else
            {
                if (_activeCameraEdgeData != null)
                {
                    // _previousCameraAcceleration = _activeCameraEdgeData.Acceleration * sign;
                    _previousCameraAcceleration = _cameraAcceleration;

                    DebugLog($"NO Edge - Previous acceleration: {_previousCameraAcceleration}, Sign {sign}");
                }
                _activeCameraEdgeData = null;

                if (Mathf.Abs(deltaToMiddleX) > 0.01f &&
                    !(Mathf.Abs(_cameraVelocity) < 0.001f) &&
                    !(Mathf.Abs(_previousCameraAcceleration) < 0.001f) &&
                    !Mathf.Approximately(Mathf.Sign(deltaToMiddleX), Mathf.Sign(_previousCameraAcceleration)))
                {
                    _cameraAcceleration += -_previousCameraAcceleration * Time.deltaTime;
                    // var targetAcceleration = 
                    _previousCameraAcceleration = sign * Mathf.Lerp(
                        Mathf.Pow(Mathf.Abs(_previousCameraAcceleration), _dampenPow),
                        0f,
                        Time.deltaTime / _dampenScalar / Mathf.Pow(deltaToMiddleX, 2f)
                    );

                    DebugLog($"Dampen - Vel: {_cameraVelocity} | Prev Acc: {_previousCameraAcceleration}");
                    
                    // var halfExtent = sign > 0f
                    //     ? WorldManager.Instance.Data.Sea_Right_Middle
                    //     : WorldManager.Instance.Data.Sea_Left_Middle;
                    // var halfDirection = WorldManager.Instance.Data.Sea_Middle_Middle - halfExtent;

                    // _cameraAcceleration += -_previousCameraAcceleration * Time.deltaTime / Mathf.Pow(deltaToMiddleX, 2f);
                    // _cameraAcceleration += -_previousCameraAcceleration * Time.deltaTime / Mathf.Log(deltaToMiddleX / halfDirection.x);
                }
                else
                {
                    _cameraAcceleration = 0f;
                    _cameraVelocity = 0f;
                    _previousCameraAcceleration = 0f;
                }

                _cameraVelocity += (_cameraAcceleration * Time.deltaTime);
                
                // _cameraVelocity = 0f;
                // _cameraAcceleration = 0f;

                // if (Mathf.Approximately(deltaToMiddle, 0f))
                // {
                //     _cameraVelocity = 0f;
                //     _cameraAcceleration = 0f;
                // }
                // else
                // {
                //     if (!Mathf.Approximately(_cameraVelocity, 0f))
                //     {
                //         // var direction = 0f - playerPosition.x;
                //         // if (Mathf.Abs(deltaToMiddle) > Mathf.Epsilon)
                //         {
                //             // var accelerationDelta = deltaToMiddle / Mathf.Abs(_previousCameraAcceleration);
                //             // var accelerationDelta = 0f - _previousCameraAcceleration;
                //             
                //             // _cameraAcceleration += -_cameraVelocity * Time.deltaTime * deltaToMiddle * sign / 1000f;
                //             // _cameraAcceleration += Time.deltaTime * deltaToMiddle / 10f;
                //
                //             _cameraVelocity = deltaToMiddle * Time.deltaTime / 1000f;
                //         }
                //     }
                // }
            }
            
            if (!Mathf.Approximately(_cameraVelocity, 0f))
            {
                WorldManager.Instance.Translate(_cameraVelocity);
            }
        }

        private void CheckCameraEdges_Recurse(
            ref CameraEdgeRecurseData recurseData,
            List<CameraEdgeData> edgesToCheck
        )
        {
            var count = edgesToCheck.Count;
            if (count > 1)
            {
                var halfIndex = count / 2;
                var firstHalfCount = halfIndex;
                var lastHalfCount = count - firstHalfCount;
                CheckCameraEdges_Recurse(ref recurseData, edgesToCheck.GetRange(0,firstHalfCount));
                CheckCameraEdges_Recurse(ref recurseData, edgesToCheck.GetRange(halfIndex, lastHalfCount));
            }
            else
            {
                var edgeToCheck = edgesToCheck[0];
                
                var frontEdgeWorld = WorldManager.Instance.CoordinatesToWorldPoint(
                    new Vector2(edgeToCheck.FrontEdge, 0f)
                );
                var backEdgeWorld = WorldManager.Instance.CoordinatesToWorldPoint(
                    new Vector2(edgeToCheck.BackEdge, 1f)
                );
                var frontToBack = backEdgeWorld - frontEdgeWorld;
                Debug.DrawRay(frontEdgeWorld, frontToBack * _drawLength, Color.green);
                var frontToPlayer = recurseData.PlayerPosition - frontEdgeWorld;
                Debug.DrawRay(frontEdgeWorld, frontToPlayer * _drawLength, Color.yellow);
                // var crossPlayer = Vector3.Cross(frontToBack, frontToPlayer);
                // var crossPlayer = Vector3.Cross(frontToBack, frontToPlayer);
                // var dotPlayer = Vector3.Dot(frontToBack, frontToPlayer);

                var baseVector = edgeToCheck.IsToLeft ? frontToBack : WorldManager.Instance.Data.Sea_Up;
                var curlVector = edgeToCheck.IsToLeft ? WorldManager.Instance.Data.Sea_Up : frontToBack;
                var outward = Vector3.Cross(baseVector, curlVector);
                Debug.DrawRay(
                    Vector3.Lerp(frontEdgeWorld, backEdgeWorld, 0.5f),
                    outward * _drawLength,
                    Color.red
                    );
                var dot = Vector3.Dot(outward, frontToPlayer);
                
                // var value = crossPlayer.x + crossPlayer.y + crossPlayer.z;

                // if (dotPlayer > 0f && recurseData.IsPlayerToLeft ||
                //     dotPlayer < 0f && recurseData.IsPlayerToRight)
                // if(value < 0f && edgeToCheck.IsToLeft ||
                //    value > 0f && edgeToCheck.IsToRight)
                if(dot > 0f)
                // if(dot > 0f && edgeToCheck.IsToLeft ||
                //    dot < 0f && edgeToCheck.IsToRight)
                {
                    recurseData.AddCameraEdgeData(edgeToCheck, dot);
                }
            }
        }
        
        private void DebugLog(string message, Object context=null)
        {
            context = context != null ? context : this;
            DebugLogUtilities.LogError(DebugLogUtilities.DebugLogType.WORLD_MANAGER, message, context);
        }
    }
}