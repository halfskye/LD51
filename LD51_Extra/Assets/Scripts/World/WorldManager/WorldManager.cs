using System.Collections.Generic;
using OldManAndTheSea.Utilities;
using Pinwheel.Poseidon;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OldManAndTheSea.World
{
    public class WorldManager : SingletonMonoBehaviour<WorldManager>
    {
        [SerializeField] private WorldManagerSettings _worldManagerSettings = null;
        public WorldManagerSettings Settings => _worldManagerSettings;
        public WorldManagerData Data { get; private set; } = new WorldManagerData();

        [SerializeField] private MeshFilter _meshFilter = null;
        
        [SerializeField] private PWater _pWater;

        private Camera c => Camera.main;
        
        public enum Direction
        {
            EAST = 0,
            WEST = 1,
            NORTH = 2,
            SOUTH = 3,
        }
        
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
            
            UpdateData();
            
            SetupWorld();
        }

        private void UpdateData()
        {
            Data.SetupFromSettings(_worldManagerSettings);
        }

        private void SetupWorld()
        {
            var worldOrientation = Vector3.Angle(c.transform.forward, Data.Sea_Forward);
            this.transform.Rotate(Vector3.right, worldOrientation);
            
            UpdateData();

            SetupWater(worldOrientation);
        }

        private void SetupWaterQuad(float worldOrientation)
        {
            //@TEMP: Messing w/ temp material's UV coords
            var uv = _meshFilter.mesh.uv;
            uv[0] = new Vector2(1f, 1f);
            // uv[1] = new Vector2(0f, 1f);
            uv[2] = new Vector2(1f, 0f);
            uv[3] = new Vector2(0f, 0f);
            _meshFilter.mesh.uv = uv;
            
            _meshFilter.transform.Rotate(Vector3.right, -worldOrientation);
            
            _meshFilter.mesh.SetVertices(new List<Vector3>()
            {
                Data.Sea_Left_Front,
                Data.Sea_Right_Front,
                Data.Sea_Left_Back,
                Data.Sea_Right_Back
            });
        }

        private void SetupWater(float worldOrientation)
        {
            SetupWaterQuad(worldOrientation);
            
            SetupPWater(worldOrientation);
            
            // _meshFilter.transform.position -= Data.Sea_Up * 3f;
        }

        private void SetupPWater(float worldOrientation)
        {
            _pWater.transform.Rotate(Vector3.right, -worldOrientation);
            
            _pWater.MeshType = PWaterMeshType.Area;
            _pWater.AreaMeshAnchors = new List<Vector3>()
            {
                Data.Sea_Left_Front,
                Data.Sea_Right_Front,
                Data.Sea_Right_Back,
                Data.Sea_Left_Back,
            };
            // _pWater.MeshType = PWaterMeshType.Area;
            _pWater.GenerateAreaMesh();

            var pWaterTransform = _pWater.transform;
            pWaterTransform.localScale = Settings.PWaterScalar;
            pWaterTransform.localPosition = Settings.PWaterPosition;
        }

        public float GetWaterHeightAtPosition(Vector3 position)
        {
            Vector3 localPos = _pWater.transform.InverseTransformPoint(position);
            localPos.y = 0;
            localPos = _pWater.GetLocalVertexPosition(localPos, true);
            Vector3 worldPos = _pWater.transform.TransformPoint(localPos);
            float waterHeight = worldPos.y;
            return waterHeight;
        }

        public Vector3 CoordinatesToWorldPoint(Vector2 coordinates)
        {
            return Data.CoordinatesToWorldPoint(coordinates);
        }

        public void Translate(float velocity)
        {
            if (!Mathf.Approximately(velocity, 0f))
            {
                // this.transform.Translate(Data.Sea_Right * velocity, Space.World);
                this.transform.position += Data.Sea_Right * velocity;

                UpdateData();
            }
        }
        
        private void DebugLogError(string message, Object context=null)
        {
            context = context != null ? context : this;
            DebugLogUtilities.LogError(DebugLogUtilities.DebugLogType.WORLD_MANAGER, message, context);
        }

        private void OnDrawGizmosSelected()
        {
            var seaUp = Data.Sea_Up;

            var cornerColor = Color.magenta;
            var cornerRadius = 5f;
            DebugExtension.DrawCircle(Data.Sea_Left_Front, seaUp, cornerColor, cornerRadius);
            DebugExtension.DrawCircle(Data.Sea_Right_Front, seaUp, cornerColor, cornerRadius);
            DebugExtension.DrawCircle(Data.Sea_Left_Back, seaUp, cornerColor, cornerRadius);
            DebugExtension.DrawCircle(Data.Sea_Right_Back, seaUp, cornerColor, cornerRadius);

            var middleColor = Color.red;
            var middleRadius = 5f;
            DebugExtension.DrawCircle(Data.Sea_Middle_Bottom, seaUp, middleColor, middleRadius);
            DebugExtension.DrawCircle(Data.Sea_Middle_Top, seaUp, middleColor, middleRadius);

            var rayColor = Color.red;
            var rayLength = 0.25f;
            Gizmos.color = rayColor;
            Gizmos.DrawRay(Data.Sea_Left_Back, Data.WestToEast_Sea_Back * rayLength);
            Gizmos.DrawRay(Data.Sea_Right_Back, Data.EastToWest_Sea_Back * rayLength);
            // DebugExtension.DrawArrow(Data.Sea_Left_Back, Data.WestToEast_Sea_Back * rayLength, rayColor);
            // DebugExtension.DrawArrow(Data.Sea_Right_Back, Data.EastToWest_Sea_Back * rayLength, rayColor);
        }
    }
}