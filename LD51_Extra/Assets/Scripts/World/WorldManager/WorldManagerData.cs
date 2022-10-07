using OldManAndTheSea.Utilities;
using UnityEngine;

namespace OldManAndTheSea.World
{
    public class WorldManagerData
    {
        public Vector3 Sea_Left_Front { get; private set; }
        public Vector3 Sea_Right_Front { get; private set; }
        public Vector3 Sea_Left_Back { get; private set; }
        public Vector3 Sea_Right_Back { get; private set; }

        public Vector3 EastToWest_Sea_Back { get; private set; }
        public Vector3 WestToEast_Sea_Back { get; private set; }
        public Vector3 EastToWest_Normalized { get; private set; }
        public Vector3 WestToEast_Normalized { get; private set; }

        public Vector3 Sea_Middle_Bottom { get; private set; }
        public Vector3 Sea_Middle_Top { get; private set; }

        public Vector3 Sea_Left { get; private set; }
        public Vector3 Sea_Right { get; private set; }
        public Vector3 Sea_Forward { get; private set; }
        public Vector3 Sea_Up { get; private set; }
        
        public float SeaToSkyRatio { get; private set; }
        public float SeaScreenHeight { get; private set; }

        public void SetupFromSettings(WorldManagerSettings settings)
        {
            var c = ScreenUtilities.Camera;
            
            var cameraZ = c.transform.position.z;
            
            var leftFront = c.ViewportToWorldPoint(new Vector3(0f, 0f, 0f - cameraZ)); // new Vector3(-seaFarWidthHalf, 0f, 0f);
            var rightFront = c.ViewportToWorldPoint(new Vector3(1f, 0f, 0f - cameraZ)); // new Vector3(-seaFarWidthHalf, 0f, 0f);

            var seaToSkyRatio = settings.SeaToSkyRatio;
            var seaWorld = c.ViewportToWorldPoint(new Vector3(0f, settings.SeaToSkyRatio, 0f));

            var seaDepth = settings.SeaDistanceFromCamera;
            
            var leftBack = c.ViewportToWorldPoint(new Vector3(0f, seaToSkyRatio, seaDepth - cameraZ)); // new Vector3(-seaFarWidthHalf, seaWorldHeight, seaDepth);
            var rightBack = c.ViewportToWorldPoint(new Vector3(1f, seaToSkyRatio, seaDepth - cameraZ)); // new Vector3(seaFarWidthHalf, seaWorldHeight, seaDepth);

            Sea_Left_Front = leftFront;
            Sea_Right_Front = rightFront;
            Sea_Left_Back = leftBack;
            Sea_Right_Back = rightBack;
            
            WestToEast_Sea_Back = Sea_Right_Back - Sea_Left_Back;
            EastToWest_Sea_Back = -WestToEast_Sea_Back;
            EastToWest_Normalized = EastToWest_Sea_Back.normalized;
            WestToEast_Normalized = -EastToWest_Normalized;

            Sea_Middle_Bottom = Vector3.Lerp(Sea_Left_Front, Sea_Right_Front, 0.1f);
            Sea_Middle_Top = Vector3.Lerp(Sea_Left_Back, Sea_Right_Back, 0.1f);

            Sea_Right = WestToEast_Normalized;
            Sea_Left = -Sea_Right;
            Sea_Forward = (Sea_Middle_Top - Sea_Middle_Bottom).normalized;
            Sea_Up = Vector3.Cross(Sea_Left, Sea_Forward);

            SeaToSkyRatio = settings.SeaToSkyRatio;
            SeaScreenHeight = ScreenUtilities.ScreenHeight * SeaToSkyRatio;
        }
        
        public Vector3 CoordinatesToWorldPoint(Vector2 coordinates)
        {
            var yWorld = WorldYAtCoordY(coordinates.y);
            var xWorld = WorldXAtCoord(coordinates);
            var zWorld = WorldZAtCoordY(coordinates.y);

            return new Vector3(xWorld, yWorld, zWorld);
        }

        private float WorldYAtCoordY(float coordY)
        {
            var heightDelta = Sea_Left_Back.y - Sea_Left_Front.y;
            return heightDelta * coordY + Sea_Left_Front.y;
        }

        private float WorldXAtCoord(Vector2 coordinates)
        {
            var topWidth = Sea_Right_Back.x - Sea_Left_Back.x;
            var bottomWidth = Sea_Right_Front.x - Sea_Left_Front.x;
            var widthDelta = topWidth - bottomWidth;
            var widthAtY = coordinates.y * widthDelta + bottomWidth;
            var xAtY = coordinates.y * (Sea_Left_Back.x - Sea_Left_Front.x) + Sea_Left_Front.x;
            
            return widthAtY * coordinates.x + xAtY;
        }

        private float WorldZAtCoordY(float coordY)
        {
            var depthDelta = Sea_Left_Back.z - Sea_Left_Front.z;
            return depthDelta * coordY + Sea_Left_Front.z;
        }
        
        //@NOTE: Hangover methods from WorldManager from previous LOG method:
        
        
        // public Vector3 WorldToScreenPosition(Vector3 worldPosition)
        // {
        //     var yLog = Settings.LogDistanceFromCamera(worldPosition.y);
        //     var seaLog = Settings.LogSeaDistanceFromCamera;
        //
        //     var ySeaLogRatio = Mathf.Clamp01(yLog / seaLog);
        //
        //     var ySea = ySeaLogRatio * SeaScreenHeight;
        //     
        //     ////
        //     ///
        //     
        //     // var xRatio = (worldPosition.x - Settings.SeaNearWidth) / (Settings.SeaFarWidth - Settings.SeaNearWidth);
        //     
        //     // var seaYRatio = Mathf.Clamp01(ySea / SeaScreenHeight);
        //     
        //     var seaXWidth = Mathf.Lerp(Settings.SeaNearWidth, Settings.SeaFarWidth, ySeaLogRatio);
        //     
        //     var seaXRatio = worldPosition.x / seaXWidth;
        //     
        //     var xSea = seaXRatio * ScreenWidth;
        //
        //     //@TODO: change Z based on ratios later
        //     return new Vector3(xSea, ySea, 0f);
        // }
        //
        // public Vector3 ScreenToWorldPosition(Vector2 screenPosition)
        // {
        //     var seaScreenHeight = SeaScreenHeight;
        //     var screenYSeaRatio = Mathf.Clamp01(screenPosition.y / seaScreenHeight);
        //
        //     var upVector = Vector3.up * screenPosition.y;
        //     
        //     var seaDistanceFromCamera = Settings.SeaDistanceFromCamera;
        //     // DebugLogError($"seaDistanceFromCamera: {seaDistanceFromCamera}");
        //
        //     var forwardVector = Vector3.up * seaScreenHeight + Vector3.right * seaDistanceFromCamera;
        //
        //     var outwardVector = forwardVector - Vector3.zero;
        //
        //     var projectedVector = Vector3.Project(upVector, outwardVector);
        //
        //     var seaYScreen = projectedVector.y;
        //
        //     var seaY = seaYScreen / seaScreenHeight * seaDistanceFromCamera;
        //     // DebugLogError($"seaY: {seaY}");
        //     
        //     ////
        //     ///
        //     ///
        //     
        //     var seaYRatio = Mathf.Clamp01(seaYScreen / seaScreenHeight);
        //     
        //     var seaXWidth = Mathf.Lerp(Settings.SeaNearWidth, Settings.SeaFarWidth, seaYRatio);
        //     
        //     var screenSeaXRatio = screenPosition.x / ScreenWidth;
        //     
        //     var seaX = screenSeaXRatio * seaXWidth;
        //
        //     //@TODO: change Z based on ratios later
        //     return new Vector3(seaX, seaY, 0f);
        // }
        
        // public Vector3 ScreenToWorldPosition(Vector2 screenPosition)
        // {
        //     // var fov = c.fieldOfView;
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
    }
}