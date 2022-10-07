using System;
using System.Collections.Generic;
using OldManAndTheSea.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OldManAndTheSea.World
{
    public class WorldManager : SingletonMonoBehaviour<WorldManager>
    {
        [SerializeField] private WorldManagerSettings _worldManagerSettings = null;
        private WorldManagerSettings Settings => _worldManagerSettings;
        public WorldManagerData Data { get; private set; } = new WorldManagerData();

        [SerializeField] private MeshFilter _meshFilter = null;

        private Camera c => Camera.main;
        
        public enum Direction
        {
            EAST = 0,
            WEST = 1,
            NORTH = 2,
            SOUTH = 3,
        }

        // private Vector3 GetNorth(Vector3 position)
        // {
        //     var heightDelta = Sea_Left_Back.y - Sea_Left_Front.y;
        //     var yRatio = (position.y - Sea_Left_Front.y) / heightDelta;
        //
        //     var widthDelta = Mathf.Abs(Sea_Left_Back.x - Sea_Left_Front.x) * 2;
        //     var widthAtY = widthDelta * yRatio;
        //     DebugLogError($"WidthAtY: {widthAtY}");
        //
        //     var xRatio = position.x / widthAtY / 2f;
        //     DebugLogError($"xRatio: {xRatio}");
        //     var baseX = (Sea_Right_Front.x - Sea_Left_Front.x) / 2f * xRatio;
        //
        //     // var depthDelta = LeftBack.z - LeftFront.z;
        //     // var baseZ = depthDelta * yRatio;
        //     
        //     var groundZero = new Vector3(baseX, Sea_Left_Front.y, Sea_Left_Front.z);
        //     
        //     return position - groundZero;
        // }
        // public Vector3 GetNorth_Normalized(Vector3 position)
        // {
        //     return GetNorth(position).normalized;
        // }
        
        protected override void Awake()
        {
            base.Awake();
            
            _worldManagerSettings.OnChangeSettings += OnChangeSettings;

            SetupWorldManagerData();
        }

        private void OnDestroy()
        {
            _worldManagerSettings.OnChangeSettings -= OnChangeSettings;
        }

        private void OnChangeSettings(float obj)
        {
            SetupWorldManagerData();
        }

        // private void Start()
        // {
        //     SetupWorldManagerData();
        // }

        private void SetupWorldManagerData()
        {
            ScreenUtilities.Camera.fieldOfView = Settings.CameraFOV;
            
            Data.SetupFromSettings(_worldManagerSettings);

            SetupWaterQuad();
        }

        private void SetupWaterQuad()
        {
            //@TEMP: Messing w/ temp material's UV coords
            var uv = _meshFilter.mesh.uv;
            uv[0] = new Vector2(1f, 1f);
            // uv[1] = new Vector2(0f, 1f);
            uv[2] = new Vector2(1f, 0f);
            uv[3] = new Vector2(0f, 0f);
            _meshFilter.mesh.uv = uv;
            
            _meshFilter.mesh.SetVertices(new List<Vector3>()
            {
                Data.Sea_Left_Front,
                Data.Sea_Right_Front,
                Data.Sea_Left_Back,
                Data.Sea_Right_Back
            });
        }

        public Vector3 CoordinatesToWorldPoint(Vector2 coordinates)
        {
            return Data.CoordinatesToWorldPoint(coordinates);
        }
        
        private void DebugLogError(string message, Object context=null)
        {
            context = context != null ? context : this;
            DebugLogUtilities.LogError(DebugLogUtilities.DebugLogType.WORLD_MANAGER, message, context);
        }
    }
}