using System;
using OldManAndTheSea.Utilities;
using Sirenix.Utilities;
using UniStorm;
using UnityEngine;

namespace OldManAndTheSea.World
{
    public class SkyboxController : MonoBehaviour
    {
        // [SerializeField] private Material _skyboxMaterial = null;
        [SerializeField] private UniStormSystem _uniStormSystem = null;

        [SerializeField] private Vector3 _rotationAxis = Vector3.up;
        [SerializeField] private float _rotationAngle = 0f;
        [SerializeField] private float _rotationEyeAngle = 0f;

        [SerializeField] private float _cameraHeight = 0.0001f;

        private Material _skyboxMaterial = null;

        private void Awake()
        {
            // var materials = _uniStormSystem.GetComponents<Material>();
            // materials.ForEach(x =>
            // {
            //     DebugLog($"Material: {x.name} - {x.shader.name}");
            // });
        }

        private void Update()
        {
            RenderSettings.skybox.SetFloat("_CameraHeight", _cameraHeight);
            
            RenderSettings.skybox.SetFloat("_Rotation", _rotationAngle);
            RenderSettings.skybox.SetFloat("_RotationEye", _rotationEyeAngle);
            RenderSettings.skybox.SetVector("_RotationAxis", _rotationAxis);
        }

        private void DebugLog(string message)
        {
            DebugLogUtilities.LogError(DebugLogUtilities.DebugLogType.WORLD_MANAGER, message, this);
        }
    }
}