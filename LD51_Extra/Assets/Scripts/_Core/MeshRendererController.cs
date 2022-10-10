using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace OldManAndTheSea.Utilities
{
    public class MeshRendererController : MonoBehaviour
    {
        private const string ALBEDO_COLOR_NAME = "_Color";
        private const string SPECULAR_COLOR_NAME = "_SpecColor";
        
        private const string MODE_NAME = "_Mode";
        private const float MODE_TRANSPARENT_VALUE = 3f;

        public class ColorData
        {
            public Color OriginalColor { get; private set; }
            public Color ActiveColor { get; private set; }

            public ColorData(Color color)
            {
                ActiveColor = OriginalColor = color;
            }

            public void SetActiveColor(Color color)
            {
                ActiveColor = color;
            }
        }
        
        public class MaterialColorData
        {
            public ColorData AlbedoColorData { get; private set; }
            public ColorData SpecularColorData { get; private set; }
            
            public float OriginalMode { get; private set; }
            
            public MaterialColorData(Material material)
            {
                AlbedoColorData = new ColorData(material.GetColor(ALBEDO_COLOR_NAME));
                SpecularColorData = new ColorData(material.GetColor(SPECULAR_COLOR_NAME));

                OriginalMode = material.GetFloat(MODE_NAME);
            }
        }
        
        [SerializeField, Required] private MeshRenderer _meshRenderer = null;
        // private Material[] _materials = null;

        private Dictionary<Material, MaterialColorData> _materialColorMap =
            new Dictionary<Material, MaterialColorData>();

        private const float MAX_TURN_ON_OFF_TIME = 10f;
        [SerializeField, Range(0f, MAX_TURN_ON_OFF_TIME)] private float _turnOnTime = 0f;
        private float _turnOnSpeed = 0f;
        [SerializeField, Range(0f, MAX_TURN_ON_OFF_TIME)] private float _turnOffTime = 0f;
        private float _turnOffSpeed = 0f;
        private float _timer = 0f;

        private float _alpha = 0f;

        private enum State
        {
            IDLE = 0,
            TURN_ON = 1,
            TURN_OFF = 2,
        }
        private State _state = State.IDLE;
        
        private void Awake()
        {
            var materials = _meshRenderer.materials;
            materials.ForEach(x => _materialColorMap.Add(x, new MaterialColorData(x)));

            _state = State.IDLE;
            _turnOnSpeed = 1f / _turnOnTime;
            _turnOffSpeed = 1f / _turnOffTime;
        }

        public void Turn(bool on)
        {
            if (on)
            {
                if (_state != State.TURN_ON)
                {
                    TurnOn();
                }
            }
            else
            {
                if (_state != State.TURN_OFF)
                {
                    TurnOff();
                }
            }
        }
        
        public void TurnOn()
        {
            SetState(State.TURN_ON);
        }

        public void TurnOff()
        {
            SetState(State.TURN_OFF);
        }

        private void SetState(State state)
        {
            _timer = 0;
            _state = state;

            // _alpha = _materials.First().color.a;
        }

        private void Update()
        {
            UpdateState();
        }

        private void UpdateState()
        {
            switch (_state)
            {
                case State.TURN_ON:
                case State.TURN_OFF:
                    UpdateTurnState(_state);
                    break;
                case State.IDLE:
                default:
                    break;
            }
        }
        
        private void UpdateTurnState(State state)
        {
            var turnDirection = state == State.TURN_ON ? 1f : -1f;
            var turnSpeed = state == State.TURN_ON ? _turnOnSpeed : _turnOffSpeed;
            var delta = (turnDirection * turnSpeed * Time.deltaTime);

            _materialColorMap.ForEach(materialData =>
            {
                var (material, colorData) = materialData;
                UpdateMaterial(ref material, ref colorData, delta);
            });
        }

        private void UpdateMaterial(ref Material material, ref MaterialColorData colorData, float delta)
        {
            // material.SetFloat(MODE_NAME, MODE_TRANSPARENT_VALUE);
            StandardShaderUtils.ChangeRenderMode(material, StandardShaderUtils.BlendMode.Transparent);

            var albedoColor = colorData.AlbedoColorData.ActiveColor;
            albedoColor.a += delta;
            ClampAlphaColor(ref albedoColor, 0f, colorData.AlbedoColorData.OriginalColor.a);
            colorData.AlbedoColorData.SetActiveColor(albedoColor);
            material.SetColor(ALBEDO_COLOR_NAME, albedoColor);

            var specularColor = colorData.SpecularColorData.ActiveColor;
            specularColor.a += delta;
            ClampAlphaColor(ref specularColor, 0f, colorData.SpecularColorData.OriginalColor.a);
            colorData.SpecularColorData.SetActiveColor(specularColor);
            material.SetColor(SPECULAR_COLOR_NAME, specularColor);
        }

        private void ClampAlphaColor(ref Color color, float min, float max)
        {
            if (color.a <= min || color.a >= max)
            {
                // _state = State.IDLE;
                color.a = Mathf.Max(min, Mathf.Min(color.a, max));
            }
        }

        // private void UpdateTurnStateOld(State state)
        // {
        //     var turnDirection = state == State.TURN_ON ? 1f : -1f;
        //     var turnSpeed = state == State.TURN_ON ? _turnOnSpeed : _turnOffSpeed;
        //
        //     _alpha += (turnDirection * turnSpeed * Time.deltaTime);
        //     if (_alpha <= 0 || _alpha >= 1f)
        //     {
        //         // _state = State.IDLE;
        //         _alpha = Mathf.Max(0f, Mathf.Min(_alpha, 1f));
        //     }
        //     
        //     DebugLog($"Alpha: {_alpha}, State: {_state}, Name: {this.name}");
        //     
        //     SetAllMaterialsAlpha(_alpha);
        // }
        //
        // private void SetAllMaterialsAlpha(float alpha)
        // {
        //     // _materials.ForEach(x =>
        //     // {
        //     //     // var color = x.color;
        //     //     var color = x.GetColor(alphaColorName);
        //     //     color.a = alpha;
        //     //     x.SetColor(alphaColorName, color);
        //     //     // x.color = color;
        //     // });
        // }

        private void DebugLog(string message)
        {
            DebugLogUtilities.LogError(DebugLogUtilities.DebugLogType.CORE, message, this);
        }
    }
}