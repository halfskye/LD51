using System;
using OldManAndTheSea.Utilities;
using UnityEngine;

namespace OldManAndTheSea.World
{
    public class DebugWorldRenderer : MonoBehaviour
    {
        [SerializeField] private bool _useSeaLine = true;
        // [SerializeField] private LineRenderer _seaLine = null;
        [SerializeField] private Color _seaLineColor = Color.blue;
        [SerializeField] private float _seaLineWidth = 0.1f;

        private LineRenderer _seaLine = null;

        private void Start()
        {
            if (_useSeaLine)
            {
                GenerateSeaLine();
            }
        }

        private void Update()
        {
            UpdateSeaLine();
        }
        
        private void GenerateSeaLine()
        {
            _seaLine = CreateDebugLineRenderer(_seaLineWidth, _seaLineColor, "SeaLineRenderer");
            _seaLine.positionCount = 2;
            
            UpdateSeaLinePosition();
        }

        private void UpdateSeaLinePosition()
        {
            var seaHeight = WorldManager.Instance.SeaScreenHeight;
            
            var screenPosition1 = new Vector3(0f, seaHeight);
            var screenPosition2 = new Vector3(Screen.width, seaHeight);
            var worldPosition1 = ScreenUtilities.ScreenToWorldPointMono(screenPosition1);
            worldPosition1.z = 0f;
            var worldPosition2 = ScreenUtilities.ScreenToWorldPointMono(screenPosition2);
            worldPosition2.z = 0f;
            _seaLine.SetPositions(new Vector3[]
            {
                worldPosition1,
                worldPosition2,
            });
        }
        
        private void UpdateSeaLine()
        {
            if (!_useSeaLine) return;
            
            _seaLine.startColor = _seaLine.endColor = _seaLineColor;
            _seaLine.startWidth = _seaLine.endWidth = _seaLineWidth;

            UpdateSeaLinePosition();
        }
        
        // private void UpdateSeaLinePosition()
        // {
        //     var screenPositionY = WorldManager.Instance.Settings.SeaToSkyRatio * Screen.height;
        //     var worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(0f, screenPositionY, 0f));
        //
        //     var seaLinePosition = _seaLine.transform.position;
        //     _seaLine.transform.position = new Vector3(seaLinePosition.x, worldPosition.y, seaLinePosition.z);
        // }

        private LineRenderer CreateDebugLineRenderer(float width, Color color, string name = null)
        {
            name = string.IsNullOrEmpty(name) ? "DebugLineRenderer" : name;
            var lineRendererObject = new GameObject(name);
            lineRendererObject.transform.parent = this.transform;
            LineRenderer lineRenderer = lineRendererObject.AddComponent<LineRenderer>();

            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startWidth = lineRenderer.endWidth = width;
            lineRenderer.startColor = lineRenderer.endColor = color;
            lineRenderer.useWorldSpace = false;
            lineRenderer.enabled = true;

            return lineRenderer;
        }
    }
}