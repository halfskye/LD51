using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OldManAndTheSea.World
{
    [CreateAssetMenu(fileName = "WorldManagerSettings", menuName = "OldManAndTheSea/World/New WorldManagerSettings", order = 0)]
    public class WorldManagerSettings : ScriptableObject
    {
        [Title("General & Camera")]
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
        
        [Title("PWater")]
        [SerializeField] private Vector3 _pWaterPosition = new Vector3(0f, -3f, 2.5f);
        public Vector3 PWaterPosition => _pWaterPosition;
        
        [SerializeField] private Vector3 _pWaterScalar = new Vector3(1.3f, 1f, 1.4f);
        public Vector3 PWaterScalar => _pWaterScalar;

        [Title("Terrain Objects")]
        [SerializeField] private Vector3 terrainObjectPosition = new Vector3(0f, -1.5f, 0f);
        public Vector3 TerrainObjectPosition => terrainObjectPosition;
        
        [SerializeField] private Vector3 _terrainObjectScale = new Vector3(1f, 1f, 1f);
        public Vector3 TerrainObjectScale => _terrainObjectScale;

        public event Action<float> OnChangeSettings;
        public void ChangeSettings()
        {
            OnChangeSettings?.Invoke(_cameraFOV);
        }
    }
}