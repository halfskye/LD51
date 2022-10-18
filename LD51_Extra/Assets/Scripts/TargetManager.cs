using System.Collections.Generic;
using System.Linq;
using OldManAndTheSea.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OldManAndTheSea
{
    public class TargetManager : SingletonMonoBehaviour<TargetManager>
    {
        public enum TargetingType
        {
            DEFAULT = 0,
            CAMERA = 1,
        }

        [Title("Configuration")]
        [SerializeField] private TargetingType _targetingType = TargetingType.DEFAULT;

        [SerializeField, Range(0f, 90f)] private float _maxAimAngle = 15f;
        [SerializeField] private bool _aimAngleCollapseYAxis = true;
        [SerializeField] private float _maxDistance = 5f;

        // Debug settings:
        [Title("Debug")]
        [SerializeField] private bool _isDebugModeOn = false;
        [SerializeField, HideIf("@!_isDebugModeOn")] private float _debugUpdateRate = .1f;
        private float _debugUpdateTimer = 0f;
        private LineRenderer _debugLineTarget = null;
        private List<LineRenderer> _debugLineRenderers = null;

        public Transform CenterEyeCamera { get; private set; } = null;

        [Title("Registered Targets")]
        [SerializeField] private List<TargetObject> _registeredTargets;

        protected override void Awake()
        {
            base.Awake();
            
            CenterEyeCamera = Camera.main.transform;
            
            _registeredTargets = new List<TargetObject>();
        }

        //@DEBUG-ONLY
        private void Update()
        {
            UpdateDebugMode();
        }

        public void RegisterTarget(TargetObject target)
        {
            if (!_registeredTargets.Contains(target))
            {
                _registeredTargets.Add(target);
            }
        }

        public void UnregisterTarget(TargetObject target)
        {
            _registeredTargets.Remove(target);
        }

        private List<TargetObject> GetTargets(Vector3 srcPosition, Vector3 srcDirection, int numTargets,
            TargetingType targetingType, TargetObject[] excludes = null)
        {
            if (targetingType == TargetingType.CAMERA)
            {
                srcPosition = CenterEyeCamera.position;
                srcDirection = CenterEyeCamera.forward;
            }

            //@TODO: Probably need to fix this for non-player.
            if (_isDebugModeOn)
            {
                // if (_debugLineTarget.IsNullOrDestroyed())
                // {
                //     LineRendererUtilities.CreateBasicDebugLineRenderer(
                //         lineRenderer: ref _debugLineTarget,
                //         colors: new[] {Color.green, Color.red},
                //         width: 0.02f,
                //         parent: this.transform
                //     );
                // }
                //
                // var positions = new[] {srcPosition, srcPosition + srcDirection.normalized * 2f};
                // _debugLineTarget.SetPositions(positions);
                // _debugLineTarget.enabled = true;
                //
                // DebugDrawPossibleTargets(srcPosition, srcDirection);
            }

            // Validate possible targets first:
            var targetObjects = GetValidRegisteredTargets(srcPosition, srcDirection, excludes);
            SortTargetsByAimAngle(ref targetObjects, srcPosition, srcDirection);

            return targetObjects?.Count <= numTargets ? targetObjects : targetObjects?.GetRange(0, numTargets);
        }

        public List<TargetObject> GetTargets(Vector3 srcPosition, Vector3 srcDirection, int numTargets)
        {
            return GetTargets(srcPosition, srcDirection, numTargets, _targetingType);
        }

        public TargetObject GetTarget(Vector3 srcPosition, Vector3 srcDirection, TargetingType targetingType,
            TargetObject[] excludes = null)
        {
            var targets = GetTargets(srcPosition, srcDirection, 1, targetingType, excludes);
            return targets?.Count > 0 ? targets[0] : null;
        }

        public TargetObject GetTarget(Vector3 srcPosition, Vector3 srcDirection, TargetObject[] excludes = null)
        {
            return GetTarget(srcPosition, srcDirection, _targetingType, excludes);
        }

        public TargetObject GetTarget()
        {
            var targets = GetTargets(
                CenterEyeCamera.position,
                CenterEyeCamera.forward,
                1,
                TargetingType.CAMERA
            );
            return targets?.Count > 0 ? targets[0] : null;
        }

        private List<TargetObject> GetValidRegisteredTargets(Vector3 srcPosition, Vector3 srcDirection,
            TargetObject[] excludes = null)
        {
            var targetObjects = new List<TargetObject>();

            // Validate possible targets first:
            foreach (var target in _registeredTargets)
            {
                if (IsTargetIncluded(target, excludes))
                {
                    var targetPosition = target.GetPosition();

                    if (target.IsValid() &&
                        IsWithinMaxDistance(targetPosition, srcPosition) &&
                        IsWithinMaxAimAngle(targetPosition, srcPosition, srcDirection))
                    {
                        targetObjects.Add(target);
                    }
                }
            }

            return targetObjects;
        }

        private bool IsTargetIncluded(TargetObject target, TargetObject[] excludedTargets)
        {
            return target != null &&
                   target.isActiveAndEnabled &&
                   !target.IsSelf && 
                   (excludedTargets == null || !excludedTargets.Contains(target));
        }

        private void SortTargetsByAimAngle(
            ref List<TargetObject> targetObjects,
            Vector3 srcPosition,
            Vector3 srcDirection)
        {
            if (targetObjects?.Count > 0)
            {
                // Sort by aim angle:
                targetObjects = targetObjects.OrderBy(target =>
                {
                    var toTarget = target.GetPosition() - srcPosition;
                    return Vector3.Angle(toTarget, srcDirection);
                }).ToList();
            }
        }

        private bool IsWithinMaxAimAngle(Vector3 targetPosition, Vector3 srcPosition, Vector3 srcDirection)
        {
            var toTarget = targetPosition - srcPosition;
            if (_aimAngleCollapseYAxis)
            {
                toTarget.y = srcDirection.y = 0f;
            }

            return Vector3.Angle(toTarget, srcDirection) <= _maxAimAngle;
        }

        private bool IsWithinMaxDistance(Vector3 targetPosition, Vector3 srcPosition)
        {
            return (targetPosition - srcPosition).sqrMagnitude <= _maxDistance * _maxDistance;
        }

        #region Debug

        private void DebugLog(string message, Object context=null)
        {
            context = context.IsNullOrDestroyed() ? this : context;
            DebugLogUtilities.LogError(DebugLogUtilities.DebugLogType.CORE, message, context);
        }
        
        private void UpdateDebugMode()
        {
            //@NOTE: Currently only have continuously updating debug visualizations for Camera mode.
            if (_isDebugModeOn)
            {
                if (_targetingType == TargetingType.CAMERA)
                {
                    UpdateDebugMode_TargetingTypeCamera();
                }
            }
            else
            {
                DisableAllDebugLineRenderers();
            }
        }

        private void UpdateDebugMode_TargetingTypeCamera()
        {
            _debugUpdateTimer += Time.deltaTime;
            if (_debugUpdateTimer >= _debugUpdateRate)
            {
                _debugUpdateTimer = 0f;
                DebugDrawPossibleTargets(CenterEyeCamera.position, CenterEyeCamera.forward);
            }
        }

        private void DebugDrawPossibleTargets(Vector3 srcPosition, Vector3 srcDirection)
        {
            var targetObjects = GetValidRegisteredTargets(srcPosition, srcDirection);
            if (targetObjects?.Count > 0)
            {
                _debugLineRenderers ??= new List<LineRenderer>();
                var colors = new Color[] {Color.cyan, Color.blue};
                while (_debugLineRenderers.Count < targetObjects.Count)
                {
                    // _debugLineRenderers.Add(
                    //     LineRendererUtilities.CreateBasicDebugLineRenderer(
                    //         colors: colors,
                    //         width: 0.01f,
                    //         parent: this.transform
                    //     )
                    // );
                }

                for (var i = 0; i < _debugLineRenderers.Count; i++)
                {
                    if (i < targetObjects.Count)
                    {
                        LineRenderer lineRenderer = _debugLineRenderers[i];
                        var positions = new[] {srcPosition, targetObjects[i].GetPosition()};
                        lineRenderer.SetPositions(positions);
                        lineRenderer.enabled = true;
                    }
                    else
                    {
                        _debugLineRenderers[i].enabled = false;
                    }
                }
            }
            else
            {
                DebugLog("No Valid Targets");
                DisableAllDebugLineRenderers();
            }
        }

        private void DisableAllDebugLineRenderers()
        {
            DisablePossibleTargetDebugLineRenderers();

            if (!_debugLineTarget.IsNullOrDestroyed())
            {
                _debugLineTarget.enabled = false;
            }
        }

        private void DisablePossibleTargetDebugLineRenderers()
        {
            if (_debugLineRenderers?.Count > 0)
            {
                foreach (LineRenderer lineRenderer in _debugLineRenderers)
                {
                    lineRenderer.enabled = false;
                }
            }
        }

        #endregion Debug
    }
}