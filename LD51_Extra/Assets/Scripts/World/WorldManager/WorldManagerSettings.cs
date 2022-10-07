using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OldManAndTheSea.World
{
    [CreateAssetMenu(fileName = "WorldManagerSettings", menuName = "OldManAndTheSea/World/New WorldManagerSettings", order = 0)]
    public class WorldManagerSettings : ScriptableObject
    {
        [SerializeField, OnValueChanged("ChangeSettings")] private float _cameraFOV = 60f;
        public float CameraFOV => _cameraFOV;
        
        [SerializeField, OnValueChanged("ChangeSettings"), Range(0f, 1f)] private float _seaToSkyRatio = 0.75f;
        public float SeaToSkyRatio => _seaToSkyRatio;

        [SerializeField, OnValueChanged("ChangeSettings")] private float _seaDistanceFromCamera = 3000f;
        public float SeaDistanceFromCamera => _seaDistanceFromCamera;

        [SerializeField, OnValueChanged("ChangeSettings")] private float _seaNearWidth = 100f;
        public float SeaNearWidth => _seaNearWidth;

        [SerializeField, OnValueChanged("ChangeSettings")] private float _seaFarWidth = 3000f;
        public float SeaFarWidth => _seaFarWidth;

        public event Action<float> OnChangeSettings;
        public void ChangeSettings()
        {
            OnChangeSettings?.Invoke(_cameraFOV);
        }
    }
}