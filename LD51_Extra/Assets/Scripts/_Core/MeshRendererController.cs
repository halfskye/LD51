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
        [SerializeField, ListDrawerSettings(Expanded = true, DraggableItems = true)]
        private List<Material> _affectedMaterials = null;

        [SerializeField] private bool _allOtherMaterialsInvisible = false;
        
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
                AlbedoColorData = new ColorData(StandardShaderUtils.GetAlbedoColor(material));
                SpecularColorData = new ColorData(StandardShaderUtils.GetSpecularColor(material));

                OriginalMode = StandardShaderUtils.GetBlendModeValue(material);
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
            var sharedMaterials = _meshRenderer.sharedMaterials;
            var materials = _meshRenderer.materials;

            var materialsAndShared = materials.Zip(sharedMaterials, (m, s) => new 
            {
                Material = m, Shared = s,
            });
            materialsAndShared.ForEach(x =>
            {
                var material = x.Material;
                if (_affectedMaterials.Contains(x.Shared))
                {
                    _materialColorMap.Add(material, new MaterialColorData(material));
                }
                else if(_allOtherMaterialsInvisible)
                {
                    SetMaterialToInvisible(material);
                }
            });

            _state = State.IDLE;
            _turnOnSpeed = 1f / _turnOnTime;
            _turnOffSpeed = 1f / _turnOffTime;
        }

        private void SetMaterialToInvisible(Material material)
        {
            StandardShaderUtils.ChangeRenderMode(material, StandardShaderUtils.BlendMode.Transparent);
            StandardShaderUtils.SetAlbedoColor(material, StandardShaderUtils.InvisibleColor);
            StandardShaderUtils.SetSpecularColor(material, StandardShaderUtils.InvisibleColor);
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
            var albedoColor = colorData.AlbedoColorData.ActiveColor;
            albedoColor.a += delta;
            var originalAlbedoAlpha = colorData.AlbedoColorData.OriginalColor.a;
            ClampAlphaColor(ref albedoColor, 0f, originalAlbedoAlpha);
            colorData.AlbedoColorData.SetActiveColor(albedoColor);
            StandardShaderUtils.SetAlbedoColor(material, albedoColor);

            var specularColor = colorData.SpecularColorData.ActiveColor;
            specularColor.a += delta;
            ClampAlphaColor(ref specularColor, 0f, colorData.SpecularColorData.OriginalColor.a);
            colorData.SpecularColorData.SetActiveColor(specularColor);
            StandardShaderUtils.SetSpecularColor(material, specularColor);
            
            // Material mode
            var blendModeValue = Mathf.Approximately(albedoColor.a, originalAlbedoAlpha)
                ? colorData.OriginalMode
                :  StandardShaderUtils.GetBlendModeValue(StandardShaderUtils.BlendMode.Transparent);
            
            if(!Mathf.Approximately(blendModeValue, StandardShaderUtils.GetBlendModeValue(material)))
            {
                StandardShaderUtils.ChangeRenderMode(material, StandardShaderUtils.GetBlendMode(blendModeValue));
            }
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