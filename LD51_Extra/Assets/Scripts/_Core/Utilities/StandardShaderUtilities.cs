using System.Collections.Generic;
using System.Linq;

namespace OldManAndTheSea.Utilities
{
    using UnityEngine;

    public static class StandardShaderUtils
    {
        public enum BlendMode
        {
            Opaque,
            Cutout,
            Fade,
            Transparent
        }
        private static Dictionary<BlendMode, float> BlendModeValues = new Dictionary<BlendMode, float>()
        {
            { BlendMode.Opaque, 0f },
            { BlendMode.Cutout, 1f },
            { BlendMode.Fade, 2f },
            { BlendMode.Transparent, 3f },
        };
        
        private const string MODE_NAME = "_Mode";
        
        private const string ALBEDO_COLOR_NAME = "_Color";
        private const string SPECULAR_COLOR_NAME = "_SpecColor";
        
        public static readonly Color InvisibleColor = new Color(0f, 0f, 0f, 0f);
        
        public static BlendMode GetBlendMode(float blendModeValue)
        {
            return BlendModeValues.First(x => Mathf.Approximately(x.Value, blendModeValue)).Key;
        }

        public static float GetBlendModeValue(BlendMode blendMode)
        {
            return BlendModeValues[blendMode];
        }

        public static float GetBlendModeValue(Material material)
        {
            return material.GetFloat(MODE_NAME);
        }

        public static Color GetAlbedoColor(Material material)
        {
            return material.GetColor(ALBEDO_COLOR_NAME);
        }
        public static void SetAlbedoColor(Material material, Color color)
        {
            material.SetColor(ALBEDO_COLOR_NAME, color);
        }

        public static Color GetSpecularColor(Material material)
        {
            return material.GetColor(SPECULAR_COLOR_NAME);
        }
        public static void SetSpecularColor(Material material, Color color)
        {
            material.SetColor(SPECULAR_COLOR_NAME, color);
        }

        public static void ChangeRenderMode(Material standardShaderMaterial, BlendMode blendMode)
        {
            switch (blendMode)
            {
                case BlendMode.Opaque:
                    standardShaderMaterial.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.One);
                    standardShaderMaterial.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.Zero);
                    standardShaderMaterial.SetInt("_ZWrite", 1);
                    standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                    standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                    standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    standardShaderMaterial.renderQueue = -1;
                    break;
                case BlendMode.Cutout:
                    standardShaderMaterial.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.One);
                    standardShaderMaterial.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.Zero);
                    standardShaderMaterial.SetInt("_ZWrite", 1);
                    standardShaderMaterial.EnableKeyword("_ALPHATEST_ON");
                    standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                    standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    standardShaderMaterial.renderQueue = 2450;
                    break;
                case BlendMode.Fade:
                    standardShaderMaterial.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.SrcAlpha);
                    standardShaderMaterial.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    standardShaderMaterial.SetInt("_ZWrite", 0);
                    standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                    standardShaderMaterial.EnableKeyword("_ALPHABLEND_ON");
                    standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    standardShaderMaterial.renderQueue = 3000;
                    break;
                case BlendMode.Transparent:
                    standardShaderMaterial.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.One);
                    standardShaderMaterial.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    standardShaderMaterial.SetInt("_ZWrite", 0);
                    standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                    standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                    standardShaderMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    standardShaderMaterial.renderQueue = 3000;
                    break;
            }
        }
    }
}