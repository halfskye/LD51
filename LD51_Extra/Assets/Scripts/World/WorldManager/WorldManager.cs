using System.Collections.Generic;
using OldManAndTheSea.Utilities;
using UnityEngine;

namespace OldManAndTheSea.World
{
    public class WorldManager : SingletonMonoBehaviour<WorldManager>
    {
        [SerializeField] private WorldManagerSettings _worldManagerSettings = null;
        public WorldManagerSettings Settings => _worldManagerSettings;

        [SerializeField] private MeshFilter _meshFilter = null;
        [SerializeField] private MeshRenderer _meshRenderer = null;
        
        //@TEMP:
        [SerializeField] private Transform target = null;
        [SerializeField] private float targetFov = 60f;

        //@TODO: Maybe use Screen.safeArea as well ?
        public float ScreenHeight => Screen.height;
        public float ScreenWidth => Screen.width;

        public float SeaScreenHeight => ScreenHeight * Settings.SeaToSkyRatio;

        public Vector3 GroundZero = Vector3.zero;
        
        public enum SeaCorner
        {
            LEFT_FRONT = 0,
            RIGHT_FRONT = 1,
            LEFT_BACK = 2,
            RIGHT_BACK = 3,
        }
        private Vector3[] _seaCorners = new []{ Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, };
        public Vector3[] SeaCorners => _seaCorners;

        public Vector3 LeftFront => SeaCorners[(int) SeaCorner.LEFT_FRONT];
        public Vector3 RightFront => SeaCorners[(int) SeaCorner.RIGHT_FRONT];
        public Vector3 LeftBack => SeaCorners[(int) SeaCorner.LEFT_BACK];
        public Vector3 RightBack => SeaCorners[(int) SeaCorner.RIGHT_BACK];
        
        public enum Direction
        {
            EAST = 0,
            WEST = 1,
            NORTH = 2,
            SOUTH = 3,
        }

        public Vector3 EastToWest_Back => RightBack - LeftBack;
        public Vector3 EastToWest_Normalized => EastToWest_Back.normalized;
        
        public Vector3 GetNorth(Vector3 position)
        {
            var heightDelta = LeftBack.y - LeftFront.y;
            var yRatio = (position.y - LeftFront.y) / heightDelta;

            var widthDelta = Mathf.Abs(LeftBack.x - LeftFront.x) * 2;
            var widthAtY = widthDelta * yRatio;
            DebugLogError($"WidthAtY: {widthAtY}");

            var xRatio = position.x / widthAtY / 2f;
            DebugLogError($"xRatio: {xRatio}");
            var baseX = (RightFront.x - LeftFront.x) / 2f * xRatio;

            // var depthDelta = LeftBack.z - LeftFront.z;
            // var baseZ = depthDelta * yRatio;
            
            var groundZero = new Vector3(baseX, LeftFront.y, LeftFront.z);
            
            return position - groundZero;
        }
        public Vector3 GetNorth_Normalized(Vector3 position)
        {
            return GetNorth(position).normalized;
        }
        
        protected override void Awake()
        {
            base.Awake();

            SetupWaterQuad();
        }

        public Vector3 CoordinatesToWorldPoint(Vector2 coordinates)
        {
            // var leftEdge = LeftBack - LeftFront;
            // var rightEdge = RightBack - RightFront;
            //
            // var heightDelta = LeftBack

            var heightDelta = LeftBack.y - LeftFront.y;
            var yWorld = heightDelta * coordinates.y + LeftFront.y; 
            
            var widthDelta = LeftBack.x - LeftFront.x;
            var widthAtY = coordinates.y * widthDelta;
            
            var xWorld = widthAtY * coordinates.x;

            var depthDelta = LeftBack.z - LeftFront.z;
            var zWorld = depthDelta * coordinates.y + LeftFront.z;

            return new Vector3(xWorld, yWorld, zWorld);
        }
        
        private void Test()
        {
            // Matrix4x4.Perspective()
        }

        private void SetupWaterQuad()
        {
            var vertices = _meshFilter.mesh.vertices;

            var uv = _meshFilter.mesh.uv;
            uv[0] = new Vector2(1f, 1f);
            // uv[1] = new Vector2(0f, 1f);
            uv[2] = new Vector2(1f, 0f);
            uv[3] = new Vector2(0f, 0f);
            _meshFilter.mesh.uv = uv;

            var cameraZ = c.transform.position.z;
            
            // var seaNearWidth = Settings.SeaNearWidth;
            // var seaNearWidthHalf = seaNearWidth / 2f;
            // var seaFarWidth = Settings.SeaFarWidth;
            // var seaFarWidthHalf = seaFarWidth / 2f;

            // var leftFront = new Vector3(-seaFarWidthHalf, 0f, 0f);
            // var rightFront = new Vector3(-seaFarWidthHalf, 0f, 0f);
            var leftFront = c.ViewportToWorldPoint(new Vector3(0f, 0f, 0f - cameraZ)); // new Vector3(-seaFarWidthHalf, 0f, 0f);
            var rightFront = c.ViewportToWorldPoint(new Vector3(1f, 0f, 0f - cameraZ)); // new Vector3(-seaFarWidthHalf, 0f, 0f);

            var seaToSkyRatio = Settings.SeaToSkyRatio;
            // var seaScreenHeight = Settings.SeaToSkyRatio * ScreenHeight;
            var seaWorld = Camera.main.ViewportToWorldPoint(new Vector3(0f, Settings.SeaToSkyRatio, 0f));
            var seaWorldHeight = seaWorld.y;

            var seaDepth = Settings.SeaDistanceFromCamera;
            
            // var leftBack = new Vector3(-seaFarWidthHalf, seaWorldHeight, seaDepth);
            // var rightBack = new Vector3(seaFarWidthHalf, seaWorldHeight, seaDepth);
            var leftBack = c.ViewportToWorldPoint(new Vector3(0f, seaToSkyRatio, seaDepth - cameraZ)); // new Vector3(-seaFarWidthHalf, seaWorldHeight, seaDepth);
            var rightBack = c.ViewportToWorldPoint(new Vector3(1f, seaToSkyRatio, seaDepth - cameraZ)); // new Vector3(seaFarWidthHalf, seaWorldHeight, seaDepth);

            _seaCorners[(int) SeaCorner.LEFT_FRONT] = leftFront;
            _seaCorners[(int) SeaCorner.RIGHT_FRONT] = rightFront;
            _seaCorners[(int) SeaCorner.LEFT_BACK] = leftBack;
            _seaCorners[(int) SeaCorner.RIGHT_BACK] = rightBack;
            
            // _seaCorners = new Vector3[4]
            // {
            //     Vector3.zero,
            //     Vector3.zero,
            //     Vector3.zero,
            //     Vector3.zero
            // };
            
            _meshFilter.mesh.vertices[(int) SeaCorner.LEFT_FRONT] = leftFront;
            _meshFilter.mesh.vertices[(int) SeaCorner.RIGHT_FRONT] = rightFront;
            _meshFilter.mesh.vertices[(int) SeaCorner.LEFT_BACK] = leftBack;
            _meshFilter.mesh.vertices[(int) SeaCorner.RIGHT_BACK] = rightBack;
            
            _meshFilter.mesh.SetVertices(new List<Vector3>()
            {
                leftFront,
                rightFront,
                leftBack,
                rightBack
            });
        }

        // public Vector3 WorldToSeaPosition(Vector3 worldPosition)
        // {
        //     return Vector3.zero;
        // }
        //
        // public Vector3 ScreenToSeaPosition(Vector3 screenPosition)
        // {
        //     
        // }
        
        public Vector3 WorldToScreenPosition(Vector3 worldPosition)
        {
            var yLog = Settings.LogDistanceFromCamera(worldPosition.y);
            var seaLog = Settings.LogSeaDistanceFromCamera;

            var ySeaLogRatio = Mathf.Clamp01(yLog / seaLog);

            var ySea = ySeaLogRatio * SeaScreenHeight;
            
            ////
            ///
            
            // var xRatio = (worldPosition.x - Settings.SeaNearWidth) / (Settings.SeaFarWidth - Settings.SeaNearWidth);
            
            // var seaYRatio = Mathf.Clamp01(ySea / SeaScreenHeight);
            
            var seaXWidth = Mathf.Lerp(Settings.SeaNearWidth, Settings.SeaFarWidth, ySeaLogRatio);
            
            var seaXRatio = worldPosition.x / seaXWidth;
            
            var xSea = seaXRatio * ScreenWidth;

            //@TODO: change Z based on ratios later
            return new Vector3(xSea, ySea, 0f);
        }
        
        public Vector3 ScreenToWorldPosition(Vector2 screenPosition)
        {
            var seaScreenHeight = SeaScreenHeight;
            var screenYSeaRatio = Mathf.Clamp01(screenPosition.y / seaScreenHeight);

            var upVector = Vector3.up * screenPosition.y;
            
            var seaDistanceFromCamera = Settings.SeaDistanceFromCamera;
            // DebugLogError($"seaDistanceFromCamera: {seaDistanceFromCamera}");

            var forwardVector = Vector3.up * seaScreenHeight + Vector3.right * seaDistanceFromCamera;

            var outwardVector = forwardVector - Vector3.zero;

            var projectedVector = Vector3.Project(upVector, outwardVector);

            var seaYScreen = projectedVector.y;

            var seaY = seaYScreen / seaScreenHeight * seaDistanceFromCamera;
            // DebugLogError($"seaY: {seaY}");
            
            ////
            ///
            ///
            
            var seaYRatio = Mathf.Clamp01(seaYScreen / seaScreenHeight);
            
            var seaXWidth = Mathf.Lerp(Settings.SeaNearWidth, Settings.SeaFarWidth, seaYRatio);
            
            var screenSeaXRatio = screenPosition.x / ScreenWidth;
            
            var seaX = screenSeaXRatio * seaXWidth;

            //@TODO: change Z based on ratios later
            return new Vector3(seaX, seaY, 0f);
        }
        
        // public Vector3 ScreenToWorldPosition(Vector2 screenPosition)
        // {
        //     // var fov = Camera.main.fieldOfView;
        //     // screenPosition = ScreenUtilities.GetNormalizedScreenPosition(screenPosition);
        //
        //     var seaScreenHeight = SeaScreenHeight;
        //     var screenYSeaRatio = Mathf.Clamp01(screenPosition.y / seaScreenHeight);
        //
        //     //@TODO: Cache this probably:
        //     var logSeaDistanceFromCamera = Settings.LogSeaDistanceFromCamera;
        //     DebugLogError($"LogSeaDistanceFromCamera: {logSeaDistanceFromCamera}");
        //
        //     var seaDistanceFromCamera = Settings.SeaDistanceFromCamera;
        //     DebugLogError($"seaDistanceFromCamera: {seaDistanceFromCamera}");
        //     
        //     // var yLog = Settings.LogDistanceFromCamera(screenYSeaRatio * seaDistanceFromCamera);
        //     // DebugLogError($"yLog: {yLog}");
        //     // var yLogRatio = yLog / logSeaDistanceFromCamera;
        //     // DebugLogError($"yLogRatio: {yLogRatio}");
        //     //
        //     // var seaY = yLogRatio * seaDistanceFromCamera;
        //     // DebugLogError($"seaY: {seaY}");
        //     
        //     // var seaYLog = screenSeaYRatio * logSeaDistanceFromCamera;
        //     // DebugLogError($"SeaYLog: {seaYLog}");
        //     
        //     var seaY = Settings.PowSeaDistance(seaYLog);
        //     DebugLogError($"seaY: {seaY}");
        //     
        //     var seaYRatio = Mathf.Clamp01(seaY / seaScreenHeight);
        //     
        //     var seaXWidth = Mathf.Lerp(Settings.SeaNearWidth, Settings.SeaFarWidth, seaYRatio);
        //     // var seaXWidthHalf = seaXWidth / 2f;
        //     
        //     // var screenWidthHalf = ScreenWidth / 2f;
        //     // var screenSeaXRatio = screenPosition.x / screenWidthHalf;
        //     var screenSeaXRatio = screenPosition.x / ScreenWidth;
        //
        //     // var seaXHalfRatio = screenPosition.x / seaXWidthHalf;
        //     // var seaX = seaXHalfRatio * seaXWidthHalf;
        //     // var seaX = screenSeaXRatio * seaXWidthHalf;
        //     var seaX = screenSeaXRatio * seaXWidth;
        //
        //     //@TODO: change Z based on ratios later
        //     return new Vector3(seaX, seaY, 0f);
        // }
        
        private void DebugLogError(string message, Object context=null)
        {
            context = context != null ? context : this;
            DebugLogUtilities.LogError(DebugLogUtilities.DebugLogType.SEA_MANAGER, message, context);
        }
        
        private Camera c => Camera.main;
        
        void OnDrawGizmos()
        {
            return;
            
            Gizmos.color = Color.yellow;

            // draw line and sphere towards object, real fov
            Gizmos.DrawWireSphere(target.position, 0.02f);
            Gizmos.DrawLine(target.position, c.transform.position);

            Gizmos.color = Color.red;

            float distance = c.nearClipPlane;
            float halfFOV = c.fieldOfView * 0.5f * Mathf.Deg2Rad;
            float aspect = c.aspect;

            float height = Mathf.Tan(halfFOV) * distance;
            float width = height * aspect;

            // draw small red spheres for screen perimeter and center
            Gizmos.DrawWireSphere(c.transform.position + c.transform.forward * distance + c.transform.right * width,
                0.015f);
            Gizmos.DrawWireSphere(c.transform.position + c.transform.forward * distance - c.transform.right * width,
                0.015f);
            Gizmos.DrawWireSphere(c.transform.position + c.transform.forward * distance + c.transform.up * height,
                0.015f);
            Gizmos.DrawWireSphere(c.transform.position + c.transform.forward * distance - c.transform.up * height,
                0.015f);
            Gizmos.DrawWireSphere(c.transform.position + c.transform.forward * distance, 0.015f);

            Vector3 targetViewportPoint = c.WorldToViewportPoint(target.position);
            targetViewportPoint.x = targetViewportPoint.x * 2f - 1f;
            targetViewportPoint.y = targetViewportPoint.y * 2f - 1f;

            // draw red sphere, real fov, on near clip plane
            Gizmos.DrawWireSphere(
                c.transform.position + c.transform.forward * distance +
                c.transform.right * width * targetViewportPoint.x + c.transform.up * height * targetViewportPoint.y,
                0.02f);

            ////// this is the magic sauce

            Matrix4x4 normalToScreen = Matrix4x4.Perspective(targetFov, c.aspect, c.nearClipPlane, c.farClipPlane) *
                                       c.worldToCameraMatrix;
            Matrix4x4 modifiedToScreen =
                Matrix4x4.Perspective(c.fieldOfView, c.aspect, c.nearClipPlane, c.farClipPlane) * c.worldToCameraMatrix;

            Matrix4x4 m = Matrix4x4.Inverse(modifiedToScreen) * normalToScreen;

            Vector3 pos = m.MultiplyPoint(target.position);
            Gizmos.color = Color.magenta;

            // draw line and sphere towards object, desired fov
            Gizmos.DrawLine(c.transform.position, pos);
            Gizmos.DrawWireSphere(pos, 0.02f);

            targetViewportPoint = c.WorldToViewportPoint(pos);
            targetViewportPoint.x = targetViewportPoint.x * 2f - 1f;
            targetViewportPoint.y = targetViewportPoint.y * 2f - 1f;

            // draw white sphere, desired fov, on near clip plane
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(
                c.transform.position + c.transform.forward * distance +
                c.transform.right * width * targetViewportPoint.x + c.transform.up * height * targetViewportPoint.y,
                0.02f);
        }
    }
}