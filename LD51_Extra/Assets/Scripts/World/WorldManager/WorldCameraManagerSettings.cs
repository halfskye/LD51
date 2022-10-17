using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OldManAndTheSea.World
{
    [CreateAssetMenu(fileName = "WorldCameraManagerSettings", menuName = "OldManAndTheSea/World/New WorldCameraManagerSettings", order = 0)]
    public class WorldCameraManagerSettings : SerializedScriptableObject
    {
        [Serializable]
        public class CameraEdgeData
        {
            [SerializeField] private bool _useSameFrontAndBackEdge = true;
            public bool UseSameFrontAndBackEdge => _useSameFrontAndBackEdge;
            [SerializeField, PropertyRange(0f, 1f)] private float _frontEdge = 0f;
            public float FrontEdge => _frontEdge;
            [SerializeField, PropertyRange(0f, 1f), HideIf("@_useSameFrontAndBackEdge")] private float _backEdge = 0f;
            public float BackEdge => UseSameFrontAndBackEdge ? FrontEdge : _backEdge;

            [SerializeField] private float _acceleration = 0f;
            public float Acceleration => _acceleration;
            
            public bool IsToLeft => _frontEdge < 0.5f || (_backEdge < 0.5f && !_useSameFrontAndBackEdge);
            public bool IsToRight => _frontEdge > 0.5f || (_backEdge > 0.5f && !_useSameFrontAndBackEdge);

            public void SetToSymmetrical(CameraEdgeData other)
            {
                _acceleration = other.Acceleration;
                _useSameFrontAndBackEdge = other.UseSameFrontAndBackEdge;
                _frontEdge = 1f - other.FrontEdge;
                _backEdge = 1f - other.BackEdge;
            }
        }

        [SerializeField] private bool _isSymmetrical = true;
        public bool IsSymmetrical => _isSymmetrical;

        [SerializeField] private List<CameraEdgeData> _cameraEdgeSettings = null;

        public List<CameraEdgeData> GetCameraEdgeSettings()
        {
            if (IsSymmetrical)
            {
                var cameraEdgeSettings = new List<CameraEdgeData>(_cameraEdgeSettings);
                _cameraEdgeSettings.ForEach(x =>
                {
                    var cameraEdgeData = new CameraEdgeData();
                    cameraEdgeData.SetToSymmetrical(x);
                    cameraEdgeSettings.Add(cameraEdgeData);
                });
                return cameraEdgeSettings;
            }
            else
            {
                return _cameraEdgeSettings;
            }
        }
    }
}