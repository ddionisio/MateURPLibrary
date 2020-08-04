using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

namespace M8.URP {
    public class ShaderFeatureCrossHatchEditor : IShaderFeatureEditor {
        public const string propModeShade = "_ShadeMode";

        public const string keywordShadeGradient = "_SHADE_GRADIENT";

        public enum ShadeMode {
            None,
            Gradient
        }

        public MaterialProperty shadeGradientMap;

        public void Setup(MaterialProperty[] properties) {
            shadeGradientMap = EditorUtils.FindProperty("_ShadeGradientMap", properties);
        }

        public void MaterialChanged(Material material) {
            var shadeMode = (ShadeMode)material.GetFloat(propModeShade);
            switch(shadeMode) {
                case ShadeMode.None:
                    CoreUtils.SetKeyword(material, keywordShadeGradient, false);
                    break;
                case ShadeMode.Gradient:
                    CoreUtils.SetKeyword(material, keywordShadeGradient, true);
                    break;
            }
        }

        public void OnGUI(MaterialEditor materialEditor) {
            var material = materialEditor.target as Material;

            var _shadeMode = (ShadeMode)material.GetFloat(propModeShade);
            var shadeMode = (ShadeMode)EditorGUILayout.EnumPopup("Shade Mode", _shadeMode);
            if(shadeMode != _shadeMode)
                material.SetFloat(propModeShade, (float)shadeMode);

            switch(shadeMode) {
                case ShadeMode.Gradient:
                    shadeGradientMap.textureValue = EditorGUILayout.ObjectField(shadeGradientMap.textureValue, typeof(Texture2D), false) as Texture;
                    break;
            }
        }
    }
}